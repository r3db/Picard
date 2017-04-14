using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Picard
{
    public sealed class IREmiter
    {
        // Internal Instance Data
        private readonly Stack<object> _stack = new Stack<object>();
        private readonly StringBuilder _localInstructions = new StringBuilder();
        private readonly StringBuilder _globalInstructions = new StringBuilder();
        private readonly IDictionary<string, object> _globalData = new Dictionary<string, object>();
        // Todo: Determine the size of the locals!
        private readonly object[] _locals = new object[100];

        private int _localIdentifierCounter;
        private int _globalIdentifierCounter;

        // .Ctor
        private IREmiter()
        {
        }

        public static string Emit(MethodInfo method)
        {
            return new IREmiter().EmitInternal(method);
        }

        // Todo: Refactor!
        private string EmitInternal(MethodBase method)
        {
            var instructions = new MsilInstructionDecoder(method.GetMethodBody()?.GetILAsByteArray(), method.Module).DecodeAll().ToList();
            
            for (var i = 0; i < instructions.Count; ++i)
            {
                var instruction = instructions[i];
            
                switch (instruction.OpCodeValue)
                {
                    case MsilInstructionOpCodeValue.Nop:
                    {
                        EmitNop(instruction);
                        continue;
                    }
                    case MsilInstructionOpCodeValue.Break:
                    case MsilInstructionOpCodeValue.Ldarg_0:
                    case MsilInstructionOpCodeValue.Ldarg_1:
                    case MsilInstructionOpCodeValue.Ldarg_2:
                    case MsilInstructionOpCodeValue.Ldarg_3:
                    {
                        break;
                    }
                    case MsilInstructionOpCodeValue.Ldloc_0:
                    {
                        PushToStack(_locals[0]);
                        continue;
                    }
                    case MsilInstructionOpCodeValue.Ldloc_1:
                    {
                        PushToStack(_locals[1]);
                        continue;
                    }
                    case MsilInstructionOpCodeValue.Ldloc_2:
                    {
                        PushToStack(_locals[2]);
                        continue;
                    }
                    case MsilInstructionOpCodeValue.Ldloc_3:
                    {
                        PushToStack(_locals[3]);
                        continue;
                    }
                    case MsilInstructionOpCodeValue.Stloc_0:
                    {
                        StoreInLocal(0);
                        continue;
                    }
                    case MsilInstructionOpCodeValue.Stloc_1:
                    {
                        StoreInLocal(1);
                        continue;
                    }
                    case MsilInstructionOpCodeValue.Stloc_2:
                    {
                        StoreInLocal(2);
                        continue;
                    }
                    case MsilInstructionOpCodeValue.Stloc_3:
                    {
                        StoreInLocal(3);
                        continue;
                    }
                    case MsilInstructionOpCodeValue.Ldarg_S:
                    case MsilInstructionOpCodeValue.Ldarga_S:
                    case MsilInstructionOpCodeValue.Starg_S:
                    case MsilInstructionOpCodeValue.Ldloc_S:
                    case MsilInstructionOpCodeValue.Ldloca_S:
                    case MsilInstructionOpCodeValue.Stloc_S:
                    {
                        break;
                    }
                    case MsilInstructionOpCodeValue.Ldnull:
                    {
                        PushToStack("null");
                        continue;
                    }
                    case MsilInstructionOpCodeValue.Ldc_I4_M1:
                    {
                        PushToStack(-1);
                        continue;
                    }
                    case MsilInstructionOpCodeValue.Ldc_I4_0:
                    {
                        PushToStack(0);
                        continue;
                    }
                    case MsilInstructionOpCodeValue.Ldc_I4_1:
                    {
                        PushToStack(1);
                        continue;
                    }
                    case MsilInstructionOpCodeValue.Ldc_I4_2:
                    {
                        PushToStack(2);
                        continue;
                    }
                    case MsilInstructionOpCodeValue.Ldc_I4_3:
                    {
                        PushToStack(3);
                        continue;
                    }
                    case MsilInstructionOpCodeValue.Ldc_I4_4:
                    {
                        PushToStack(4);
                        continue;
                    }
                    case MsilInstructionOpCodeValue.Ldc_I4_5:
                    {
                        PushToStack(5);
                        continue;
                    }
                    case MsilInstructionOpCodeValue.Ldc_I4_6:
                    {
                        PushToStack(6);
                        continue;
                    }
                    case MsilInstructionOpCodeValue.Ldc_I4_7:
                    {
                        PushToStack(7);
                        continue;
                    }
                    case MsilInstructionOpCodeValue.Ldc_I4_8:
                    {
                        PushToStack(8);
                        continue;
                    }
                    case MsilInstructionOpCodeValue.Ldc_I4_S:
                    {
                        PushToStack(instruction.Operand);
                        continue;
                    }
                    case MsilInstructionOpCodeValue.Ldc_I4:
                    {
                        PushToStack(instruction.Operand);
                        continue;
                    }
                    case MsilInstructionOpCodeValue.Ldc_I8:
                    {
                        PushToStack(instruction.Operand);
                        continue;
                    }
                    case MsilInstructionOpCodeValue.Ldc_R4:
                    {
                        PushToStack(instruction.Operand);
                        continue;
                    }
                    case MsilInstructionOpCodeValue.Ldc_R8:
                    {
                        PushToStack(instruction.Operand);
                        continue;
                    }
                    case MsilInstructionOpCodeValue.Dup:
                    case MsilInstructionOpCodeValue.Pop:
                    case MsilInstructionOpCodeValue.Jmp:
                    {
                        break;
                    }
                    case MsilInstructionOpCodeValue.Call:
                    {
                        var op = (MethodInfo)instruction.Operand;
                        var tempStack = new Stack<object>();

                        for (var k = 0; k < op.GetParameters().Length; k++)
                        {
                            tempStack.Push(_stack.Pop());
                        }

                        if (op.Name == "WriteLine")
                        {
                            var res0 = NextLocalIdentifier();
                            var pop = tempStack.Pop() as string;

                            if (pop == null)
                            {
                                _localInstructions.AppendLine(string.Format("########## > {0}", instruction.Code));
                                continue;
                            }

                            var str = (string)_globalData[pop];
                            _localInstructions.AppendLine(string.Format("{0} = getelementptr [{1} x i8]* {2}, i64 0, i64 0", res0, str.Length + 1, pop));
                            _localInstructions.AppendFormat("IR_{0:x4}: ", instruction.Offset);
                            _localInstructions.AppendLine(string.Format("call i32 @puts(i8* {0})", res0));
                        }
                        else
                        {
                            _localInstructions.AppendLine(string.Format("call i32 @{0}({1})", op.Name, string.Join(", ", tempStack)));
                        }

                        continue;
                    }
                    case MsilInstructionOpCodeValue.Calli:
                    {
                        break;
                    }
                    case MsilInstructionOpCodeValue.Ret:
                    {
                        EmitRet(instruction);
                        continue;
                    }
                    case MsilInstructionOpCodeValue.Br_S:
                    case MsilInstructionOpCodeValue.Brfalse_S:
                    case MsilInstructionOpCodeValue.Brtrue_S:
                    case MsilInstructionOpCodeValue.Beq_S:
                    case MsilInstructionOpCodeValue.Bge_S:
                    case MsilInstructionOpCodeValue.Bgt_S:
                    case MsilInstructionOpCodeValue.Ble_S:
                    case MsilInstructionOpCodeValue.Blt_S:
                    case MsilInstructionOpCodeValue.Bne_Un_S:
                    case MsilInstructionOpCodeValue.Bge_Un_S:
                    case MsilInstructionOpCodeValue.Bgt_Un_S:
                    case MsilInstructionOpCodeValue.Ble_Un_S:
                    case MsilInstructionOpCodeValue.Blt_Un_S:
                    case MsilInstructionOpCodeValue.Br:
                    case MsilInstructionOpCodeValue.Brfalse:
                    case MsilInstructionOpCodeValue.Brtrue:
                    case MsilInstructionOpCodeValue.Beq:
                    case MsilInstructionOpCodeValue.Bge:
                    case MsilInstructionOpCodeValue.Bgt:
                    case MsilInstructionOpCodeValue.Ble:
                    case MsilInstructionOpCodeValue.Blt:
                    case MsilInstructionOpCodeValue.Bne_Un:
                    case MsilInstructionOpCodeValue.Bge_Un:
                    case MsilInstructionOpCodeValue.Bgt_Un:
                    case MsilInstructionOpCodeValue.Ble_Un:
                    case MsilInstructionOpCodeValue.Blt_Un:
                    case MsilInstructionOpCodeValue.Switch:
                    case MsilInstructionOpCodeValue.Ldind_I1:
                    case MsilInstructionOpCodeValue.Ldind_U1:
                    case MsilInstructionOpCodeValue.Ldind_I2:
                    case MsilInstructionOpCodeValue.Ldind_U2:
                    case MsilInstructionOpCodeValue.Ldind_I4:
                    case MsilInstructionOpCodeValue.Ldind_U4:
                    case MsilInstructionOpCodeValue.Ldind_I8:
                    case MsilInstructionOpCodeValue.Ldind_I:
                    case MsilInstructionOpCodeValue.Ldind_R4:
                    case MsilInstructionOpCodeValue.Ldind_R8:
                    case MsilInstructionOpCodeValue.Ldind_Ref:
                    case MsilInstructionOpCodeValue.Stind_Ref:
                    case MsilInstructionOpCodeValue.Stind_I1:
                    case MsilInstructionOpCodeValue.Stind_I2:
                    case MsilInstructionOpCodeValue.Stind_I4:
                    case MsilInstructionOpCodeValue.Stind_I8:
                    case MsilInstructionOpCodeValue.Stind_R4:
                    case MsilInstructionOpCodeValue.Stind_R8:
                    case MsilInstructionOpCodeValue.Add:
                    case MsilInstructionOpCodeValue.Sub:
                    case MsilInstructionOpCodeValue.Mul:
                    case MsilInstructionOpCodeValue.Div:
                    case MsilInstructionOpCodeValue.Div_Un:
                    case MsilInstructionOpCodeValue.Rem:
                    case MsilInstructionOpCodeValue.Rem_Un:
                    case MsilInstructionOpCodeValue.And:
                    case MsilInstructionOpCodeValue.Or:
                    case MsilInstructionOpCodeValue.Xor:
                    case MsilInstructionOpCodeValue.Shl:
                    case MsilInstructionOpCodeValue.Shr:
                    case MsilInstructionOpCodeValue.Shr_Un:
                    case MsilInstructionOpCodeValue.Neg:
                    case MsilInstructionOpCodeValue.Not:
                    case MsilInstructionOpCodeValue.Conv_I1:
                    case MsilInstructionOpCodeValue.Conv_I2:
                    case MsilInstructionOpCodeValue.Conv_I4:
                    case MsilInstructionOpCodeValue.Conv_I8:
                    case MsilInstructionOpCodeValue.Conv_R4:
                    case MsilInstructionOpCodeValue.Conv_R8:
                    case MsilInstructionOpCodeValue.Conv_U4:
                    case MsilInstructionOpCodeValue.Conv_U8:
                    case MsilInstructionOpCodeValue.Callvirt:
                    case MsilInstructionOpCodeValue.Cpobj:
                    case MsilInstructionOpCodeValue.Ldobj:
                    {
                        break;
                    }
                    case MsilInstructionOpCodeValue.Ldstr:
                    {
                        EmitLdstr(instruction);
                        continue;
                    }
                    case MsilInstructionOpCodeValue.Newobj:
                    case MsilInstructionOpCodeValue.Castclass:
                    case MsilInstructionOpCodeValue.Isinst:
                    case MsilInstructionOpCodeValue.Conv_R_Un:
                    case MsilInstructionOpCodeValue.Unbox:
                    case MsilInstructionOpCodeValue.Throw:
                    case MsilInstructionOpCodeValue.Ldfld:
                    case MsilInstructionOpCodeValue.Ldflda:
                    case MsilInstructionOpCodeValue.Stfld:
                    case MsilInstructionOpCodeValue.Ldsfld:
                    case MsilInstructionOpCodeValue.Ldsflda:
                    case MsilInstructionOpCodeValue.Stsfld:
                    case MsilInstructionOpCodeValue.Stobj:
                    case MsilInstructionOpCodeValue.Conv_Ovf_I1_Un:
                    case MsilInstructionOpCodeValue.Conv_Ovf_I2_Un:
                    case MsilInstructionOpCodeValue.Conv_Ovf_I4_Un:
                    case MsilInstructionOpCodeValue.Conv_Ovf_I8_Un:
                    case MsilInstructionOpCodeValue.Conv_Ovf_U1_Un:
                    case MsilInstructionOpCodeValue.Conv_Ovf_U2_Un:
                    case MsilInstructionOpCodeValue.Conv_Ovf_U4_Un:
                    case MsilInstructionOpCodeValue.Conv_Ovf_U8_Un:
                    case MsilInstructionOpCodeValue.Conv_Ovf_I_Un:
                    case MsilInstructionOpCodeValue.Conv_Ovf_U_Un:
                    case MsilInstructionOpCodeValue.Box:
                    case MsilInstructionOpCodeValue.Newarr:
                    case MsilInstructionOpCodeValue.Ldlen:
                    case MsilInstructionOpCodeValue.Ldelema:
                    case MsilInstructionOpCodeValue.Ldelem_I1:
                    case MsilInstructionOpCodeValue.Ldelem_U1:
                    case MsilInstructionOpCodeValue.Ldelem_I2:
                    case MsilInstructionOpCodeValue.Ldelem_U2:
                    case MsilInstructionOpCodeValue.Ldelem_I4:
                    case MsilInstructionOpCodeValue.Ldelem_U4:
                    case MsilInstructionOpCodeValue.Ldelem_I8:
                    case MsilInstructionOpCodeValue.Ldelem_I:
                    case MsilInstructionOpCodeValue.Ldelem_R4:
                    case MsilInstructionOpCodeValue.Ldelem_R8:
                    case MsilInstructionOpCodeValue.Ldelem_Ref:
                    case MsilInstructionOpCodeValue.Stelem_I:
                    case MsilInstructionOpCodeValue.Stelem_I1:
                    case MsilInstructionOpCodeValue.Stelem_I2:
                    case MsilInstructionOpCodeValue.Stelem_I4:
                    case MsilInstructionOpCodeValue.Stelem_I8:
                    case MsilInstructionOpCodeValue.Stelem_R4:
                    case MsilInstructionOpCodeValue.Stelem_R8:
                    case MsilInstructionOpCodeValue.Stelem_Ref:
                    case MsilInstructionOpCodeValue.Ldelem:
                    case MsilInstructionOpCodeValue.Stelem:
                    case MsilInstructionOpCodeValue.Unbox_Any:
                    case MsilInstructionOpCodeValue.Conv_Ovf_I1:
                    case MsilInstructionOpCodeValue.Conv_Ovf_U1:
                    case MsilInstructionOpCodeValue.Conv_Ovf_I2:
                    case MsilInstructionOpCodeValue.Conv_Ovf_U2:
                    case MsilInstructionOpCodeValue.Conv_Ovf_I4:
                    case MsilInstructionOpCodeValue.Conv_Ovf_U4:
                    case MsilInstructionOpCodeValue.Conv_Ovf_I8:
                    case MsilInstructionOpCodeValue.Conv_Ovf_U8:
                    case MsilInstructionOpCodeValue.Refanyval:
                    case MsilInstructionOpCodeValue.Ckfinite:
                    case MsilInstructionOpCodeValue.Mkrefany:
                    case MsilInstructionOpCodeValue.Ldtoken:
                    case MsilInstructionOpCodeValue.Conv_U2:
                    case MsilInstructionOpCodeValue.Conv_U1:
                    case MsilInstructionOpCodeValue.Conv_I:
                    case MsilInstructionOpCodeValue.Conv_Ovf_I:
                    case MsilInstructionOpCodeValue.Conv_Ovf_U:
                    case MsilInstructionOpCodeValue.Add_Ovf:
                    case MsilInstructionOpCodeValue.Add_Ovf_Un:
                    case MsilInstructionOpCodeValue.Mul_Ovf:
                    case MsilInstructionOpCodeValue.Mul_Ovf_Un:
                    case MsilInstructionOpCodeValue.Sub_Ovf:
                    case MsilInstructionOpCodeValue.Sub_Ovf_Un:
                    case MsilInstructionOpCodeValue.Endfinally:
                    case MsilInstructionOpCodeValue.Leave:
                    case MsilInstructionOpCodeValue.Leave_S:
                    case MsilInstructionOpCodeValue.Stind_I:
                    case MsilInstructionOpCodeValue.Conv_U:
                    case MsilInstructionOpCodeValue.Prefix7:
                    case MsilInstructionOpCodeValue.Prefix6:
                    case MsilInstructionOpCodeValue.Prefix5:
                    case MsilInstructionOpCodeValue.Prefix4:
                    case MsilInstructionOpCodeValue.Prefix3:
                    case MsilInstructionOpCodeValue.Prefix2:
                    case MsilInstructionOpCodeValue.Prefix1:
                    case MsilInstructionOpCodeValue.Prefixref:
                    case MsilInstructionOpCodeValue.Arglist:
                    case MsilInstructionOpCodeValue.Ceq:
                    case MsilInstructionOpCodeValue.Cgt:
                    case MsilInstructionOpCodeValue.Cgt_Un:
                    case MsilInstructionOpCodeValue.Clt:
                    case MsilInstructionOpCodeValue.Clt_Un:
                    case MsilInstructionOpCodeValue.Ldftn:
                    case MsilInstructionOpCodeValue.Ldvirtftn:
                    case MsilInstructionOpCodeValue.Ldarg:
                    case MsilInstructionOpCodeValue.Ldarga:
                    case MsilInstructionOpCodeValue.Starg:
                    case MsilInstructionOpCodeValue.Ldloc:
                    case MsilInstructionOpCodeValue.Ldloca:
                    case MsilInstructionOpCodeValue.Stloc:
                    case MsilInstructionOpCodeValue.Localloc:
                    case MsilInstructionOpCodeValue.Endfilter:
                    case MsilInstructionOpCodeValue.Unaligned:
                    case MsilInstructionOpCodeValue.Volatile:
                    case MsilInstructionOpCodeValue.Tail:
                    case MsilInstructionOpCodeValue.Initobj:
                    case MsilInstructionOpCodeValue.Constrained:
                    case MsilInstructionOpCodeValue.Cpblk:
                    case MsilInstructionOpCodeValue.Initblk:
                    case MsilInstructionOpCodeValue.Sizeof:
                    case MsilInstructionOpCodeValue.Rethrow:
                    case MsilInstructionOpCodeValue.Refanytype:
                    case MsilInstructionOpCodeValue.Readonly:
                    {
                        break;
                    }
                }

                //// Todo: We need a mapping to make this faster!
                //if (instruction.Code == System.Reflection.Emit.OpCodes.Brtrue_S)
                //{
                //    var arg0 = stack.Pop();
                //    var res0 = string.Format("%{0}", localCounter);

                //    sb.AppendLine(string.Format("{0} = icmp i32 {1}, true", res0, arg0));
                //    sb.AppendFormat("\tIR_{0:x4}: ", instruction.Offset);
                //    sb.AppendLine(string.Format("br i32 {0}, label IR_{1:x4}", res0, instruction.Operand));
                //    continue;
                //}

                //if (instruction.Code == System.Reflection.Emit.OpCodes.Br_S)
                //{
                //    var arg0 = stack.Pop();
                //    var res0 = string.Format("%{0}", localCounter);

                //    sb.AppendLine(string.Format("{0} = icmp i32 {1}, true", res0, arg0));
                //    sb.AppendFormat("\tIR_{0:x4}: ", instruction.Offset);
                //    sb.AppendLine(string.Format("br i32 {0}, label %true label %false", res0));
                //    continue;
                //}
                
                //if (instruction.Code == System.Reflection.Emit.OpCodes.Box)
                //{
                //    sb.AppendLine("__________");
                //    continue;
                //}
                

                //if (instruction.Code == System.Reflection.Emit.OpCodes.Ldarg_1)
                //{
                //    var param = method.GetParameters();
                //    var name = method.IsStatic
                //        ? param[1].Name
                //        : param[0].Name;

                //    stack.Push(string.Format("%{0}", name));
                //    sb.AppendLine("__________");
                //    continue;
                //}


                //if (instruction.Code == System.Reflection.Emit.OpCodes.Ldarg_2)
                //{
                //    var param = method.GetParameters();
                //    var name = method.IsStatic
                //        ? param[2].Name
                //        : param[1].Name;

                //    stack.Push(string.Format("%{0}", name));
                //    sb.AppendLine("__________");
                //    continue;
                //}
                
                //if (instruction.Code == System.Reflection.Emit.OpCodes.Conv_R4)
                //{
                //    var arg0 = stack.Pop();
                //    var res0 = string.Format("%{0}", localCounter);

                //    sb.AppendLine(string.Format("{0} = bitcast i32 {1} to float", res0, arg0));
                //    stack.Push(res0);
                //    continue;
                //}

                //if (instruction.Code == System.Reflection.Emit.OpCodes.Conv_R8)
                //{
                //    var arg0 = stack.Pop();
                //    var res0 = string.Format("%{0}", localCounter);

                //    sb.AppendLine(string.Format("{0} = bitcast i32 {1} to double", res0, arg0));
                //    stack.Push(res0);
                //    continue;
                //}

                //if (instruction.Code == System.Reflection.Emit.OpCodes.Add)
                //{
                //    var arg0 = stack.Pop();
                //    var arg1 = stack.Pop();
                //    var res0 = string.Format("%{0}", localCounter);

                //    sb.AppendLine(string.Format("{0} = add i32 {1} {2}", res0, arg0, arg1));
                //    stack.Push(res0);
                //    continue;
                //}

                //if (instruction.Code == System.Reflection.Emit.OpCodes.Mul)
                //{
                //    var arg0 = stack.Pop();
                //    var arg1 = stack.Pop();
                //    var res0 = string.Format("%{0}", localCounter);

                //    sb.AppendLine(string.Format("{0} = mul i32 {1} {2}", res0, arg0, arg1));
                //    stack.Push(res0);
                //    continue;
                //}

                //if (instruction.Code == System.Reflection.Emit.OpCodes.Sub)
                //{
                //    var arg0 = stack.Pop();
                //    var arg1 = stack.Pop();
                //    var res0 = string.Format("%{0}", localCounter);

                //    sb.AppendLine(string.Format("{0} = sub i32 {1} {2}", res0, arg0, arg1));
                //    stack.Push(res0);
                //    continue;
                //}

                //if (instruction.Code == System.Reflection.Emit.OpCodes.Div)
                //{
                //    var arg0 = stack.Pop();
                //    var arg1 = stack.Pop();
                //    var res0 = string.Format("%{0}", localCounter);

                //    sb.AppendLine(string.Format("{0} = div i32 {1} {2}", res0, arg0, arg1));
                //    stack.Push(res0);
                //    continue;
                //}

                //if (instruction.Code == System.Reflection.Emit.OpCodes.Rem)
                //{
                //    var arg0 = stack.Pop();
                //    var arg1 = stack.Pop();
                //    var res0 = string.Format("%{0}", localCounter);

                //    sb.AppendLine(string.Format("{0} = srem i32 {1} {2}", res0, arg0, arg1));
                //    stack.Push(res0);
                //    continue;
                //}
                
                //if (instruction.Code == System.Reflection.Emit.OpCodes.Call || instruction.Code == System.Reflection.Emit.OpCodes.Callvirt)
                //{
                //    var op = (MethodInfo)instruction.Operand;
                //    var tempStack = new Stack<object>();

                //    for (var k = 0; k < op.GetParameters().Length; k++)
                //    {
                //        tempStack.Push(stack.Pop());
                //    }

                //    if (op.Name == "WriteLine")
                //    {
                //        var res0 = string.Format("%{0}", localCounter);
                //        var pop = tempStack.Pop() as string;

                //        if (pop == null)
                //        {
                //            sb.AppendLine(string.Format("########## > {0}", instruction.Code));
                //            continue;
                //        }

                //        var str = (string)globalData[pop];
                //        sb.AppendLine(string.Format("{0} = getelementptr [{1} x i8]* {2}, i64 0, i64 0", res0, str.Length + 1, pop));
                //        sb.AppendFormat("IR_{0:x4}: ", instruction.Offset);
                //        sb.AppendLine(string.Format("call i32 @puts(i8* {0})", res0));
                //    }
                //    else
                //    {
                //        sb.AppendLine(string.Format("call i32 @{0}({1})", op.Name, string.Join(", ", tempStack)));
                //    }

                //    continue;
                //}

                _localInstructions.AppendLine(string.Format("########## > {0}", instruction.Code));
            }

            return string.Format("{0}\r\n{1}", _globalInstructions, _localInstructions);
        }
        
        // Helpers - Instructions
        private void EmitNop(MsilInstruction instruction)
        {
            _localInstructions.Append(CreatePreamble(instruction));
            _localInstructions.AppendLine("call void @llvm.donothing()");
        }

        private void EmitRet(MsilInstruction instruction)
        {
            _localInstructions.Append(CreatePreamble(instruction));
            _localInstructions.AppendLine("ret void");
        }

        private void EmitLdstr(MsilInstruction instruction)
        {
            var operand = (string)instruction.Operand;
            var identifier = NextGlobalIdentifier();

            _stack.Push(identifier);
            _globalData.Add(identifier, operand);
            _globalInstructions.Append(CreatePreamble(instruction));
            _globalInstructions.AppendLine(string.Format("{0} = constant [{1} x i8] c\"{2}\\00\"", identifier, operand.Length + 1, operand));
        }

        // Helpers
        private string NextLocalIdentifier()
        {
            return string.Format("%{0}", _localIdentifierCounter++);
        }

        private string NextGlobalIdentifier()
        {
            return string.Format("@{0}", _globalIdentifierCounter++);
        }

        private void StoreInLocal(int index)
        {
            _locals[index] = _stack.Pop();
        }

        private void PushToStack(object value)
        {
            _stack.Push(value);
        }

        // Helpers - Static
        private static string CreatePreamble(MsilInstruction instruction)
        {
            return string.Format("IR_{0:x4}: ", instruction.Offset);
        }
    }
}