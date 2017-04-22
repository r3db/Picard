using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Picard
{
    internal sealed class LLVMMethodTranslator : IEnumerable
    {
        // Internal Instance Data
        private readonly IDictionary<string, IList<LLVMMethodTranslatorMapping>> _mappings = new Dictionary<string, IList<LLVMMethodTranslatorMapping>>();

        // Methods - Static
        internal static LLVMMethodTranslatorMapping Register(string returnType, string name, string[] argumentTypes, Action<LLVMMethodEmiterState> codeResolver)
        {
            return new LLVMMethodTranslatorMapping
            {
                Name = string.Format("{0} {1} {2}", returnType.ToLowerInvariant(), name, argumentTypes.Length),
                ArgumentTypes = argumentTypes,
                CodeResolver = codeResolver,
            };
        }

        // Methods
        internal Action<LLVMMethodEmiterState> Resolve(MethodInfo method)
        {
            var parameters = method.GetParameters();
            var name = string.Format("{0} {1}.{2} {3}", method.ReturnType.Name.ToLowerInvariant(), method.DeclaringType?.Name, method.Name, parameters.Length);

            if (_mappings.ContainsKey(name) == false)
            {
                return null;
            }

            var parameterNames = parameters.Select(x => x.ParameterType.Name.ToLowerInvariant()).ToArray();
            return _mappings[name].FirstOrDefault(x => x.ArgumentTypes.SequenceEqual(parameterNames))?.CodeResolver;
        }

        internal void Add(LLVMMethodTranslatorMapping item)
        {
            if (_mappings.ContainsKey(item.Name) == false)
            {
                _mappings.Add(item.Name, new List<LLVMMethodTranslatorMapping>());
            }

            _mappings[item.Name].Add(item);
        }

        public IEnumerator GetEnumerator()
        {
            return _mappings.Values.GetEnumerator();
        }
    }
}