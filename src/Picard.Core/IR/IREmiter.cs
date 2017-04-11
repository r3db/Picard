using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Picard
{
    public static class IREmiter
    {
        // Todo: Refactor!
        public static string Emit(MethodInfo method)
        {
            var sb = new StringBuilder();
            var stack = new Stack<object>();
            var instructions = new MsilInstructionDecoder(method.GetMethodBody()?.GetILAsByteArray(), method.Module).DecodeAll().ToList();
            var locals = new object[100];

            var global = new StringBuilder();
            var globalData = new Dictionary<string, object>();

            int localCounter = 0;
            int globalCounter = 0;

            for (var i = 0; i < instructions.Count; ++i)
            {
                var instruction = instructions[i];
            
                sb.AppendFormat("IR_{0:x4}: ", instruction.Offset);

                switch (instruction.Code.Name)
                {
                    case "nop":
                    {
                        sb.AppendLine("call void @llvm.donothing()");
                        continue;
                    }
                    case "break":
                    case "ldarg.0":
                    case "ldarg.1":
                    case "ldarg.2":
                    case "ldarg.3":
                    case "ldloc.0":
                    case "ldloc.1":
                    case "ldloc.2":
                    case "ldloc.3":
                    case "stloc.0":
                    case "stloc.1":
                    case "stloc.2":
                    case "stloc.3":
                    case "ldarg.s":
                    case "ldarga.s":
                    case "starg.s":
                    case "ldloc.s":
                    case "ldloca.s":
                    case "stloc.s":
                    case "ldnull":
                    case "ldc.i4.m1":
                    case "ldc.i4.0":
                    case "ldc.i4.1":
                    case "ldc.i4.2":
                    case "ldc.i4.3":
                    case "ldc.i4.4":
                    case "ldc.i4.5":
                    case "ldc.i4.6":
                    case "ldc.i4.7":
                    case "ldc.i4.8":
                    case "ldc.i4.s":
                    case "ldc.i4":
                    case "ldc.i8":
                    case "ldc.r4":
                    case "ldc.r8":
                    case "dup":
                    case "pop":
                    case "jmp":
                    {
                        break;
                    }
                    case "call":
                    {
                        var op = (MethodInfo)instruction.Operand;
                        var tempStack = new Stack<object>();

                        for (var k = 0; k < op.GetParameters().Length; k++)
                        {
                            tempStack.Push(stack.Pop());
                        }

                        if (op.Name == "WriteLine")
                        {
                            var res0 = string.Format("%{0}", localCounter);
                            var pop = tempStack.Pop() as string;

                            if (pop == null)
                            {
                                sb.AppendLine(string.Format("########## > {0}", instruction.Code));
                                continue;
                            }

                            var str = (string)globalData[pop];
                            sb.AppendLine(string.Format("{0} = getelementptr [{1} x i8]* {2}, i64 0, i64 0", res0, str.Length + 1, pop));
                            sb.AppendFormat("IR_{0:x4}: ", instruction.Offset);
                            sb.AppendLine(string.Format("call i32 @puts(i8* {0})", res0));
                        }
                        else
                        {
                            sb.AppendLine(string.Format("call i32 @{0}({1})", op.Name, string.Join(", ", tempStack)));
                        }

                        continue;
                    }
                    case "calli":
                    case "ret":
                    case "br.s":
                    case "brfalse.s":
                    case "brtrue.s":
                    case "beq.s":
                    case "bge.s":
                    case "bgt.s":
                    case "ble.s":
                    case "blt.s":
                    case "bne.un.s":
                    case "bge.un.s":
                    case "bgt.un.s":
                    case "ble.un.s":
                    case "blt.un.s":
                    case "br":
                    case "brfalse":
                    case "brtrue":
                    case "beq":
                    case "bge":
                    case "bgt":
                    case "ble":
                    case "blt":
                    case "bne.un":
                    case "bge.un":
                    case "bgt.un":
                    case "ble.un":
                    case "blt.un":
                    case "switch":
                    case "ldind.i1":
                    case "ldind.u1":
                    case "ldind.i2":
                    case "ldind.u2":
                    case "ldind.i4":
                    case "ldind.u4":
                    case "ldind.i8":
                    case "ldind.i":
                    case "ldind.r4":
                    case "ldind.r8":
                    case "ldind.ref":
                    case "stind.ref":
                    case "stind.i1":
                    case "stind.i2":
                    case "stind.i4":
                    case "stind.i8":
                    case "stind.r4":
                    case "stind.r8":
                    case "add":
                    case "sub":
                    case "mul":
                    case "div":
                    case "div.un":
                    case "rem":
                    case "rem.un":
                    case "and":
                    case "or":
                    case "xor":
                    case "shl":
                    case "shr":
                    case "shr.un":
                    case "neg":
                    case "not":
                    case "conv.i1":
                    case "conv.i2":
                    case "conv.i4":
                    case "conv.i8":
                    case "conv.r4":
                    case "conv.r8":
                    case "conv.u4":
                    case "conv.u8":
                    case "callvirt":
                    case "cpobj":
                    case "ldobj":
                    {
                        break;
                    }
                    case "ldstr":
                    {
                        var identifier = string.Format("@global_{0}", globalCounter++);
                        stack.Push(identifier);

                        var str = (string)instruction.Operand;

                        globalData.Add(identifier, str);
                        global.AppendLine(string.Format("{0} = constant [{1} x i8] c\"{2}\\00\"", identifier, str.Length + 1, str));
                        continue;
                    }
                    case "newobj":
                    case "castclass":
                    case "isinst":
                    case "conv.r.un":
                    case "unbox":
                    case "throw":
                    case "ldfld":
                    case "ldflda":
                    case "stfld":
                    case "ldsfld":
                    case "ldsflda":
                    case "stsfld":
                    case "stobj":
                    case "conv.ovf.i1.un":
                    case "conv.ovf.i2.un":
                    case "conv.ovf.i4.un":
                    case "conv.ovf.i8.un":
                    case "conv.ovf.u1.un":
                    case "conv.ovf.u2.un":
                    case "conv.ovf.u4.un":
                    case "conv.ovf.u8.un":
                    case "conv.ovf.i.un":
                    case "conv.ovf.u.un":
                    case "box":
                    case "newarr":
                    case "ldlen":
                    case "ldelema":
                    case "ldelem.i1":
                    case "ldelem.u1":
                    case "ldelem.i2":
                    case "ldelem.u2":
                    case "ldelem.i4":
                    case "ldelem.u4":
                    case "ldelem.i8":
                    case "ldelem.i":
                    case "ldelem.r4":
                    case "ldelem.r8":
                    case "ldelem.ref":
                    case "stelem.i":
                    case "stelem.i1":
                    case "stelem.i2":
                    case "stelem.i4":
                    case "stelem.i8":
                    case "stelem.r4":
                    case "stelem.r8":
                    case "stelem.ref":
                    case "ldelem":
                    case "stelem":
                    case "unbox.any":
                    case "conv.ovf.i1":
                    case "conv.ovf.u1":
                    case "conv.ovf.i2":
                    case "conv.ovf.u2":
                    case "conv.ovf.i4":
                    case "conv.ovf.u4":
                    case "conv.ovf.i8":
                    case "conv.ovf.u8":
                    case "refanyval":
                    case "ckfinite":
                    case "mkrefany":
                    case "ldtoken":
                    case "conv.u2":
                    case "conv.u1":
                    case "conv.i":
                    case "conv.ovf.i":
                    case "conv.ovf.u":
                    case "add.ovf":
                    case "add.ovf.un":
                    case "mul.ovf":
                    case "mul.ovf.un":
                    case "sub.ovf":
                    case "sub.ovf.un":
                    case "endfinally":
                    case "leave":
                    case "leave.s":
                    case "stind.i":
                    case "conv.u":
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

                //if (instruction.Code == System.Reflection.Emit.OpCodes.Ldstr)
                //{
                //    var res0 = string.Format("@gn_{0}", globalCounter++);
                //    --localCounter;
                //    stack.Push(res0);

                //    var str = (string)instruction.Operand;

                //    globalData.Add(res0, str);

                //    global.AppendLine(string.Format("{0} = constant [{1} x i8] c\"{2}\\00\"", res0, str.Length + 1, str));
                //    sb.AppendLine("__________");
                //    continue;
                //}

                //if (instruction.Code == System.Reflection.Emit.OpCodes.Stloc_0)
                //{
                //    locals[0] = stack.Pop();
                //    sb.AppendLine("__________");
                //    continue;
                //}

                //if (instruction.Code == System.Reflection.Emit.OpCodes.Stloc_1)
                //{
                //    locals[1] = stack.Pop();
                //    sb.AppendLine("__________");
                //    continue;
                //}

                //if (instruction.Code == System.Reflection.Emit.OpCodes.Stloc_2)
                //{
                //    locals[2] = stack.Pop();
                //    sb.AppendLine("__________");
                //    continue;
                //}


                //if (instruction.Code == System.Reflection.Emit.OpCodes.Stloc_3)
                //{
                //    locals[3] = stack.Pop();
                //    sb.AppendLine("__________");
                //    continue;
                //}

                //if (instruction.Code == System.Reflection.Emit.OpCodes.Box)
                //{
                //    sb.AppendLine("__________");
                //    continue;
                //}

                //if (instruction.Code == System.Reflection.Emit.OpCodes.Ldloc_0)
                //{
                //    stack.Push(locals[0]);
                //    sb.AppendLine("__________");
                //    continue;
                //}

                //if (instruction.Code == System.Reflection.Emit.OpCodes.Ldloc_1)
                //{
                //    stack.Push(locals[1]);
                //    sb.AppendLine("__________");
                //    continue;
                //}

                //if (instruction.Code == System.Reflection.Emit.OpCodes.Ldloc_2)
                //{
                //    stack.Push(locals[2]);
                //    sb.AppendLine("__________");
                //    continue;
                //}

                //if (instruction.Code == System.Reflection.Emit.OpCodes.Ldloc_3)
                //{
                //    stack.Push(locals[3]);
                //    sb.AppendLine("__________");
                //    continue;
                //}

                //if (instruction.Code == System.Reflection.Emit.OpCodes.Ldc_I4)
                //{
                //    stack.Push(instruction.Operand);
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

                //if (instruction.Code == System.Reflection.Emit.OpCodes.Ldc_I4_S)
                //{
                //    stack.Push(string.Format("{0}", instruction.Operand));
                //    sb.AppendLine("__________");
                //    continue;
                //}

                ////ldc.i4.s

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

                //if (instruction.Code == System.Reflection.Emit.OpCodes.Ldc_R4)
                //{
                //    stack.Push(string.Format("%{0}", instruction.Operand));
                //    sb.AppendLine("__________");
                //    continue;
                //}

                //if (instruction.Code == System.Reflection.Emit.OpCodes.Ldnull)
                //{
                //    stack.Push(string.Format("%{0}", "null"));
                //    sb.AppendLine("__________");
                //    continue;
                //}

                //if (instruction.Code == System.Reflection.Emit.OpCodes.Ret)
                //{
                //    sb.AppendLine("ret void");
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

                //if (instruction.Code == System.Reflection.Emit.OpCodes.Nop)
                //{
                //    sb.AppendLine("call void @llvm.donothing()");
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

                sb.AppendLine(string.Format("########## > {0}", instruction.Code));
            }

            return string.Format("{0}\r\n{1}", global, sb);
        }
    }
}