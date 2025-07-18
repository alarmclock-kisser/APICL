using System.Diagnostics;
using APICL.Core;
using APICL.OpenCl;
using APICL.Shared;
using Microsoft.AspNetCore.Mvc;
using TextCopy;

namespace APICL.Api.Controllers
{
	[ApiController]
    [Route("api/[controller]")]
    public class OpenClController : ControllerBase
    {
        private readonly OpenClService openClService;
        private readonly ImageCollection imageCollection;
        private readonly AudioCollection audioCollection;
        private readonly IClipboard clipboard;

        private Task<OpenClServiceInfo> status =>
            Task.Run(() => new OpenClServiceInfo(this.openClService));

        private Task<OpenClUsageInfo> usage =>
            Task.Run(() => new OpenClUsageInfo(this.openClService.MemoryRegister));

        public bool FlagReadable { get; set; } = false;

        public OpenClController(OpenClService openClService, ImageCollection imageCollection, AudioCollection audioCollection, IClipboard clipboard)
        {
            this.openClService = openClService;
            this.imageCollection = imageCollection;
            this.audioCollection = audioCollection;
            this.clipboard = clipboard;
        }

        [HttpGet("status")]
        [ProducesResponseType(typeof(OpenClServiceInfo), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<OpenClServiceInfo>> GetStatus()
        {
            try
            {
                return this.Ok(await this.status);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return this.StatusCode(500, ex.Message);
            }
        }

        [HttpGet("devices")]
        [ProducesResponseType(typeof(IEnumerable<OpenClDeviceInfo>), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<IEnumerable<OpenClDeviceInfo>>> GetDevices()
        {
            try
            {
                var infos = await Task.Run(() =>
                {
                    return this.openClService.GetDevices()
                               .Select((device, index) => new OpenClDeviceInfo(this.openClService, index));
                });

                if (!infos.Any())
                {
                    return this.NoContent();
                }

                return this.Ok(infos);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return this.StatusCode(500, ex.Message);
            }
        }

        [HttpPost("devices/{deviceId}/initialize")]
        [ProducesResponseType(typeof(OpenClServiceInfo), 201)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<OpenClServiceInfo>> Initialize(int deviceId = 2)
        {
            try
            {
                int count = this.openClService.Devices.Count;

                if (deviceId < 0 || deviceId >= count)
                {
                    return this.NotFound($"Invalid device ID (max:{count})");
                }

                await Task.Run(() => this.openClService.Initialize(deviceId));

                if (!(await this.status).Initialized)
                {
                    return this.BadRequest("Failed to initialize OpenCL service. Device might not be available.");
                }

                return this.Created($"api/status", await this.status);
            }
            catch (Exception ex)
            {
                return this.StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("dispose")]
        [ProducesResponseType(typeof(OpenClServiceInfo), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<OpenClServiceInfo>> Dispose()
        {
            try
            {
                await Task.Run(() => this.openClService.Dispose());

                if ((await this.status).Initialized)
                {
                    return this.BadRequest("OpenCL service is still initialized. Please ensure it is properly disposed.");
                }

                return this.Ok(await this.status);
            }
            catch (Exception ex)
            {
                return this.StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("usage")]
        [ProducesResponseType(typeof(OpenClUsageInfo), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<OpenClUsageInfo>> GetUsage()
        {
            if (this.openClService.MemoryRegister == null)
            {
                return this.NotFound("OpenCL memory register is not initialized.");
            }

            try
            {
                return this.Ok(await this.usage);
            }
            catch (Exception ex)
            {
                return this.StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("memory")]
        [ProducesResponseType(typeof(IEnumerable<OpenClMemoryInfo>), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<IEnumerable<OpenClMemoryInfo>>> GetMemoryObjects()
        {
            if (this.openClService.MemoryRegister == null)
            {
                return this.NotFound("OpenCL memory register is not initialized.");
            }

            try
            {
                var infos = await Task.Run(() => this.openClService.MemoryRegister.Memory
                    .Select((memory, index) => new OpenClMemoryInfo(memory)));

                if (infos.LongCount() == 0)
                {
                    return this.NoContent();
                }

                return this.Ok(infos);
            }
            catch (Exception ex)
            {
                return this.StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("kernels/{filter}")]
        [ProducesResponseType(typeof(IEnumerable<OpenClKernelInfo>), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<IEnumerable<OpenClKernelInfo>>> GetKernels(string filter = "")
        {
            if (this.openClService.KernelCompiler == null)
            {
                return this.NotFound("OpenCL kernel compiler is not initialized.");
            }
            try
            {
                var infos = await Task.Run(() => this.openClService.KernelCompiler.Files
                    .Select((file, index) => new OpenClKernelInfo(this.openClService.KernelCompiler, index))
                    .Where(info => string.IsNullOrEmpty(filter) || info.FunctionName.Contains(filter, StringComparison.OrdinalIgnoreCase))
					.ToList());

                if (infos.Count <= 0)
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        return this.NotFound($"No kernels with function name found containing '{filter}'");
					}

					return this.NoContent();
                }

                return this.Ok(infos);
            }
            catch (Exception ex)
            {
                return this.StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("executeMandelbrot/{kernel}/{version}/{width}/{height}/{zoom}/{x}/{y}/{coeff}/{r}/{g}/{b}/{copyGuid}/{allowTempSession}")]
        [ProducesResponseType(typeof(ImageObjInfo), 201)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ImageObjInfo>> ExecuteMandelbrot(string kernel = "mandelbrotPrecise", string version = "01",
            int width = 1920, int height = 1080, double zoom = 1.0, double x = 0.0, double y = 0.0, int coeff = 8,
            int r = 0, int g = 0, int b = 0, bool copyGuid = true, bool allowTempSession = true)
        {
            bool temp = false;

            try
            {
                // Get status
                if (!(await this.status).Initialized)
                {
                    if (allowTempSession)
                    {
                        int count = this.openClService.Devices.Count;
                        await Task.Run(() => this.openClService.Initialize(count - 1));
                        temp = true;
                    }

                    if (!(await this.status).Initialized)
                    {
                        return this.BadRequest("OpenCL service is not initialized. Please initialize it first.");
                    }
                }

                // Create an empty image
                var obj = await Task.Run(() => this.imageCollection.PopEmpty(new(width, height), true));
                if (obj == null || !this.imageCollection.Images.Contains(obj))
                {
                    return this.NotFound("Failed to create empty image or couldnt add it to the collection.");
                }

                // Build variable arguments
                object[] variableArgs =
                    [
                        0, 0,
                        width, height,
                        zoom, x, y,
                        coeff,
                        r, g, b
                    ];

                Stopwatch sw = Stopwatch.StartNew();

                // Call service accessor
                var result = await Task.Run(() =>
                    this.openClService.ExecuteImageKernel(obj, kernel, version, variableArgs));

                sw.Stop();

                // Get image obj info
                var info = await Task.Run(() => new ImageObjInfo(obj, null, sw.Elapsed));
                if (!info.OnHost)
                {
                    return this.NotFound(
                        "Failed to execute OpenCL kernel or image is not on the host after execution call.");
                }

                // Optionally copy guid to clipboard
                if (copyGuid)
                {
                    await this.clipboard.SetTextAsync(info.Guid.ToString());
                }

                return this.Created($"api/image/images/{info.Guid}/image64", info);
            }
            catch (Exception ex)
            {
                return this.StatusCode(500, $"Internal server error: {ex.Message}");
            }
            finally
            {
                if (temp)
                {
                    await Task.Run(() => this.openClService.Dispose());
                }
            }
        }

        [HttpGet("executeTimestretch/{guid}/{kernel}/{version}/{factor}/{chunkSize}/{overlap}/{copyGuid}/{allowTempSession}")]

        public async Task<ActionResult<AudioObjInfo>> ExecuteTimestretch(Guid guid, string kernel = "timestretch_double",
            string version = "03", double factor = 0.8d, int chunkSize = 16384, float overlap = 0.5f,
            bool copyGuid = true, bool allowTempSession = true)
        {
            // Find audio obj
            var obj = this.audioCollection[guid];
            if (obj == null || obj.Guid == Guid.Empty)
            {
                return this.NotFound($"No audio object found with Guid '{guid}'");
            }

            bool temp = false;

            // Verify initialized
            try
            {
                if (!(await this.status).Initialized)
                {
                    if (allowTempSession)
                    {
                        int count = this.openClService.Devices.Count;
                        await Task.Run(() => this.openClService.Initialize(count - 1));
                        temp = true;
                    }

                    if (!(await this.status).Initialized)
                    {
                        return this.BadRequest("OpenCL service is not initialized. Please initialize it first.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return this.BadRequest("Error while trying to initialize OpenCL, aborting");
            }

            // Build variable args (factor)
            Dictionary<string, object> optionalArguments = [];
            if (kernel.ToLower().Contains("double"))
            {
                optionalArguments.Add("factor", (double)factor);
            }
            else
            {
                optionalArguments.Add("factor", (float)factor);
            }

            Stopwatch sw = Stopwatch.StartNew();

            try
            {
                var result = await this.openClService.ExecuteAudioKernel(obj, kernel, version, chunkSize, overlap,
                    optionalArguments, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return this.StatusCode(500, "Error: " + ex.Message);
            }
            finally
            {
                sw.Stop();

                if (temp)
                {
                    await Task.Run(() => this.openClService.Dispose());
                }
            }

            return this.Created($"api/audio/audios/{obj.Guid}/info", new AudioObjInfo(obj, null, sw.Elapsed));
        }

    }
}
