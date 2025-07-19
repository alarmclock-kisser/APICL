using APICL.Core;
using System.Text.Json.Serialization;

namespace APICL.Shared
{
    public class ImageData
    {
        public Guid Guid { get; set; } = Guid.Empty;
		public string Base64 { get; set; } = string.Empty;
        public int Width { get; set; } = 0;
        public int Height { get; set; } = 0;

		

		public ImageData()
		{
			// Default constructor for serialization
		}

		[JsonConstructor]
		public ImageData(Guid guid, string base64, int width, int height)
		{
			this.Guid = guid;
			this.Base64 = base64;
			this.Width = width;
			this.Height = height;
		}

		public async Task<ImageData> CreateFromImageObjAsync(ImageObj? obj)
		{
			var data = new ImageData();
			if (obj == null)
			{
				return data;
			}

			data.Guid = obj.Guid;
			data.Width = obj.Width;
			data.Height = obj.Height;
			data.Base64 = await obj.AsBase64();

			return data;
		}
	}
}
