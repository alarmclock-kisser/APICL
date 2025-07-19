using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.PixelFormats;

namespace APICL.Core
{
	public class ImageObj : IDisposable
	{
		public Guid Guid { get; private set; }

		public string Filepath { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;

		public Image<Rgba32>? Img { get; set; } = null;
		public int Width { get; set; } = 0;
		public int Height { get; set; } = 0;
		public int Channels { get; set; } = 4;
		public int Bitdepth { get; set; } = 0;

		public long SizeInBytes => this.Width * this.Height * this.Channels * (this.Bitdepth / 8);

		public IntPtr Pointer { get; set; } = IntPtr.Zero;

		public bool OnHost => this.Pointer == IntPtr.Zero && this.Img != null;
		public bool OnDevice => this.Pointer != IntPtr.Zero && this.Img == null;


		private readonly object lockObj = new();


		public ImageObj(string filePath)
		{
			this.Guid = Guid.NewGuid();
			this.Filepath = filePath;
			this.Name = Path.GetFileNameWithoutExtension(filePath);

			try
			{
				using var img = Image.LoadAsync(filePath).GetAwaiter().GetResult();

				lock (this.lockObj)
				{
					this.Img = this.Img?.CloneAs<Rgba32>();
				}

				this.Width = this.Img?.Width ?? 0;
				this.Height = this.Img?.Height ?? 0;
				this.Channels = 4;
				this.Bitdepth = this.Img?.PixelType.BitsPerPixel ?? 0;

				img.Dispose();
				GC.Collect();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error loading image {filePath}: {ex.Message}");
				this.Img = null;
			}
			finally
			{
				Task.Yield();
			}
		}

		public ImageObj(IEnumerable<byte> rawPixelData, int width, int height, string name = "UnbenanntesBild")
		{
			this.Guid = Guid.NewGuid();
			this.Name = name;
			this.Filepath = string.Empty;

			try
			{
				lock(this.lockObj)
				{
					this.Img = Image.LoadPixelData<Rgba32>(rawPixelData.ToArray(), width, height);
				}

				this.Width = this.Img.Width;
				this.Height = this.Img.Height;
				this.Channels = 4;
				this.Bitdepth = this.Img.PixelType.BitsPerPixel;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Fehler beim Erstellen des Bildes aus rohen Daten: {ex.Message}");
				this.Img = null;
			}
		}

		public async Task<string> AsBase64(string format = "bmp")
		{
			if (this.Img == null)
			{
				return string.Empty;
			}

			Image<Rgba32> imgClone;

			lock (this.lockObj)
			{
				imgClone = this.Img.CloneAs<Rgba32>();
			}

			try
			{
				// Get image encoder based on format
				IImageEncoder encoder = format.ToLower() switch
				{
					"png" => new SixLabors.ImageSharp.Formats.Png.PngEncoder(),
					"jpeg" => new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder(),
					"gif" => new SixLabors.ImageSharp.Formats.Gif.GifEncoder(),
					_ => new SixLabors.ImageSharp.Formats.Bmp.BmpEncoder()
				};

				// Save memory stream async
				var ms = new MemoryStream();
				await imgClone.SaveAsync(ms, encoder);
				ms.Position = 0;
				return await Task.Run(() => Convert.ToBase64String(ms.ToArray()));
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Fehler bei der Base64-Konvertierung: {ex.Message}");
				return ex + " (" + ex.InnerException + ").";
			}
		}

		public async Task<IEnumerable<byte>> GetBytes(bool keepImage = false)
		{
			if (this.Img == null)
			{
				return [];
			}

			Image<Rgba32> imgClone;

			lock (this.lockObj)
			{
				imgClone = this.Img.CloneAs<Rgba32>();
			}

			int bytesPerPixel = this.Img.PixelType.BitsPerPixel / 8;
			long totalBytes = this.Width * this.Height * bytesPerPixel;

			byte[] bytes = new byte[totalBytes];

			await Task.Run(() =>
			{
				imgClone.CopyPixelDataTo(bytes);
			});

			if (!keepImage)
			{
				this.Img.Dispose();
				this.Img = null;
			}

			return bytes.AsEnumerable();
		}

		public async Task<Image<Rgba32>?> SetImage(IEnumerable<byte> bytes, bool keepPointer = false)
		{
			if (this.Img != null)
			{
				this.Img.Dispose();
				this.Img = null;
			}

			Image<Rgba32>? img = null;

			try
			{
				img = await Task.Run(() =>
				{
					return Image.LoadPixelData<Rgba32>(bytes.ToArray(), this.Width, this.Height);
				});

				// Lock
				lock (this.lockObj)
				{
					this.Img = img;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error setting image from bytes: {ex.Message}");
				this.Img = null;
				return null;
			}
			finally
			{
				if (!keepPointer)
				{
					this.Pointer = IntPtr.Zero;
				}

				await Task.Yield();
			}

			return img;
		}

		public void Dispose()
		{
			if (this.Img != null)
			{
				this.Img.Dispose();
				this.Img = null;
			}

			this.Pointer = IntPtr.Zero;

			GC.SuppressFinalize(this);
		}

		public async Task<string> Export(string filePath = "", IImageFormat? format = null)
		{
			if (this.Img == null)
			{
				return string.Empty;
			}

			// Fallback to Bmp
			format ??= BmpFormat.Instance;
			string extension = format.FileExtensions.FirstOrDefault()?.Trim('.') ?? "bmp";
			if (extension.ToLower() == "bmp")
			{
				format = BmpFormat.Instance;
			}

			if (string.IsNullOrEmpty(filePath))
			{
				filePath = Path.Combine(Path.GetTempPath(), $"{this.Name}_{DateTime.Now:yyyyMMdd_HHmmss}.{extension}");
			}

			try
			{
				// Clone img in a thread-safe manner
				Image<Rgba32> clone = await Task.Run(() =>
				{
					lock (this.lockObj)
					{
						return this.Img.CloneAs<Rgba32>();
					}
				});

				// Use the clone in an async context
				using (clone)
				{
					// Get encoder from the format
					IImageEncoder encoder = Configuration.Default.ImageFormatsManager.GetEncoder(format);

					// Create the directory if it doesn't exist
					var directory = Path.GetDirectoryName(filePath);
					if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
					{
						Directory.CreateDirectory(directory);
					}

					// Save asynchronously with proper disposal
					await using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
					{
						await clone.SaveAsync(fileStream, encoder);
					}
				}

				return filePath;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error exporting image: {ex.Message}");
				return string.Empty;
			}
		}

		public async Task<byte[]> GetImageAsFileFormatAsync(IImageEncoder? encoder = null)
		{
			if (this.Img == null)
			{
				return [];
			}

			encoder ??= new BmpEncoder();
			using MemoryStream ms = new();
			await this.Img.SaveAsync(ms, encoder);
			return ms.ToArray();
		}

		public override string ToString()
		{
			return $"{this.Width}x{this.Height} px, {this.Channels} ch., {this.Bitdepth} Bits";
		}

	}
}
