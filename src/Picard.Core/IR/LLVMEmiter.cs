using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Picard
{
    public sealed class LLVMEmiter
    {
        // Internal Instance Data
        private readonly List<MethodInfo> _methods = new List<MethodInfo>();

        // .Ctor
        public LLVMEmiter(params MethodInfo[] methods)
        {
            _methods.AddRange(methods);
        }

        // Methods
        // Todo: Refactor!
        public string Emit()
        {
            var r = _methods.Select(x =>
            {
                var emiter = new LLVMMethodEmiter(x);
                emiter.Emit();

                return new
                {
                    emiter.Directives,
                    Instructions = emiter.Code
                };
            })
            .ToArray();

            var nl = Environment.NewLine;

            return new StringBuilder()
                .Append(string.Join(nl, r.Select(x => x.Directives)) + nl + nl)
                .Append(string.Join(nl, r.Select(x => x.Instructions)))
                .ToString();
        }        
    }
}