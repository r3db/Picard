using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Picard
{
    internal sealed class LlvmMethodTranslator : IEnumerable
    {
        // Internal Instance Data
        private readonly IDictionary<string, IList<LlvmMethodTranslatorMapping>> _mappings = new Dictionary<string, IList<LlvmMethodTranslatorMapping>>();

        // Methods - Static
        internal static LlvmMethodTranslatorMapping Register(string returnType, string name, string[] argumentTypes, Action<LvvmMethodEmiterState> codeResolver)
        {
            return new LlvmMethodTranslatorMapping
            {
                Name = string.Format("{0} {1} {2}", returnType.ToLowerInvariant(), name, argumentTypes.Length),
                ArgumentTypes = argumentTypes,
                CodeResolver = codeResolver,
            };
        }

        // Methods
        internal Action<LvvmMethodEmiterState> Resolve(MethodInfo method)
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

        internal void Add(LlvmMethodTranslatorMapping item)
        {
            if (_mappings.ContainsKey(item.Name) == false)
            {
                _mappings.Add(item.Name, new List<LlvmMethodTranslatorMapping>());
            }

            _mappings[item.Name].Add(item);
        }

        public IEnumerator GetEnumerator()
        {
            return _mappings.Values.GetEnumerator();
        }
    }
}