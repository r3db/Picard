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
            var instructions = new MsilInstructionDecoder(method.GetMethodBody().GetILAsByteArray(), method.Module).DecodeAll().ToList();
            var locals = new object[100];

            var global = new StringBuilder();
            var globalData = new Dictionary<string, object>();

            int localCounter = 0;
            int globalCounter = 0;

            for (var i = 0; i < instructions.Count; ++i, ++localCounter)
            {
                var instruction = instructions[i];
            
                sb.AppendFormat("IR_{0:x4}: ", instruction.Offset);

                // Todo: We need a mapping to make this faster!
                if (instruction.Code == System.Reflection.Emit.OpCodes.Brtrue_S)
                {
                    var arg0 = stack.Pop();
                    var res0 = string.Format("%{0}", localCounter);

                    sb.AppendLine(string.Format("{0} = icmp i32 {1}, true", res0, arg0));
                    sb.AppendFormat("\tIR_{0:x4}: ", instruction.Offset);
                    sb.AppendLine(string.Format("br i32 {0}, label IR_{1:x4}", res0, instruction.Operand));
                    continue;
                }

                if (instruction.Code == System.Reflection.Emit.OpCodes.Br_S)
                {
                    var arg0 = stack.Pop();
                    var res0 = string.Format("%{0}", localCounter);

                    sb.AppendLine(string.Format("{0} = icmp i32 {1}, true", res0, arg0));
                    sb.AppendFormat("\tIR_{0:x4}: ", instruction.Offset);
                    sb.AppendLine(string.Format("br i32 {0}, label %true label %false", res0));
                    continue;
                }

                if (instruction.Code == System.Reflection.Emit.OpCodes.Ldstr)
                {
                    var res0 = string.Format("@gn_{0}", globalCounter++);
                    --localCounter;
                    stack.Push(res0);

                    var str = (string)instruction.Operand;

                    globalData.Add(res0, str);

                    global.AppendLine(string.Format("{0} = constant [{1} x i8] c\"{2}\\00\"", res0, str.Length + 1, str));
                    sb.AppendLine("__________");
                    continue;
                }

                if (instruction.Code == System.Reflection.Emit.OpCodes.Stloc_0)
                {
                    locals[0] = stack.Pop();
                    sb.AppendLine("__________");
                    continue;
                }

                if (instruction.Code == System.Reflection.Emit.OpCodes.Stloc_1)
                {
                    locals[1] = stack.Pop();
                    sb.AppendLine("__________");
                    continue;
                }

                if (instruction.Code == System.Reflection.Emit.OpCodes.Stloc_2)
                {
                    locals[2] = stack.Pop();
                    sb.AppendLine("__________");
                    continue;
                }


                if (instruction.Code == System.Reflection.Emit.OpCodes.Stloc_3)
                {
                    locals[3] = stack.Pop();
                    sb.AppendLine("__________");
                    continue;
                }

                if (instruction.Code == System.Reflection.Emit.OpCodes.Box)
                {
                    sb.AppendLine("__________");
                    continue;
                }

                if (instruction.Code == System.Reflection.Emit.OpCodes.Ldloc_0)
                {
                    stack.Push(locals[0]);
                    sb.AppendLine("__________");
                    continue;
                }

                if (instruction.Code == System.Reflection.Emit.OpCodes.Ldloc_1)
                {
                    stack.Push(locals[1]);
                    sb.AppendLine("__________");
                    continue;
                }

                if (instruction.Code == System.Reflection.Emit.OpCodes.Ldloc_2)
                {
                    stack.Push(locals[2]);
                    sb.AppendLine("__________");
                    continue;
                }

                if (instruction.Code == System.Reflection.Emit.OpCodes.Ldloc_3)
                {
                    stack.Push(locals[3]);
                    sb.AppendLine("__________");
                    continue;
                }

                if (instruction.Code == System.Reflection.Emit.OpCodes.Ldc_I4)
                {
                    stack.Push(instruction.Operand);
                    sb.AppendLine("__________");
                    continue;
                }

                if (instruction.Code == System.Reflection.Emit.OpCodes.Ldarg_1)
                {
                    var param = method.GetParameters();
                    var name = method.IsStatic
                        ? param[1].Name
                        : param[0].Name;

                    stack.Push(string.Format("%{0}", name));
                    sb.AppendLine("__________");
                    continue;
                }

                if (instruction.Code == System.Reflection.Emit.OpCodes.Ldc_I4_S)
                {
                    stack.Push(string.Format("{0}", instruction.Operand));
                    sb.AppendLine("__________");
                    continue;
                }

                //ldc.i4.s

                if (instruction.Code == System.Reflection.Emit.OpCodes.Ldarg_2)
                {
                    var param = method.GetParameters();
                    var name = method.IsStatic
                        ? param[2].Name
                        : param[1].Name;

                    stack.Push(string.Format("%{0}", name));
                    sb.AppendLine("__________");
                    continue;
                }

                if (instruction.Code == System.Reflection.Emit.OpCodes.Ldc_R4)
                {
                    stack.Push(string.Format("%{0}", instruction.Operand));
                    sb.AppendLine("__________");
                    continue;
                }

                if (instruction.Code == System.Reflection.Emit.OpCodes.Ldnull)
                {
                    stack.Push(string.Format("%{0}", "null"));
                    sb.AppendLine("__________");
                    continue;
                }

                if (instruction.Code == System.Reflection.Emit.OpCodes.Ret)
                {
                    sb.AppendLine("ret void");
                    continue;
                }

                if (instruction.Code == System.Reflection.Emit.OpCodes.Conv_R4)
                {
                    var arg0 = stack.Pop();
                    var res0 = string.Format("%{0}", localCounter);

                    sb.AppendLine(string.Format("{0} = bitcast i32 {1} to float", res0, arg0));
                    stack.Push(res0);
                    continue;
                }

                if (instruction.Code == System.Reflection.Emit.OpCodes.Conv_R8)
                {
                    var arg0 = stack.Pop();
                    var res0 = string.Format("%{0}", localCounter);

                    sb.AppendLine(string.Format("{0} = bitcast i32 {1} to double", res0, arg0));
                    stack.Push(res0);
                    continue;
                }

                if (instruction.Code == System.Reflection.Emit.OpCodes.Add)
                {
                    var arg0 = stack.Pop();
                    var arg1 = stack.Pop();
                    var res0 = string.Format("%{0}", localCounter);

                    sb.AppendLine(string.Format("{0} = add i32 {1} {2}", res0, arg0, arg1));
                    stack.Push(res0);
                    continue;
                }

                if (instruction.Code == System.Reflection.Emit.OpCodes.Mul)
                {
                    var arg0 = stack.Pop();
                    var arg1 = stack.Pop();
                    var res0 = string.Format("%{0}", localCounter);

                    sb.AppendLine(string.Format("{0} = mul i32 {1} {2}", res0, arg0, arg1));
                    stack.Push(res0);
                    continue;
                }

                if (instruction.Code == System.Reflection.Emit.OpCodes.Sub)
                {
                    var arg0 = stack.Pop();
                    var arg1 = stack.Pop();
                    var res0 = string.Format("%{0}", localCounter);

                    sb.AppendLine(string.Format("{0} = sub i32 {1} {2}", res0, arg0, arg1));
                    stack.Push(res0);
                    continue;
                }

                if (instruction.Code == System.Reflection.Emit.OpCodes.Div)
                {
                    var arg0 = stack.Pop();
                    var arg1 = stack.Pop();
                    var res0 = string.Format("%{0}", localCounter);

                    sb.AppendLine(string.Format("{0} = div i32 {1} {2}", res0, arg0, arg1));
                    stack.Push(res0);
                    continue;
                }

                if (instruction.Code == System.Reflection.Emit.OpCodes.Rem)
                {
                    var arg0 = stack.Pop();
                    var arg1 = stack.Pop();
                    var res0 = string.Format("%{0}", localCounter);

                    sb.AppendLine(string.Format("{0} = srem i32 {1} {2}", res0, arg0, arg1));
                    stack.Push(res0);
                    continue;
                }

                if (instruction.Code == System.Reflection.Emit.OpCodes.Nop)
                {
                    sb.AppendLine("call void @llvm.donothing()");
                    continue;
                }

                if (instruction.Code == System.Reflection.Emit.OpCodes.Call || instruction.Code == System.Reflection.Emit.OpCodes.Callvirt)
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

                sb.AppendLine(string.Format("########## > {0}", instruction.Code));
            }

            return string.Format("{0}\r\n{1}", global, sb);
        }
    }
}