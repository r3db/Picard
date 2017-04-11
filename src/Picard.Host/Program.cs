using System;
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
            };
            
            var method0 = action0.Method;

            DumpIL(method0);
            DumpIR(method0);
        }

        private static void DumpIL(MethodInfo method)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            var instructions = new MsilInstructionDecoder(method.GetMethodBody().GetILAsByteArray(), method.Module).DecodeAll();

            foreach (var instruction in instructions)
            {
                Console.WriteLine("\t" + MsilInstructionRepresenter.Represent(instruction, method));
            }

            Console.ResetColor();
            Console.WriteLine(new string('-', 110));
        }

        private static void DumpIR(MethodInfo method)
        {
            var result = IREmiter.Emit(method);
            var lines = result.Split(new[] { Environment.NewLine }, StringSplitOptions.None).Where(x => x.Contains("__________") == false).ToList();

            Console.ForegroundColor = ConsoleColor.Red;

            foreach (var item in lines)
            {
                Console.ForegroundColor = item.Contains("########## >")
                    ? ConsoleColor.Yellow
                    : ConsoleColor.Red;

                Console.WriteLine("\t" + item);
            }

            Console.ResetColor();
            Console.WriteLine(new string('-', 110));
        }
    }
}