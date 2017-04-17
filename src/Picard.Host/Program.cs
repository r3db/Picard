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
            Action action0 = () =>
            {
                Console.WriteLine("Some String 1");
                Console.WriteLine("Some String 2");
                Console.WriteLine("Some String 3");
            };

            var method0 = action0.Method;

            DumpIL(method0);
            Console.WriteLine(new string('-', 110));

            DumpLLVM(method0);
            Console.WriteLine(new string('-', 110));

            CudaDriver.Initialize();
            NvvmInterop(method0);

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

        private static void PrintStatistics(int length, double swElapsedMilliseconds)
        {
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "\t{0:F2}ms for {1} instructions.", swElapsedMilliseconds, length));
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "\t{0:F2}μs per instruction.", swElapsedMilliseconds / length * 1000f));
            Console.WriteLine();
        }

        private static void NvvmInterop(MethodInfo method)
        {
            Console.WriteLine(NvvmDriver.Version);
            Console.WriteLine(NvvmDriver.IRVersion);

            var sw = Stopwatch.StartNew();

            var program = NvvmDriver.CreateProgram();

            var llvm = LLVMEmiter.Emit(method);
            
            NvvmDriver.AddModuleToProgram(program, llvm);
            var ptx = NvvmDriver.CompileProgram(program);
            
            var device = CudaDriver.GetDevice(0);

            Console.WriteLine(CudaDriver.GetDeviceCount());
            Console.WriteLine(CudaDriver.GetDeviceName(0));
            Console.WriteLine(CudaDriver.GetComputeCapability(device));

            var ctx = CudaDriver.CreateContext(0);
            var mod = CudaDriver.LoadModule(ptx);
            var function = CudaDriver.ModuleGetFunction(mod, "main");

            Console.WriteLine(sw.ElapsedMilliseconds);

            CudaDriver.LaunchKernel(function);
            CudaDriver.CtxSynchronize();
            NvvmDriver.DestroyProgram(program);
        }
    }
}