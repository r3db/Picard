using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Emit;

namespace Picard
{
    public sealed class MsilInstructionDecoder
    {
        // Internal Instance Data
        private readonly BinaryReader _reader;
        private readonly ITokenResolver _resolver;

        // .Ctor
        public MsilInstructionDecoder(byte[] msil, ITokenResolver resolver)
        {
            _reader = new BinaryReader(new MemoryStream(msil));
            _resolver = resolver;
        }
        
        // Methods
        public IEnumerable<MsilInstruction> DecodeAll()
        {
            var result = new List<MsilInstruction>();

            while (_reader.BaseStream.Position < _reader.BaseStream.Length)
            {
                var offset = (int) _reader.BaseStream.Position;
                var il = _reader.ReadByte();

                OpCode code;
                bool isMultiByte;

                if (il == 0xfe)
                {
                    il = _reader.ReadByte();
                    isMultiByte = true;
                    code = OpCodeUtility.GetMultipleByteOpCode(il);
                }
                else
                {
                    isMultiByte = false;
                    code = OpCodeUtility.GetSingleByteOpCode(il);
                }

                var instruction = new MsilInstruction
                {
                    Code = code,
                    Offset = offset,
                    Operand = GetOperand(code, offset),
                    IsMultiByte = isMultiByte
                };

                result.Add(instruction);
            }

            return result;
        }

        // Helpers
        private object GetOperand(OpCode code, int offset)
        {
            switch (code.OperandType)
            {
                case OperandType.InlineBrTarget:      return _reader.ReadInt32() + offset;
                case OperandType.InlineField:         return _resolver.ResolveField(_reader.ReadInt32());
                case OperandType.InlineI:             return _reader.ReadInt32();
                case OperandType.InlineI8:            return _reader.ReadInt64();
                case OperandType.InlineMethod:        return _resolver.ResolveMember(_reader.ReadInt32());
                case OperandType.InlineR:             return _reader.ReadDouble();
                case OperandType.InlineSig:           return _reader.ReadInt32();
                case OperandType.InlineString:        return _resolver.ResolveString(_reader.ReadInt32());
                case OperandType.InlineSwitch:        return _reader.ReadInt32();
                case OperandType.InlineTok:           return _reader.ReadInt32();
                case OperandType.InlineType:          return _resolver.ResolveType(_reader.ReadInt32());
                case OperandType.InlineVar:           return _reader.ReadUInt16();
                case OperandType.ShortInlineBrTarget: return (byte)(_reader.ReadByte() + offset);
                case OperandType.ShortInlineI:        return _reader.ReadByte();
                case OperandType.ShortInlineR:        return _reader.ReadSingle();
                case OperandType.ShortInlineVar:      return _reader.ReadByte();
                case OperandType.InlineNone:          return null;
            }

            throw new NotSupportedException("Unknown operand type.");
        }
    }
}