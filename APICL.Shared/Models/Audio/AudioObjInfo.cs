using APICL.Core;

namespace APICL.Shared
{
	public class AudioObjInfo
	{
        public Guid Guid { get; } = Guid.Empty;
        public string Filepath { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Samplerate { get; set; } = -1;
        public int Bitdepth { get; set; } = -1;
        public int Channels { get; set; } = -1;
        public long Length { get; set; } = -1;

        public long Pointer { get; set; } = 0;
        public bool OnHost { get; set; } = false;
		public int ChunkSize { get; set; } = 0;
        public int OverlapSize { get; set; } = 0;
        public string Form { get; set; } = "f";
        public double StretchFactor { get; set; } = 1.0;
        public float Bpm { get; set; } = 0.0f;
        public bool Playing { get; set; } = false;

        public long SizeInBytes { get; set; } = 0;
		public double Duration { get; set; } = 0.0;

        public TimeSpan LastProcessingTimeSpan = TimeSpan.Zero;
        public string LastProcessingTime { get; set; } = string.Empty;

        public TimeSpan LastLoadingTimeSpan = TimeSpan.Zero;
        public string LastLoadingTime { get; set; } = string.Empty;

        public string Entry { get; set; } = string.Empty;

		public AudioObjInfo(AudioObj? obj, TimeSpan? loadingTime = null, TimeSpan? executionTime = null)
        {
            if (obj == null)
            {
                return;
            }

            this.Guid = obj.Guid;
            this.Filepath = obj.Filepath;
            this.Name = obj.Name;
            this.Samplerate = obj.Samplerate;
            this.Bitdepth = obj.Bitdepth;
            this.Channels = obj.Channels;
            this.Length = obj.Length;
            this.Pointer = obj.Pointer;
            this.OnHost = obj.OnHost;
			this.ChunkSize = obj.ChunkSize;
            this.OverlapSize = obj.OverlapSize;
            this.Form = obj.Form.ToString();
            this.StretchFactor = obj.StretchFactor;
            this.Bpm = obj.Bpm;
            this.Playing = obj.Playing;

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

            this.SizeInBytes = this.Length * (this.Bitdepth / 8) * this.Channels;
            this.Duration = (this.Samplerate > 0 && this.Channels > 0) ? (double)this.Length / (this.Samplerate * this.Channels) : 0;

            this.Entry = $"'{this.Name}' [{this.Bpm} BPM] - {this.Duration} sec.";
		}
}
}
