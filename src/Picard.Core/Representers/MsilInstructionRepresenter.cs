using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Picard
{
    public static class MsilInstructionRepresenter
    {
        // Internal Static Data
        private static readonly string _mscorlib = typeof(string).Assembly.FullName;

        // Todo: Refactor!
        // Methods
        public static string Represent(MsilInstruction instruction, MethodInfo method)
        {
            var dtn = new AssemblyName(method.DeclaringType.Assembly.FullName).Name;
            var locals = method.GetMethodBody().LocalVariables;

            var result = new StringBuilder();
            
            result.AppendFormat("IL_{0:x4}: 0x{1:x2}{2} {3}{4}", instruction.Offset, instruction.Code.Value, instruction.IsMultiByte ? null : "  ", instruction.Code.Name, new string(' ', 12 - instruction.Code.Name.Length));

            switch (instruction.OpCodeValue)
            {
                case MsilInstructionOpCodeValue.Ldarg_0:
                {
                    var param = method.GetParameters();
                    result.AppendFormat(" // {0}", method.IsStatic ? param[0].Name : "this");
                    break;
                }
                case MsilInstructionOpCodeValue.Ldarg_1:
                case MsilInstructionOpCodeValue.Ldarg_2:
                case MsilInstructionOpCodeValue.Ldarg_3:
                {
                    var param = method.GetParameters();
                    result.AppendFormat(" // {0}", method.IsStatic ? param[1].Name : param[0].Name);
                    break;
                }
                case MsilInstructionOpCodeValue.Ldloc_0:
                case MsilInstructionOpCodeValue.Ldloc_1:
                case MsilInstructionOpCodeValue.Ldloc_2:
                case MsilInstructionOpCodeValue.Ldloc_3:
                {
                    result.AppendFormat(" // {0}", "V_0" /*GetReturnTypeName(dtn, method.GetMethodBody().LocalVariables[0].LocalType)*/);
                    break;
                }
            }
            
            // Todo: Refactor!
            switch (instruction.Code.OperandType)
            {
                case OperandType.InlineBrTarget:
                {
                    var offset = (int)instruction.Operand;
                    result.AppendFormat("IL_{0:x8}", offset);
                    break;
                }
                case OperandType.InlineField:
                {
                    var operand = (FieldInfo)instruction.Operand;
                    result.AppendFormat(" {0} {1}::{2}", GetReturnTypeName(dtn, operand.FieldType), GetTypeName(dtn, operand.ReflectedType), operand.Name);
                    break;
                }
                case OperandType.InlineI:
                {
                    var o = instruction.Operand;
                    result.AppendFormat(" {0}", o);
                    break;
                }
                //case OperandType.InlineI8:
                //{
                //    ReadInt64();
                //    break;
                //}
                case OperandType.InlineMethod:
                {
                    var methodInfo = instruction.Operand as MethodInfo;

                    if (methodInfo != null)
                    {
                        var operand = methodInfo;

                        if (operand.IsStatic == false)
                        {
                            result.Append(" instance");
                        }

                        result.AppendFormat(" {0} {1}::{2}", GetReturnTypeName(dtn, operand.ReturnType), GetTypeName(dtn, operand.ReflectedType), operand.Name);
                        result.Append("(");
                        result.Append(string.Join(", ", operand.GetParameters().Select(x => GetReturnTypeName(dtn, x.ParameterType))));
                        result.Append(")");
                    }

                    var constructorInfo = instruction.Operand as ConstructorInfo;

                    if (constructorInfo != null)
                    {
                        var operand = constructorInfo;

                        if (operand.IsStatic == false)
                        {
                            result.Append(" instance");
                        }

                        result.AppendFormat(" void {0}::{1}", GetTypeName(dtn, operand.ReflectedType), operand.Name);
                        result.Append("(");
                        result.Append(string.Join(", ", operand.GetParameters().Select(x => GetReturnTypeName(dtn, x.ParameterType))));
                        result.Append(")");
                    }

                    break;
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
                    result.AppendFormat(" \"{0}\"", (string)instruction.Operand);
                    break;
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
                    var type = (Type)instruction.Operand;
                    result.AppendFormat(" {0}", GetReturnTypeName(dtn, type));
                    break;
                }
                //case OperandType.InlineVar:
                //{
                //    ReadUInt16();
                //    break;
                //}
                case OperandType.ShortInlineBrTarget:
                {
                    var offset = (byte)instruction.Operand;
                    result.AppendFormat(" IL_{0:x4}", offset);
                    break;
                }
                case OperandType.ShortInlineI:
                {
                    var value = (byte)instruction.Operand;
                    result.AppendFormat(" {0} // 0x{0:x2}", value);
                    break;
                }
                case OperandType.ShortInlineR:
                {
                    var value = instruction.Operand;
                    result.AppendFormat(" {0}", value);
                    break;
                }
                case OperandType.ShortInlineVar:
                {
                    var index = (byte)instruction.Operand;
                    result.AppendFormat(" V_{0} // {1}", index, GetReturnTypeName(dtn, locals[index].LocalType));
                    break;
                }
                default:
                {
                    throw new NotSupportedException("Unknown operand type.");
                }
            }

            return result.ToString();
        }
        
        // Helpers
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