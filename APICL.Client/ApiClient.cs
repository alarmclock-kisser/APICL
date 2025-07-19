using APICL.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APICL.Client
{
    public class ApiClient
    {
        private InternalClient internalClient;
        private HttpClient httpClient;

        public ApiClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
            this.internalClient = new InternalClient(httpClient.BaseAddress?.AbsoluteUri ?? "https://localhost:7117", httpClient);
        }


        // OpenCL-Control Tasks
        public async Task<OpenClServiceInfo> GetOpenClServiceInfo()
        {
            var task = this.internalClient.StatusAsync();

            try
            {
                return await task;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return new OpenClServiceInfo(null);
            }
        }

        public async Task<ICollection<OpenClDeviceInfo>> GetOpenClDeviceInfos()
        {
            var task = this.internalClient.DevicesAsync();

            try
            {
                return await task;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return [];
            }
        }

        public async Task<OpenClServiceInfo> InitializeOpenCl(int index = 0)
        {
            var task = this.internalClient.InitializeAsync(index);

            try
            {
                return await task;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return new OpenClServiceInfo(null);
            }
        }

        public async Task<OpenClServiceInfo> DisposeOpenCl()
        {
            var task = this.internalClient.DisposeAsync();

            try
            {
                return await task;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return new OpenClServiceInfo(null);
            }
        }

        public async Task<OpenClUsageInfo> GetOpenClUsageInfo()
        {
            var task = this.internalClient.UsageAsync();

            try
            {
                return await task;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return new OpenClUsageInfo(null);
            }
        }

        public async Task<ICollection<OpenClMemoryInfo>> GetOpenClMemoryInfos()
        {
            var task = this.internalClient.MemoryAsync();

            try
            {
                return await task;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return [];
            }
        }

        public async Task<ICollection<OpenClKernelInfo>> GetOpenClKernelInfos(string filter = "")
        {
            var task = this.internalClient.KernelsAsync(filter);

            try
            {
                return await task;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return [];
            }
        }


        // Image Tasks
        public async Task<ICollection<ImageObjInfo>> GetImageInfos()
        {
            var task = this.internalClient.ImagesAsync();

            try
            {
                return await task;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return [];
            }
        }

        public async Task<ImageObjInfo> GetImageInfo(Guid guid)
        {
            var task = this.internalClient.Info2Async(guid);

            try
            {
                return await task;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return new ImageObjInfo(null);
            }
        }

        public async Task<bool> RemoveImage(Guid guid)
        {
            var task = this.internalClient.Remove2Async(guid);

            try
            {
                return await task;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return false;
            }
        }

        public async Task<ImageObjInfo> AddEmptyImage(int width = 720, int height = 480)
        {
            var task = this.internalClient.EmptyAsync(width, height);

            try
            {
                return await task;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return new ImageObjInfo(null);
            }
        }

        public async Task<ImageObjInfo> UploadImage(FileParameter file, bool copyGuid = false)
        {
            var task = this.internalClient.Upload2Async(copyGuid, file);

            try
            {
                return await task;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return new ImageObjInfo(null);
            }
        }

        public async Task<ImageData> GetBase64Image(Guid guid, string format = "bmp")
        {
            var task = this.internalClient.Image64Async(guid, format);

            try
            {
                return await task;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ImageData();
            }
        }


        // Audio Tasks
        public async Task<ICollection<AudioObjInfo>> GetAudioInfos()
        {
            var task = this.internalClient.AudiosAsync();

            try
            {
                return await task;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return [];
            }
        }

        public async Task<AudioObjInfo> GetAudioInfo(Guid guid)
        {
            var task = this.internalClient.InfoAsync(guid);

            try
            {
                return await task;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return new AudioObjInfo(null);
            }
        }

        public async Task<bool> RemoveAudio(Guid guid)
        {
            var task = this.internalClient.RemoveAsync(guid);

            try
            {
                return await task;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return false;
            }
        }

        public async Task<AudioObjInfo> UploadAudio(FileParameter file, bool copyGuid = false)
        {
            Stopwatch sw = Stopwatch.StartNew();
            var info = new AudioObjInfo(null);
			var task = this.internalClient.UploadAsync(copyGuid, file);

            try
            {
                info = await task;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return new AudioObjInfo(null);
            }
            finally
            {
                sw.Stop();
                info.LastLoadingTimeSpan = sw.Elapsed;
			}

            return info;
		}

        public async Task<AudioData> GetBase64Waveform(Guid guid)
        {
            var task = this.internalClient.Waveform64Async(guid);

            try
            {
                return await task;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new AudioData(null);
            }
        }


        // Kernel Tasks
        public async Task<ImageObjInfo> ExecuteMandelbrot(string kernel = "mandelbrotPrecise", string version = "01",
            int width = 480, int height = 360, double zoom = 1.0, double x = 0.0, double y = 0.0, int coeff = 8,
            int r = 0, int g = 0, int b = 0, bool copyGuid = true, bool allowTempSession = true)
        {
			var task = this.internalClient.ExecuteMandelbrotAsync(kernel, version, width, height, zoom, x, y, coeff, r, g, b, copyGuid, allowTempSession);
            var info = new ImageObjInfo(null);

			try
            {
                info = await task;
			}
            catch (Exception ex)
            {
				info.ErrorInfo = $"'{ex.Message}' ({ex.InnerException?.Message})";
				Console.WriteLine(ex);
			}

            return info;
        }

        public async Task<AudioObjInfo> ExecuteTimestretch(Guid guid, string kernel = "timestretch_double",
            string version = "03", double factor = 0.8, int chunkSize = 16384, float overlap = 0.5f,
            bool copyGuid = true, bool allowTempSession = true)
        {
            Stopwatch sw = Stopwatch.StartNew();

			var info = await this.GetAudioInfo(guid);
			var task = this.internalClient.ExecuteTimestretchAsync(info.Guid, kernel, version, factor, chunkSize, overlap, copyGuid, allowTempSession);

			try
            {
                info = await task;
                info.LastProcessingTimeSpan = sw.Elapsed;
			}
            catch (Exception ex)
            {
                info.ErrorInfo = $"'{ex.Message}' ({ex.InnerException?.Message})";
				Console.WriteLine(ex);
            }
            finally
            {
                sw.Stop();
            }

            return info;
        }


    }
}
