using APICL.OpenCl;
using System;

namespace APICL.Shared
{
	public class OpenClUsageInfo
	{
		public string TotalMemory { get; set; } = "0";
		public string UsedMemory { get; set; } = "0";
		public string FreeMemory { get; set; } = "0";
		public float UsagePercentage { get; set; } = 0.0f;

		public string SizeUnit { get; set; } = "Bytes";

		public IEnumerable<PieChartData> PieChart { get; set; } = [];



		public OpenClUsageInfo()
		{
			// Default ctor for JSON serialization
		}

		public OpenClUsageInfo(string total, string used, string free, float percentage)
		{
			this.TotalMemory = total;
			this.UsedMemory = used;
			this.FreeMemory = free;
			this.UsagePercentage = percentage;

			this.GetPieChart();
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
				this.TotalMemory = register.GetMemoryTotal(readable).ToString();
				this.UsedMemory = register.GetMemoryUsed(readable).ToString();
				this.FreeMemory = register.GetMemoryFree(readable).ToString();
				
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error creating OpenClUsageInfo object: {ex.Message} ({ex.InnerException?.Message})");
			}
		}

		private void GetPieChart()
		{
			this.PieChart = [new PieChartData() { Label = "Used", Value = this.UsagePercentage}, new PieChartData() { Label = "Free", Value = 100f - this.UsagePercentage}];
		}

	}



	public class PieChartData
	{
		public string Label { get; set; } = string.Empty;
		public float Value { get; set; } = 0.0f;


		public PieChartData()
		{

		}
	}
}
