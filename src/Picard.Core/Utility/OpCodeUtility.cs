using System.Reflection;
using System.Reflection.Emit;

namespace Alea
{
    internal static class OpCodeUtility
    {
        // Internal Const Data
        private const int Mask = 0xfe00;

        private static readonly OpCode[] _sbOpCodes = new OpCode[0x100];
        private static readonly OpCode[] _mbOpCodes = new OpCode[0x100];

        // Static .Ctor
        static OpCodeUtility()
        {
            Register();
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

        // Helpers
        private static void Register()
        {
            var codes = typeof(OpCodes).GetFields();

            foreach (var item in codes)
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
    }
}