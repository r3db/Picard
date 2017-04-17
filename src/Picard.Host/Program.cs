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
            NvvmInterop();

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

        private static void NvvmInterop()
        {
            Console.WriteLine(NvvmDriver.Version);
            Console.WriteLine(NvvmDriver.IRVersion);

            var sw = Stopwatch.StartNew();

            var program = NvvmDriver.CreateProgram();

            const string llvm = @"
                target triple = ""nvptx64 - unknown - cuda""
                target datalayout = ""e-p:64:64:64-i1:8:8-i8:8:8-i16:16:16-i32:32:32-i64:64:64-f32:32:32-f64:64:64-v16:16:16-v32:32:32-v64:64:64-v128:128:128-n16:32:64""

                @_1_0 = private addrspace(4) constant [14 x i8] c""Some Text 2\0D\0A\00""

                define void @main() {
                entry:
                    %_0 = call i8* @llvm.nvvm.ptr.constant.to.gen.p0i8.p4i8(i8 addrspace(4)* getelementptr inbounds ([14 x i8] addrspace(4)* @_1_0, i64 0, i64 0))
                    call i32 @vprintf(i8* %_0, i8* null)
                    call void @llvm.donothing()
                    ret void
                }
                
                declare i8* @llvm.nvvm.ptr.constant.to.gen.p0i8.p4i8(i8 addrspace(4)*) #0
                declare i32 @vprintf(i8* nocapture, i8*) #1
                declare void @llvm.donothing() #0

                attributes #0 = { nounwind readnone }
                attributes #1 = { nounwind }

                !nvvmir.version = !{!0}
                !nvvm.annotations = !{!1}

                !0 = metadata !{i32 1, i32 2, i32 2, i32 0}
                !1 = metadata !{void ()* @main, metadata !""kernel"", i32 1}
            ";

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