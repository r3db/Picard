using System;

namespace Picard
{
    internal sealed class LLVMMethodTranslatorMapping
    {
        internal string Name { get; set; }
        internal string[] ArgumentTypes { get; set; }
        internal Action<LLVMMethodEmiterState> CodeResolver { get; set; }
    }
}