using System;
using System.Reflection.Emit;

namespace Picard
{
    public struct MsilInstruction
    {
        internal OpCode Code { get; set; }
        internal int Offset { get; set; }
        internal object Operand { get; set; }
        internal bool IsMultiByte { get; set; }
        internal MsilInstructionOpCodeValue OpCodeValue { get; set; }
    }
}