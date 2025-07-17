using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace APICL.Shared
{
    public class KernelArguments
    {
        public IEnumerable<string> ArgumentNames { get; set; } = [];
        public IEnumerable<Type> ArgumentTypes { get; set; } = [];


        public Dictionary<string, Type> Arguments
        {
            get
            {
                if (this.ArgumentNames.Count() != this.ArgumentTypes.Count())
                    throw new InvalidOperationException("Argument names and values count mismatch");

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
