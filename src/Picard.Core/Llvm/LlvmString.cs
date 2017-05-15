using System;

namespace Picard
{
    internal struct LlvmString
    {
        // .Ctor
        internal LlvmString(string s)
        {
            Original = s;
            Encoded = s.Replace("\r", "\\0D").Replace("\n", "\\0A");
        }

        // Properties
        internal string Original { get; }
        internal string Encoded { get; }
    }
}