﻿using APICL.OpenCl;
using System.Text.Json.Serialization;

namespace APICL.Shared
{
	public class OpenClKernelInfo
	{
		public int Index { get; set; } = -1;
		public string FilePath { get; set; } = string.Empty;
		public string FunctionName { get; set; } = string.Empty;
		public List<string> ArgumentNames { get; set; } = [];

        public List<Type> ArgumentTypes = [];

		public int ArgumentsCount { get; set; } = 0;
		public List<string> ArgumentTypeNames { get; set; } = [];
		public string InputPointerTypeName { get; set; } = "void*";
		public string OutputPointerTypeName { get; set; } = string.Empty;

        public string MediaType { get; set; } = "DIV";
        public string Entry { get; set; } = string.Empty;



		public OpenClKernelInfo()
		{
			// Empty default ctor
		}

		[JsonConstructor]
		public OpenClKernelInfo(OpenClKernelCompiler? compiler, int index)
		{
			this.Index = index;

			if (compiler == null)
            {
				return;
            }

			if (index < 0 || index >= compiler.Files.Count)
			{
				Console.WriteLine($"Kernel-index is out of range (max: {(compiler.Files.Count - 1)}, was {index})");
				return;
			}

			try
			{
				var file = compiler.Files.ElementAt(index);
				var args = Task.Run(() =>
				{
					return compiler.GetKernelArguments(null, file.Key);
				}).Result;

				this.FilePath = file.Key;
				this.FunctionName = file.Value;
				this.ArgumentNames = args.Select(arg => arg.Key).ToList();
				this.ArgumentTypes = args.Select(arg => arg.Value).ToList();
				this.ArgumentTypeNames = this.ArgumentTypes.Select(type => type.Name).ToList();
				this.ArgumentsCount = this.ArgumentNames.Count == this.ArgumentTypes.Count ? this.ArgumentTypes.Count : -1;
				this.InputPointerTypeName = this.ArgumentTypeNames.FirstOrDefault(n => n.Contains('*')) ?? "void*";
				this.OutputPointerTypeName = this.ArgumentTypeNames.LastOrDefault(n => n.Contains('*')) ?? string.Empty;

				this.MediaType = this.FilePath.Contains(@"\Image\") ? "IMG" : this.MediaType;
				this.MediaType = this.FilePath.Contains(@"\Audio\") ? "AUD" : this.MediaType;
				this.Entry = $"{this.MediaType}: '{this.FunctionName}' [{this.ArgumentsCount}] ({this.InputPointerTypeName} -> {this.OutputPointerTypeName})";
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error creating OpenClKernelInfo object for index [{index}]: {ex.Message} ({ex.InnerException?.Message})");
				this.Index = -1;
			}
		}
	}
}
