using APICL.OpenCl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace APICL.Shared
{
    public class KernelArguments
    {
        public IEnumerable<string> ArgumentNames { get; set; } = [];
        public IEnumerable<Type> ArgumentTypes { get; set; } = [];



		public KernelArguments()
		{
			// Default constructor for serialization
		}

		[JsonConstructor]
        public KernelArguments(OpenClKernelCompiler? compiler, string kernel)
        {
            if (compiler == null)
            {
                return;
            }

            // Find in Files by kernel (contains)
            var match = compiler.Files.FirstOrDefault(f => f.Key.Contains(kernel, StringComparison.OrdinalIgnoreCase)).Key;
            
            if (string.IsNullOrEmpty(match))
            {
				Console.WriteLine($"Could not find matching kernel file for '{kernel}'");
                return;
            }
           
            try
            {
                compiler.LoadKernel("", match);

                var arguments = compiler.Arguments.Count <= 0 ? compiler.GetKernelArgumentsAnalog(match) : compiler.GetKernelArguments();
                this.ArgumentNames = arguments.Keys;
                this.ArgumentTypes = arguments.Values;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating KernelArguments object for file '{Path.GetFileName(match)}': {ex.Message} ({ex.InnerException?.Message})";
                this.ArgumentNames = [];
                this.ArgumentTypes = [];
            }
		}

		public Dictionary<string, Type> Arguments
        {
            get
            {
                if (this.ArgumentNames.Count() != this.ArgumentTypes.Count())
				{
					Console.WriteLine($"Cleared uneven kernel arguments <string, Type>: [{this.ArgumentNames.Count()} != {this.ArgumentTypes.Count()}]");
					this.ArgumentNames = [];
					this.ArgumentTypes = [];
				}

				return this.ArgumentNames
                           .Zip(this.ArgumentTypes, (name, value) => new { name, value })
                           .ToDictionary(x => x.name, x => x.value);
            }
        }

        public KernelArguments(Dictionary<string, Type>? givenArgs = null, MethodInfo? callingMethod = null)
        {
            if (givenArgs != null)
            {
                this.ArgumentNames = givenArgs.Keys;
                this.ArgumentTypes = givenArgs.Values;
                return;
            }

            List<string> argNames = [];
            List<Type> argTypes = [];

            if (callingMethod != null)
            {
                this.ArgumentNames = callingMethod.GetParameters().Select(p => p.Name ?? ("param_" + Guid.NewGuid()))
                                                  .Where(p => p.GetType() != typeof(string))
                                                  .Where(p => p.GetType() != typeof(bool));
                this.ArgumentTypes = callingMethod.GetParameters().Select(p => p.ParameterType);
            }
        }

    }
}
