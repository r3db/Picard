using System;
using System.Collections;
using System.Reflection;

namespace Picard
{
    internal sealed class LlvmMethodEmiter
    {
        #region Intrinsic Translator Registry

        // Internal Static Data
        private static readonly LlvmMethodTranslator _translator = new LlvmMethodTranslator
        {
            LlvmMethodTranslator.Register("void", "Console.WriteLine", new[] {"bool"                                           }, null),
            LlvmMethodTranslator.Register("void", "Console.WriteLine", new[] {"char"                                           }, null),
            LlvmMethodTranslator.Register("void", "Console.WriteLine", new[] {"char[]"                                         }, null),
            LlvmMethodTranslator.Register("void", "Console.WriteLine", new[] {"char[]", "int", "int"                           }, null),
            LlvmMethodTranslator.Register("void", "Console.WriteLine", new[] {"decimal"                                        }, null),
            LlvmMethodTranslator.Register("void", "Console.WriteLine", new[] {"double"                                         }, null),
            LlvmMethodTranslator.Register("void", "Console.WriteLine", new[] {"float"                                          }, null),
            LlvmMethodTranslator.Register("void", "Console.WriteLine", new[] {"int"                                            }, null),
            LlvmMethodTranslator.Register("void", "Console.WriteLine", new[] {"uint"                                           }, null),
            LlvmMethodTranslator.Register("void", "Console.WriteLine", new[] {"long"                                           }, null),
            LlvmMethodTranslator.Register("void", "Console.WriteLine", new[] {"ulong"                                          }, null),
            LlvmMethodTranslator.Register("void", "Console.WriteLine", new[] {"object"                                         }, null),
            LlvmMethodTranslator.Register("void", "Console.WriteLine", new[] {"string"                                         }, IntrinsicWriteLineForString),
            LlvmMethodTranslator.Register("void", "Console.WriteLine", new[] {"string", "object"                               }, null),
            LlvmMethodTranslator.Register("void", "Console.WriteLine", new[] {"string", "object", "object"                     }, null),
            LlvmMethodTranslator.Register("void", "Console.WriteLine", new[] {"string", "object", "object", "object"           }, null),
            LlvmMethodTranslator.Register("void", "Console.WriteLine", new[] {"string", "object", "object", "object", "object" }, null),
            LlvmMethodTranslator.Register("void", "Console.WriteLine", new[] {"string", "object[]"                             }, null),
        };

        #endregion

        // Internal Instance Data
        private readonly LvvmMethodEmiterState _state;
        private readonly MethodBody _body;
        private readonly byte[] _msil;
        private readonly Module _module;
        
        // .Ctor
        private LlvmMethodEmiter(MethodBase method, Func<string> directiveIdentifierGenerator)
        {
            _state = new LvvmMethodEmiterState(directiveIdentifierGenerator);
            _body = method.GetMethodBody();
            _msil = _body.GetILAsByteArray();
            _module = method.Module;
        }
        
        // Factory .Ctor
        internal static LlvmMethodEmiter Emit(MethodBase method, Func<string> directiveIdentifierGenerator)
        {
            var instance = new LlvmMethodEmiter(method, directiveIdentifierGenerator);
            instance.Emit();
            return instance;
        }

        // Properties - Readonly
        internal string Code => _state.Instructions.ToString();

        internal string Directives => _state.Directives.ToString();

