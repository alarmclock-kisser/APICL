using SixLabors.ImageSharp;
using System.Collections.Concurrent;

namespace APICL.Core
{
    public class ImageCollection : IDisposable
    {
        private readonly ConcurrentDictionary<Guid, ImageObj> images = [];
        private readonly object lockObj = new();

        public IReadOnlyCollection<ImageObj> Images => this.images.Values.ToList();

        public ImageObj? this[Guid guid]
        {
            get
            {
                this.images.TryGetValue(guid, out ImageObj? imgObj);
                return imgObj;
            }
        }

        public ImageObj? this[string name]
        {
            get
            {
                lock (this.lockObj)
                {
                    return this.images.Values.FirstOrDefault(img => img.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                }
            }
        }

        public ImageObj? this[int index]
        {
            get
            {
                lock (this.lockObj)
                {
                    return this.images.Values.ElementAtOrDefault(index);
                }
            }
        }

        public bool Add(ImageObj imgObj)
        {
            return this.images.TryAdd(imgObj.Guid, imgObj);
        }

        public bool Remove(Guid guid)
        {
            bool result = this.images.TryRemove(guid, out ImageObj? imgObj);
            if (result && imgObj != null)
            {
                imgObj.Dispose();
                Console.WriteLine($"Removed and disposed image '{imgObj.Name}' (ID: {imgObj.Guid}).");
            }
            else
            {
                Console.WriteLine($"Failed to remove image with ID: {guid}. It might not exist.");
			}

            return result;
		}

        public async Task Clear()
        {
            await Task.Run(() =>
            {
                lock (this.lockObj)
                {
                    foreach (var imgObj in this.images.Values)
                    {
                        imgObj.Dispose();
                        Console.WriteLine($"Disposed image '{imgObj.Name}' (Guid: {imgObj.Guid}).");
                    }

                    this.images.Clear();
                }
            });
		}

        public void Dispose()
        {
            this.Clear().Wait();
            GC.SuppressFinalize(this);
        }

        public async Task<ImageObj?> LoadImage(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                Console.WriteLine($"LoadImage: File not found or path empty: {filePath}");
                return null;
            }

            ImageObj obj;

			try
            {
				obj = await Task.Run(() =>
				{
					lock (this.lockObj)
					{
						return new ImageObj(filePath);
					}
				});
			}
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading image from file '{filePath}': {ex.Message}");
                return null;
			}

			if (this.Add(obj))
            {
                Console.WriteLine($"Loaded and added image '{obj.Name}' (ID: {obj.Guid}) from file.");
                return obj;
            }

			obj.Dispose();
			Console.WriteLine($"Failed to add image '{obj.Name}' (ID: {obj.Guid}). An image with this ID might already exist.");
			return null;
		}

        public async Task<ImageObj?> PopEmpty(Size? size = null, bool add = false)
        {
            size ??= new Size(1080, 1920);
            int number = this.images.Count + 1;
            int digits = (int)Math.Log10(number) + 1;

            ImageObj imgObj;

            try
            {
                imgObj = await Task.Run(() =>
                {
                     lock (this.lockObj)
                    {
                        return new ImageObj(new byte[size.Value.Width * size.Value.Height * 4], size.Value.Width, size.Value.Height, $"EmptyImage_{number.ToString().PadLeft(digits, '0')}");
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating empty image: {ex.Message}");
                return null;
            }

			if (add)
            {
				if (this.Add(imgObj))
				{
					Console.WriteLine($"Created and added empty image '{imgObj.Name}' (ID: {imgObj.Guid}) with size {size.Value.Width}x{size.Value.Height}.");
					return imgObj;
				}

				imgObj.Dispose();
				Console.WriteLine($"Failed to add empty image '{imgObj.Name}' (ID: {imgObj.Guid}). An image with this ID might already exist.");
				return null;
			}

            Console.WriteLine($"Created empty image '{imgObj.Name}' (ID: {imgObj.Guid}) with size {size.Value.Width}x{size.Value.Height}, but not added to collection.");
            return imgObj;
		}



    }
}