using System;
using System.Reflection.Emit;

namespace Picard
{
    // Done!
    internal static class OpCodeUtility
    {
        // Internal Const Data
        private const int Mask = 0xfe00;

        private static readonly OpCode[] _sbOpCodes = new OpCode[0x100];
        private static readonly OpCode[] _mbOpCodes = new OpCode[0x01f];

        // Static .Ctor
        static OpCodeUtility()
        {
            foreach (var item in typeof(OpCodes).GetFields())
            {
                var code = (OpCode)item.GetValue(null);
                
                if ((code.Value & Mask) == 0)
                {
                    _sbOpCodes[code.Value] = code;
                }
                else
                {
                    _mbOpCodes[code.Value & 0xff] = code;
                }
            }
        }

        // Methods
        internal static OpCode GetSingleByteOpCode(byte code)
        {
            return _sbOpCodes[code];
        }

        internal static OpCode GetMultipleByteOpCode(byte code)
        {
            return _mbOpCodes[code];
        }
    }
}