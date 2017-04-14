using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Picard
{
    // Todo: Refactor!
    public static class MsilInstructionRepresenter
    {
        // Internal Static Data
        private static readonly string _mscorlib = typeof(string).Assembly.FullName;

        // Methods
        public static string Represent(MsilInstruction instruction, MethodInfo method)
        {
            var dtn = new AssemblyName(method.DeclaringType.Assembly.FullName).Name;
            var locals = method.GetMethodBody().LocalVariables;

            var result = new StringBuilder();

            result.Append(RepresentPreamble(instruction));
            result.Append(RepresentInstruction(instruction));
            result.Append(RepresentVariableInformation(instruction, method));
            result.Append(RepresentOperand(instruction, dtn, locals));

            return result.ToString();
        }
        
        // Helpers
        private static string RepresentPreamble(MsilInstruction instruction)
        {
            return string.Format("IL_{0:x4}: 0x{1:x2}{2}",
                instruction.Offset,
                instruction.Code.Value,
                instruction.IsMultiByte ? null : "  ");
        }

        private static string RepresentInstruction(MsilInstruction instruction)
        {
            return string.Format("{0}{1}",
                instruction.Code.Name,
                new string(' ', 12 - instruction.Code.Name.Length));
        }

        private static string RepresentVariableInformation(MsilInstruction instruction, MethodBase method)
        {
            switch (instruction.OpCodeValue)
            {
                case MsilInstructionOpCodeValue.Ldarg_0:
                {
                    var param = method.GetParameters();
                    return string.Format(" // {0}", method.IsStatic ? param[0].Name : "this");
                }
                case MsilInstructionOpCodeValue.Ldarg_1:
                case MsilInstructionOpCodeValue.Ldarg_2:
                case MsilInstructionOpCodeValue.Ldarg_3:
                {
                    var param = method.GetParameters();
                    return string.Format(" // {0}", method.IsStatic ? param[1].Name : param[0].Name);
                }
                case MsilInstructionOpCodeValue.Ldloc_0:
                case MsilInstructionOpCodeValue.Ldloc_1:
                case MsilInstructionOpCodeValue.Ldloc_2:
                case MsilInstructionOpCodeValue.Ldloc_3:
                {
                    return string.Format(" // {0}", "V_0" /*GetReturnTypeName(dtn, method.GetMethodBody().LocalVariables[0].LocalType)*/);
                }
            }

            return null;
        }

        private static string RepresentOperand(MsilInstruction instruction, string dtn, IList<LocalVariableInfo> locals)
        {
            // Todo: Refactor!
            switch (instruction.Code.OperandType)
            {
                case OperandType.InlineBrTarget:
                {
                    var offset = (int) instruction.Operand;
                    return string.Format("IL_{0:x8}", offset);
                }
                case OperandType.InlineField:
                {
                    var operand = (FieldInfo) instruction.Operand;
                    return string.Format(" {0} {1}::{2}", GetReturnTypeName(dtn, operand.FieldType), GetTypeName(dtn, operand.ReflectedType), operand.Name);
                }
                case OperandType.InlineI:
                {
                    return string.Format(" {0}", instruction.Operand);
                }
                //case OperandType.InlineI8:
                //{
                //    ReadInt64();
                //    break;
                //}
                case OperandType.InlineMethod:
                {
                    var sb = new StringBuilder();
                    var methodInfo = instruction.Operand as MethodInfo;

                    if (methodInfo != null)
                    {
                        var operand = methodInfo;

                        if (operand.IsStatic == false)
                        {
                            sb.Append(" instance");
                        }

                        sb.AppendFormat(" {0} {1}::{2}", GetReturnTypeName(dtn, operand.ReturnType), GetTypeName(dtn, operand.ReflectedType), operand.Name);
                        sb.Append("(");
                        sb.Append(string.Join(", ", operand.GetParameters().Select(x => GetReturnTypeName(dtn, x.ParameterType))));
                        sb.Append(")");
                    }

                    var constructorInfo = instruction.Operand as ConstructorInfo;

                    if (constructorInfo != null)
                    {
                        var operand = constructorInfo;

                        if (operand.IsStatic == false)
                        {
                            sb.Append(" instance");
                        }

                        sb.AppendFormat(" void {0}::{1}", GetTypeName(dtn, operand.ReflectedType), operand.Name);
                        sb.Append("(");
                        sb.Append(string.Join(", ", operand.GetParameters().Select(x => GetReturnTypeName(dtn, x.ParameterType))));
                        sb.Append(")");
                    }

                    return sb.ToString();
                }
                case OperandType.InlineNone:
                {
                    break;
                }
                //case OperandType.InlineR:
                //{
                //    ReadSingle32();
                //    break;
                //}
                //case OperandType.InlineSig:
                //{
                //    ReadInt32();
                //    break;
                //}
                case OperandType.InlineString:
                {
                    return string.Format(" \"{0}\"", instruction.Operand);
                }
                //case OperandType.InlineSwitch:
                //{
                //    ReadInt32();
                //    break;
                //}
                //case OperandType.InlineTok:
                //{
                //    ReadInt32();
                //    break;
                //}
                case OperandType.InlineType:
                {
                    return string.Format(" {0}", GetReturnTypeName(dtn, (Type)instruction.Operand));
                }
                //case OperandType.InlineVar:
                //{
                //    ReadUInt16();
                //    break;
                //}
                case OperandType.ShortInlineBrTarget:
                {
                    return string.Format(" IL_{0:x4}", instruction.Operand);
                }
                case OperandType.ShortInlineI:
                {
                    return string.Format(" {0} // 0x{0:x2}", instruction.Operand);
                }
                case OperandType.ShortInlineR:
                {
                    return string.Format(" {0}", instruction.Operand);
                }
                case OperandType.ShortInlineVar:
                {
                    var index = (byte)instruction.Operand;
                    return string.Format(" V_{0} // {1}", index, GetReturnTypeName(dtn, locals[index].LocalType));
                }
                default:
                {
                    throw new NotSupportedException("Unknown operand type.");
                }
            }

            return null;
        }





        private static string GetReturnTypeName(string declaringTypeName, Type type)
        {
            if (type.Assembly.FullName == _mscorlib)
            {
                if (type == typeof(void))
                {
                    return "void";
                }

                if (type == typeof(string))
                {
                    return "string";
                }

                if (type == typeof(object))
                {
                    return "object";
                }

                if (type == typeof(IntPtr))
                {
                    return "native int";
                }

                if (type == typeof(int))
                {
                    return "int32";
                }

                if (type == typeof(float))
                {
                    return "float";
                }

                if (type == typeof(int[]))
                {
                    return "int32[]";
                }
            }

            return "class " + GetTypeName(declaringTypeName, type);
        }

        // Todo: Handle Generics!
        private static string GetTypeName(string declaringTypeName, Type type)
        {
            var name = new AssemblyName(type.Assembly.FullName).Name;

            Func<string, string> cleanType = s =>
            {
                if (s.Contains('<') || s.Contains('>'))
                {
                    return $"'{s}'";
                }

                return s;
            };

            var nestedType = type.IsNested
                ? $".{cleanType(type.DeclaringType.Name)}"
                : null;

            var accessOperator = type.IsNested
                ? "+"
                : ".";

            var fullName = $"{type.Namespace}{nestedType}{accessOperator}{cleanType(type.Name)}";

            return name == declaringTypeName
                ? $"{fullName}"
                : $"[{name}]{fullName}";
        }
    }
}