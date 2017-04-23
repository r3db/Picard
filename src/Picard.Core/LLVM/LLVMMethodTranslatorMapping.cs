using System;

namespace Picard
{
    internal sealed class LlvmMethodTranslatorMapping
    {
        internal string Name { get; set; }
        internal string[] ArgumentTypes { get; set; }
        internal Action<LvvmMethodEmiterState> CodeResolver { get; set; }
    }
}