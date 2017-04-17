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
        // Internal Const Data
        private const string Targets = @"
            target triple = ""nvptx64 - unknown - cuda""
            target datalayout = ""e-p:64:64:64-i1:8:8-i8:8:8-i16:16:16-i32:32:32-i64:64:64-f32:32:32-f64:64:64-v16:16:16-v32:32:32-v64:64:64-v128:128:128-n16:32:64""
        ";

        private const string Attributes = @"
            declare i8* @llvm.nvvm.ptr.constant.to.gen.p0i8.p4i8(i8 addrspace(4)*) #0
            declare i32 @vprintf(i8* nocapture, i8*) #1
            declare void @llvm.donothing() #0

            attributes #0 = { nounwind readnone }
            attributes #1 = { nounwind }

            !nvvmir.version = !{!0}
            !nvvm.annotations = !{!1}

            !0 = metadata !{i32 1, i32 2, i32 2, i32 0}
            !1 = metadata !{void ()* @main, metadata !""kernel"", i32 1}
        ";

        // Internal Instance Data
        private readonly ThreadLocal<Func<string>> _identifierGenerator = new ThreadLocal<Func<string>>(() =>
        {
            var identifier = 0;
            return () => string.Format("@_{0}_{1}", Thread.CurrentThread.ManagedThreadId, identifier++);
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
            var result = _methods.Select(x =>
            {
                var emiter = LLVMMethodEmiter.Emit(x, _identifierGenerator.Value);

                return new
                {
                    emiter.Directives,
                    emiter.Code
                };
            })
            .ToArray();

            return new StringBuilder()
                .AppendLine(Targets)
                .AppendLine(string.Join(Environment.NewLine, result.Select(x => x.Directives)))
                .AppendLine(string.Join(Environment.NewLine, result.Select(x => x.Code)))
                .AppendLine(Attributes)
                .ToString();
        }        
    }
}