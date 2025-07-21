using APICL.Core;
using System.Text.Json.Serialization;

namespace APICL.Shared
{
    public class AudioObjInfo
    {
        public Guid Guid { get; set; } = Guid.Empty;
        public string Filepath { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Samplerate { get; set; } = 0;
        public int Bitdepth { get; set; } = 0;
        public int Channels { get; set; } = 0;
        public int Length { get; set; } = 0;

        public long Pointer = 0;
        public bool OnHost = false;
        public string MemoryLocation { get; set; } = "Host";

		public int ChunkSize { get; set; } = 0;
        public int OverlapSize { get; set; } = 0;
        public string Form { get; set; } = "f";
        public double StretchFactor = 1.0;
        public float Bpm { get; set; } = 0.0f;
        public bool Playing = false;
        public string PlayingString { get; set; } = "Stopped";

		public long SizeInBytes
        {
            get => this.Length * (this.Bitdepth / 8) * this.Channels;
		}
        public float SizeInMegaBytes => (float)(this.SizeInBytes / 1024.0 / 1024.0);
		public string SizeString => $"{this.SizeInMegaBytes:F2} MB";
		public double Duration = 0.0;
        public TimeSpan DurationTimeSpan => TimeSpan.FromSeconds(this.Duration);
        public string DurationString { get; set; } = TimeSpan.Zero.ToString("hh':'mm':'ss'.'fff");

		public TimeSpan LastProcessingTimeSpan = TimeSpan.Zero;
        public string LastProcessingTime { get; set; } = string.Empty;

        public TimeSpan LastLoadingTimeSpan = TimeSpan.Zero;
        public string LastLoadingTime { get; set; } = string.Empty;

        public string Entry { get; set; } = string.Empty;

		public string ErrorInfoService { get; set; } = string.Empty;
		public string ErrorInfoMemory { get; set; } = string.Empty;
		public string ErrorInfoCompiler { get; set; } = string.Empty;
		public string ErrorInfoExecutioner { get; set; } = string.Empty;
		public String ErrorInfo { get; set; }

		public AudioObjInfo()
        {
			// Default constructor for serialization
		}

		[JsonConstructor]
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
            this.Length = (int)obj.Length;
            this.Pointer = obj.Pointer;
            this.OnHost = obj.OnHost;
            this.ChunkSize = obj.ChunkSize;
            this.OverlapSize = obj.OverlapSize;
            this.Form = obj.Form;
            this.StretchFactor = obj.StretchFactor;
            this.Bpm = obj.Bpm;
            this.Playing = obj.Playing;

            this.PlayingString = obj.Playing ? "Playing" : "Stopped";

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

            this.MemoryLocation = this.OnHost ? "Host" : "Device";

			this.Duration = (this.Samplerate > 0 && this.Channels > 0) ? (double) this.Length / (this.Samplerate * this.Channels) : 0;
			this.DurationString = this.DurationTimeSpan.ToString("hh':'mm':'ss'.'fff");

			this.Entry = $"'{this.Filepath}' ({this.Guid})";
		}

        public string ToString(int shorten = 0, bool singleLine = true)
        {
			string br = singleLine ? "| " : Environment.NewLine;
			string more = "...";
			shorten = Math.Max(0, shorten - more.Length);

            string playback = this.Playing ? "▶" : "◼";

			if (shorten > 0)
            {
				// Shorten strings to a maximum length
                string name = this.Name.Length > shorten ? this.Name.Substring(0, shorten) + more : this.Name;
                string guid = this.Guid.ToString().Length > shorten ? this.Guid.ToString().Substring(0, shorten) + more : this.Guid.ToString();
                string bpm = this.Bpm.ToString($"F{shorten}");

                // Duration in seconds or timespan
                string duration = shorten < 10 ? $"{this.Duration:F2}" : this.DurationString;
				
                string samplerate = shorten > 5 ? $"{this.Samplerate / 1000} k" : this.Samplerate.ToString();
                string bitdepth = this.Bitdepth.ToString();
                string channels = this.Channels.ToString();

				// Shorten length by 10^3 => K, 10^6 => M, 10^9 => G
                string length = shorten > 4 ? shorten > 7 ? shorten > 10 ? $"{this.Length / 1_000_000_000.0:F2} G" : $"{this.Length / 1_000_000.0:F2} M" : $"{this.Length / 1_000.0:F2} K" : this.Length.ToString();

                string size = this.SizeInBytes / 1024.0 / 1024.0 > 1000 ? $"{this.SizeInBytes / 1024.0 / 1024.0 / 1024.0:F2} GB" : $"{this.SizeInBytes / 1024.0 / 1024.0:F2} MB";

				// Pointer as decimal if more places than shorten, otherwise as hex
				string pointer = shorten < this.Pointer.ToString().Length ? this.Pointer.ToString("X") : this.Pointer.ToString();

                return $"{playback} '{name}' {br} " +
                       $"({guid}) {br}" +
					   $"[{bpm}] {br}" +
                       $"{duration} {br}" +
                       $"{samplerate} Hz {br}" +
                       $"{bitdepth} bit {br}" +
                       $"{channels} {br}" +
                       $"{length} f32 {br}" +
                       $"{size} {br}" +
                       $"<{pointer}>";
			}
            return $"{playback} '{this.Name}' {br} " +
                   $"[{this.Bpm:F4} BPM] {br}" +
                   $"({this.Guid}) {br}" +
				   $"{this.DurationString} {br}" +
                   $"Samplerate: {this.Samplerate} Hz {br}" +
                   $"Bitdepth: {this.Bitdepth} bit {br}" +
                   $"Channels: {this.Channels} {br}" +
                   $"Length: {this.Length} samples {br}" +
                   $"Size: {this.SizeInBytes / 1024.0 / 1024.0:F2} MB {br}" +
                   $"Pointer: <{this.Pointer:X}>";
		}
    }
}
