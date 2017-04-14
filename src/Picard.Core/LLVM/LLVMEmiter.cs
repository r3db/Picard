using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Picard
{
    public sealed class LLVMEmiter
    {
        // Internal Instance Data
        private readonly ThreadLocal<Func<string>> _identifierGenerator = new ThreadLocal<Func<string>>(() =>
        {
            var identifier = 0;
            return () => string.Format("@{0}_{1}", Thread.CurrentThread.ManagedThreadId, identifier++);
        });

        // Internal Instance Data
        private readonly List<MethodInfo> _methods = new List<MethodInfo>();

        // .Ctor
        private LLVMEmiter(params MethodInfo[] methods)
        {
            _methods.AddRange(methods);
        }

        // Factory .Ctor
        public static string Emit(params MethodInfo[] methods)
        {
            return new LLVMEmiter(methods).Emit();
        }

        // Helpers
        private string Emit()
        {
            // Todo: Check for any missing Methods we may have found on the way!
            var result = _methods.AsParallel().Select(x =>
            {
                var emiter = LLVMMethodEmiter.Emit(x, _identifierGenerator.Value);

                return new
                {
                    emiter.Directives,
                    emiter.Code
                };
            })
            .ToArray();

            var nl = Environment.NewLine;

            return new StringBuilder()
                .Append(string.Join(nl, result.Select(x => x.Directives.Trim('\r', '\n'))))
                .Append(nl + nl)
                .Append(string.Join(nl + nl, result.Select(x => x.Code.Trim('\r', '\n'))))
                .ToString();
        }        
    }
}