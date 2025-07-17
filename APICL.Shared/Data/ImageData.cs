using APICL.Core;

namespace APICL.Shared
{
    public class ImageData
    {
        public Guid Guid { get; set; } = Guid.Empty;
		public string Base64 { get; set; } = string.Empty;
        public int Width { get; set; } = 0;
        public int Height { get; set; } = 0;

        public ImageData(ImageObj? obj)
        {
            if (obj == null)
            {
                return;
			}

            this.Guid = obj.Guid;
			this.Base64 = obj.AsBase64().GetAwaiter().GetResult();
            this.Width = obj.Width;
            this.Height = obj.Height;
        }
    }
}
