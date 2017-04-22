using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Picard
{
    internal sealed class LLVMMethodEmiterState
    {
        // Internal Instance Data
        private readonly Func<string> _directiveIdentifierGenerator;
        private int _instructionIdentifier;

        // .Ctor
        internal LLVMMethodEmiterState(Func<string> directiveIdentifierGenerator)
        {
            _directiveIdentifierGenerator = directiveIdentifierGenerator;
        }

        // Properties - Auto Implemented
        internal Stack DirectiveStack       { get; } = new Stack();
        internal Stack InstructionStack     { get; } = new Stack();
        internal StringBuilder Directives   { get; } = new StringBuilder();
        internal StringBuilder Instructions { get; } = new StringBuilder();
        internal HashSet<int> Labels        { get; } = new HashSet<int>();

        // Methods
        internal string NextDirectiveIdentifier()
        {
            return _directiveIdentifierGenerator();
        }

        internal string NextInstructionIdentifier()
        {
            return string.Format("%_{0}", _instructionIdentifier++);
        }

        [Conditional("DEBUG")]
        internal void AppendPreamble()
        {
            Instructions.Append("    ");
        }
    }
}