using System;
using System.Reflection.Emit;

namespace Alea
{
    public struct MsilInstruction
    {
        internal OpCode Code { get; set; }
        internal int Offset { get; set; }
        internal object Operand { get; set; }
        internal bool IsMultiByte { get; set; }
    }
}