using APICL.OpenCl;
using System.Runtime.InteropServices;

namespace APICL.Shared
{
	public class OpenClMemoryInfo
	{
		public List<long> Pointers { get; set; } = [];
		public List<long> Lengths { get; set; } = [];

		public long IndexPointer { get; set; } = 0;
		public long IndexLength { get; set; } = 0;
		public long Count { get; set; } = 0;
		public long TotalLength { get; set; } = 0;

		public int DataTypeSize { get; set; } = 0;
		public string DataTypeName { get; set; } = string.Empty;
		public long TotalSizeBytes { get; set; } = 0;

		// Important: NOT a filed, since not JSON-deserializable
		private Type DataType = typeof(object);

		public OpenClMemoryInfo(ClMem? obj = null)
		{
            if (obj == null)
            {
				return;
            }

			this.Pointers = obj.Buffers.Select(b => (long) b).ToList();
			this.Lengths = obj.Lengths.Select(l => (long) l).ToList();
			this.DataType = obj.ElementType ?? typeof(object);

			this.IndexPointer = this.Pointers.FirstOrDefault();
			this.IndexLength = this.Lengths.FirstOrDefault();
			this.Count = this.Pointers.LongCount();
			this.TotalLength = this.Lengths.Sum();
			this.DataTypeSize = Marshal.SizeOf(this.DataType);
			this.DataTypeName = this.DataType.Name;
			this.TotalSizeBytes = this.Lengths.Sum(length => length * this.DataTypeSize);
		}
	}
}
