using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Picard
{
    public sealed class IREmiter
    {
        // Internal Instance Data
        private readonly IRDirectiveEmiter _state = new IRDirectiveEmiter();
        private readonly List<MethodInfo> _methods = new List<MethodInfo>();

        // .Ctor
        public IREmiter(params MethodInfo[] methods)
        {
            _methods.AddRange(methods);
        }

        // Methods
        public string Emit()
        {
            var result = new StringBuilder();
            var methods = _methods.Select(x => new IRMethodEmiter(x, _state).Emit()).ToList();

            result.Append(_state.Directives + Environment.NewLine);

            foreach (var item in methods)
            {
                result.Append(item);
            }

            return result.ToString();
        }        
    }
}