using APICL.Core;

namespace APICL.Shared
{
    public class ImageObjInfo
    {
        public Guid Guid { get; set; } = Guid.Empty;
        public string Name { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public int Height { get; set; } = 0;
        public int Width { get; set; } = 0;
        public int Bitdepth { get; set; } = 0;
        public int Channels { get; set; } = 0;
        public long SizeInBytes { get; set; } = 0;
        public long Pointer { get; set; } = 0;
        public bool OnHost { get; set; } = false;


        public string Entry { get; set; } = string.Empty;


        public TimeSpan LastProcessingTimeSpan = TimeSpan.Zero;
        public string LastProcessingTime { get; set; } = string.Empty;
       
        public TimeSpan LastLoadingTimeSpan = TimeSpan.Zero;
        public string LastLoadingTime { get; set; } = string.Empty;



        public ImageObjInfo(ImageObj? obj, TimeSpan? loadingTime = null, TimeSpan? executionTime = null)
        {
            if (obj == null)
            {
                return;
            }

            this.Guid = obj.Guid;
            this.Name = obj.Name;
            this.FilePath = obj.Filepath;
            this.Height = obj.Height;
            this.Width = obj.Width;
            this.Bitdepth = obj.Bitdepth;
            this.Channels = obj.Channels;
            this.SizeInBytes = obj.SizeInBytes;
            this.Pointer = obj.Pointer;
            this.OnHost = obj.OnHost;

            if (executionTime.HasValue)
            {
                this.LastProcessingTimeSpan = executionTime.Value;
                this.LastProcessingTime = this.LastProcessingTimeSpan.ToString("hh':'mm':'ss'.'fff");
            }

            if (loadingTime.HasValue)
            {
                this.LastLoadingTimeSpan = loadingTime.Value;
                this.LastLoadingTime = this.LastLoadingTimeSpan.ToString("hh':'mm':'ss'.'fff");
            }

            this.Entry = $"'{this.Name}' ({this.Width}x{this.Height}, {(this.SizeInBytes / 1024)} kB) <{(this.Pointer != 0 ? this.Pointer : "")}>";
        }
    }
}
