using APICL.Core;
using System.Text.Json.Serialization;

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

        public string ErrorInfoService { get; set; } = string.Empty;
        public string ErrorInfoMemory { get; set; } = string.Empty;
        public string ErrorInfoCompiler { get; set; } = string.Empty;
        public string ErrorInfoExecutioner { get; set; } = string.Empty;


		public ImageObjInfo()
		{
			// Default constructor for serialization
		}

		[JsonConstructor]
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

		public string ToString(int shorten = 0, bool singleLine = true)
		{
			string br = singleLine ? "| " : Environment.NewLine;
			string more = "...";
			shorten = Math.Max(0, shorten - more.Length);

			if (shorten > 0)
			{
				// Shorten strings to a maximum length
				string name = this.Name.Length > shorten ? this.Name.Substring(0, shorten) + more : this.Name;
				string guid = this.Guid.ToString().Length > shorten ? this.Guid.ToString().Substring(0, shorten) + more : this.Guid.ToString();
				
				string bitdepth = this.Bitdepth.ToString();
				string channels = this.Channels.ToString();

				string size = this.SizeInBytes / 1024.0 / 1024.0 > 1000 ? $"{this.SizeInBytes / 1024.0 / 1024.0 / 1024.0:F2} GB" : $"{this.SizeInBytes / 1024.0 / 1024.0:F2} MB";

				// Pointer as decimal if more places than shorten, otherwise as hex
				string pointer = shorten < this.Pointer.ToString().Length ? this.Pointer.ToString("X") : this.Pointer.ToString();

				return $"'{name}' {br} " +
                       $"({guid}) {br}" +
					   $"{bitdepth} bit {br}" +
					   $"{channels} ch. {br}" +
					   $"{size} {br}" +
					   $"<{pointer}>";
			}
			return $"'{this.Name}' {br} " +
                   $"({this.Guid}) {br}" +
				   $"Bitdepth: {this.Bitdepth} bit {br}" +
				   $"Channels: {this.Channels} {br}" +
				   $"Size: {this.SizeInBytes / 1024.0 / 1024.0:F2} MB {br}" +
				   $"Pointer: <{this.Pointer:X}>";
		}
	}
}