        // Helpers
        private void Emit()
        {
            var locals = new ArrayList(_body.LocalVariables.Count);

            _state.Instructions.AppendLine("define void @main() {");
            _state.Instructions.AppendLine("entry:");

            foreach (var instruction in new MsilInstructionDecoder(_msil, _module).DecodeAll())
            {
                if (_state.Labels.Contains(instruction.Offset))
                {
                    EmitLabel(instruction);
                }

                switch (instruction.OpCodeValue)
                {
                    case MsilInstructionOpCodeValue.Nop:
                    {
                        EmitNop();
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
                    case MsilInstructionOpCodeValue.Ldloc_1:
                    case MsilInstructionOpCodeValue.Ldloc_2:
                    case MsilInstructionOpCodeValue.Ldloc_3:
                    {
                        var index = (int)MsilInstructionOpCodeValue.Ldloc_0 - (int)instruction.OpCodeValue;
                        _state.InstructionStack.Push(locals[index]);
                        continue;
                    }
                    case MsilInstructionOpCodeValue.Stloc_0:
                    case MsilInstructionOpCodeValue.Stloc_1:
                    case MsilInstructionOpCodeValue.Stloc_2:
                    case MsilInstructionOpCodeValue.Stloc_3:
                    {
                        var index = (int)MsilInstructionOpCodeValue.Stloc_0 - (int)instruction.OpCodeValue;
                        locals[index] = _state.InstructionStack.Pop();
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
                        _state.InstructionStack.Push("null");
                        continue;
                    }
                    case MsilInstructionOpCodeValue.Ldc_I4_M1:
                    {
                        _state.InstructionStack.Push(-1);
                        continue;
                    }
                    case MsilInstructionOpCodeValue.Ldc_I4_0:
                    case MsilInstructionOpCodeValue.Ldc_I4_1:
                    case MsilInstructionOpCodeValue.Ldc_I4_2:
                    case MsilInstructionOpCodeValue.Ldc_I4_3:
                    case MsilInstructionOpCodeValue.Ldc_I4_4:
                    case MsilInstructionOpCodeValue.Ldc_I4_5:
                    case MsilInstructionOpCodeValue.Ldc_I4_6:
                    case MsilInstructionOpCodeValue.Ldc_I4_7:
                    case MsilInstructionOpCodeValue.Ldc_I4_8:
                    {
                        var value = (int)MsilInstructionOpCodeValue.Ldc_I4_0 - (int)instruction.OpCodeValue;
                        _state.InstructionStack.Push(value);
                        continue;
                    }
                    case MsilInstructionOpCodeValue.Ldc_I4_S:
                    case MsilInstructionOpCodeValue.Ldc_I4:
                    case MsilInstructionOpCodeValue.Ldc_I8:
                    case MsilInstructionOpCodeValue.Ldc_R4:
                    case MsilInstructionOpCodeValue.Ldc_R8:
                    {
                        _state.InstructionStack.Push(instruction.Operand);
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
                        EmitCall(instruction);
                        continue;
                    }
                    case MsilInstructionOpCodeValue.Calli:
                    {
                        break;
                    }
                    case MsilInstructionOpCodeValue.Ret:
                    {
                        EmitRet();
                        continue;
                    }
                    case MsilInstructionOpCodeValue.Br_S:
                    {
                        EmitBrS(instruction);
                        continue;
                    }
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
                    {
                        break;
                    }
                    case MsilInstructionOpCodeValue.Rem:
                    {
                        EmitRem();
                        continue;
                    }
                    case MsilInstructionOpCodeValue.Rem_Un:
                    {
                        EmitRemUn();
                        continue;
                    }
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

                // Todo: Continue!
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
                
                _state.AppendPreamble();
                _state.Instructions.AppendLine(string.Format("########## > {0}", instruction.Code));
            }

            _state.Instructions.AppendLine("}");
        }
        
        // Helpers - Instructions
        private void EmitLabel(MsilInstruction instruction)
        {
            _state.Instructions.AppendLine(string.Format("IR_{0:x4}: ", instruction.Offset));
        }

        private void EmitNop()
        {
            _state.AppendPreamble();
            _state.Instructions.AppendLine("call void @llvm.donothing()");
        }
        
        private void EmitCall(MsilInstruction instruction)
        {
            var method = (MethodInfo)instruction.Operand;
            var intrinsic = _translator.Resolve(method);

            if (intrinsic != null)
            {
                intrinsic.Invoke(_state);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        
        private void EmitRet()
        {
            _state.AppendPreamble();
            _state.Instructions.AppendLine("ret void");
        }

        private void EmitBrS(MsilInstruction instruction)
        {
            var label = (int)instruction.Operand;

            if (_state.Labels.Contains(label) == false)
            {
                _state.Labels.Add(label);
            }

            _state.AppendPreamble();
            _state.Instructions.AppendLine(string.Format("br label %IR_{0:x4}", label));
        }

        private void EmitRem()
        {
            var arg0 = _state.InstructionStack.Pop();
            var arg1 = _state.InstructionStack.Pop();
            var identifier = _state.NextInstructionIdentifier();

            // Todo: We need to figure the iN size!
            _state.AppendPreamble();
            _state.Instructions.AppendLine(string.Format("{0} = srem i32 {1} {2}", identifier, arg0, arg1));
            _state.InstructionStack.Push(identifier);
        }

        private void EmitRemUn()
        {
            var arg0 = _state.InstructionStack.Pop();
            var arg1 = _state.InstructionStack.Pop();
            var identifier = _state.NextInstructionIdentifier();

            // Todo: We need to figure the iN size!
            _state.AppendPreamble();
            _state.Instructions.AppendLine(string.Format("{0} = urem i32 {1} {2}", identifier, arg0, arg1));
            _state.InstructionStack.Push(identifier);
        }

        // Todo: Reuse Strings?
        private void EmitLdstr(MsilInstruction instruction)
        {
            var operand = new LlvmString((string)instruction.Operand);
            var identifier = _state.NextDirectiveIdentifier();
            
            _state.InstructionStack.Push(identifier);
            _state.DirectiveStack.Push(operand);
            _state.Directives.AppendLine(string.Format("{0} = private addrspace(4) constant [{1} x i8] c\"{2}\\00\"", identifier, operand.Original.Length + 1, operand.Encoded));
        }

        #region Intrinsic Translators

        // Todo: Support C Format Style "%A"
        private static void IntrinsicWriteLineForString(LvvmMethodEmiterState state)
        {
            var identifier = state.NextInstructionIdentifier();
            var argument = state.InstructionStack.Pop();
            var str = (LlvmString)state.DirectiveStack.Pop();

            state.AppendPreamble();
            state.Instructions.AppendLine(string.Format("{0} = call i8* @llvm.nvvm.ptr.constant.to.gen.p0i8.p4i8(i8 addrspace(4)* getelementptr inbounds ([{1} x i8] addrspace(4)* {2}, i64 0, i64 0))", identifier, str.Original.Length + 1, argument));
            state.AppendPreamble();
            state.Instructions.AppendLine(string.Format("call i32 @vprintf(i8* {0}, i8* null)", identifier));
        }

        #endregion
    }
}