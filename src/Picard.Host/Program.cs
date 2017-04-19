using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Picard
{
    internal static class Program
    {
        private static void Main()
        {
            Action action = () =>
            {
                Console.WriteLine("Some String 1\r\n");
                Console.WriteLine("Some String 2\r\n");
                Console.WriteLine("Some String 3\r\n");
            };

            DumpIL(action.Method);
            Console.WriteLine(new string('-', 110));

            DumpLLVM(action.Method);
            Console.WriteLine(new string('-', 110));
            
            ExecuteOnDevice(action.Method);
            Console.WriteLine(new string('-', 110));

            Console.ReadLine();
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

        private static void PrintStatistics(int length, double elapsedMilliseconds)
        {
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "\t{0:F2}ms for {1} instructions.", elapsedMilliseconds, length));
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "\t{0:F2}μs per instruction.", elapsedMilliseconds / length * 1000f));
            Console.WriteLine();
        }

        private static void ExecuteOnDevice(MethodInfo method)
        {
            CudaDriver.Initialize();
            CudaDriver.CreateContext(0);

            var sw = Stopwatch.StartNew();
            
            var module = CudaDriver.LoadModule(ExtractPTX(method));
            var kernel = CudaDriver.ModuleGetKernel(module, "main");

            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0:F2}ms", sw.ElapsedMilliseconds));

            Console.ForegroundColor = ConsoleColor.Cyan;
            CudaDriver.LaunchKernel(kernel);
            CudaDriver.CtxSynchronize();
            Console.ResetColor();
        }

        private static string ExtractPTX(MethodInfo method)
        {
            var program = NvvmDriver.CreateProgram();

            NvvmDriver.AddModuleToProgram(program, LLVMEmiter.Emit(method));
            var ptx = NvvmDriver.CompileProgram(program);
            NvvmDriver.DestroyProgram(program);
            return ptx;
        }
    }
}