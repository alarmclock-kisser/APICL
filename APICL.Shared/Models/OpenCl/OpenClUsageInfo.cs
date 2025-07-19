using APICL.OpenCl;
using System;

namespace APICL.Shared
{
	public class OpenClUsageInfo
	{
		public long TotalMemory { get; set; } = 0;
		public long UsedMemory { get; set; } = 0;
		public long FreeMemory { get; set; } = 0;
		public float UsagePercentage { get; set; } = 0.0f;

		public string SizeUnit { get; set; } = "Bytes";



		public OpenClUsageInfo()
		{
			// Default ctor for JSON serialization
		}

		public OpenClUsageInfo(OpenClMemoryRegister? register, bool readable = false)
		{
            if (register == null)
            {
				return;
            }

			this.SizeUnit = readable ? "MBytes" : "Bytes";

			try
			{
				this.TotalMemory = register.GetMemoryTotal(readable);
				this.UsedMemory = register.GetMemoryUsed(readable);
				this.FreeMemory = register.GetMemoryFree(readable);
				this.UsagePercentage = this.TotalMemory > 0 ? (float) this.UsedMemory / this.TotalMemory * 100 : 0f;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error creating OpenClUsageInfo object: {ex.Message} ({ex.InnerException?.Message})");
			}
		}

	}
}
