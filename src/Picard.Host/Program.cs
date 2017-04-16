using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Picard
{
    // C:\Program Files\NVIDIA GPU Computing Toolkit\CUDA\v8.0\nvvm\libnvvm-samples\simple
    internal static class Program
    {
        private static void Main()
        {
            NvvmInterop();

            //Action action0 = () =>
            //{
            //    Console.WriteLine("Some String 1");
            //    Console.WriteLine("Some String 2");
            //    Console.WriteLine("Some String 3");
            //};

            //var method0 = action0.Method;

            //DumpIL(method0);
            //Console.WriteLine(new string('-', 110));

            //DumpLLVM(method0);
            //Console.WriteLine(new string('-', 110));

            //Console.ReadLine();
        }

        private static void NvvmInterop()
        {
            Console.WriteLine(NvvmDriver.Version);
            Console.WriteLine(NvvmDriver.IRVersion);

            var program = NvvmDriver.CreateProgram();

            const string llvm = @"
                target triple = ""nvptx64 - unknown - cuda""
                target datalayout = ""e-p:64:64:64-i1:8:8-i8:8:8-i16:16:16-i32:32:32-i64:64:64-f32:32:32-f64:64:64-v16:16:16-v32:32:32-v64:64:64-v128:128:128-n16:32:64""

                @.1_0 = constant[14 x i8] c""Some String 1\00""
                @.1_1 = constant[14 x i8] c""Some String 2\00""
                @.1_2 = constant[14 x i8] c""Some String 3\00""

                define void @main() {
                entry:
                    call void @llvm.donothing()
                    %.0 = getelementptr[14 x i8] * @.1_0, i64 0, i64 0
                    call i32 @puts(i8 * %.0)
                    call void @llvm.donothing()
                    %.1 = getelementptr[14 x i8] * @.1_1, i64 0, i64 0
                    call i32 @puts(i8 * %.1)
                    call void @llvm.donothing()
                    %.2 = getelementptr[14 x i8] * @.1_2, i64 0, i64 0
                    call i32 @puts(i8 * %.2)
                    call void @llvm.donothing()
                    ret void
                }

                declare i32 @puts(i8 *)
                declare void @llvm.donothing() nounwind readnone
            ";
            
            NvvmDriver.AddModuleToProgram(program, llvm);
            NvvmDriver.CompileProgram(program);
            var ptx = NvvmDriver.GetCompiledResult(program);

            NvvmDriver.DestroyProgram(program);
        }

        private static void DumpIL(MethodInfo method)
        {
            var sw = Stopwatch.StartNew();

            var msil = method.GetMethodBody()?.GetILAsByteArray();
            var instructions = new MsilInstructionDecoder(msil, method.Module).DecodeAll().ToArray();

            PrintStatistics(instructions.Length, sw.Elapsed.TotalMilliseconds);

            Console.ForegroundColor = ConsoleColor.Cyan;

            foreach (var instruction in instructions)
            {
                Console.WriteLine("\t" + MsilInstructionRepresenter.Represent(instruction, method));
            }

            Console.ResetColor();
        }

        private static void DumpLLVM(MethodInfo method)
        {
            var sw = Stopwatch.StartNew();

            var lines = LLVMEmiter.Emit(method)
                .Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                .ToArray();

            PrintStatistics(lines.Length, sw.Elapsed.TotalMilliseconds);

            for (var i = 0; i < lines.Length - 1; i++)
            {
                var item = lines[i];
                Console.ForegroundColor = item.Contains("########## >")
                    ? ConsoleColor.Yellow
                    : ConsoleColor.Red;

                Console.WriteLine("\t" + item);
            }

            Console.ResetColor();
        }

        private static void PrintStatistics(int length, double swElapsedMilliseconds)
        {
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "\t{0:F2}ms for {1} instructions.", swElapsedMilliseconds, length));
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "\t{0:F2}μs per instruction.", swElapsedMilliseconds / length * 1000f));
            Console.WriteLine();
        }
    }
}