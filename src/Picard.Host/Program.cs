using System;
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

            Console.ForegroundColor = ConsoleColor.Cyan;
            DumpIL(method0);
            Console.WriteLine(new string('-', 110));
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Red;
            DumpIR(method0);
            Console.WriteLine(new string('-', 110));
            Console.ResetColor();
        }

        private static void DumpIL(MethodInfo method)
        {
            var msil = method.GetMethodBody()?.GetILAsByteArray();
            var instructions = new MsilInstructionDecoder(msil, method.Module).DecodeAll();

            foreach (var instruction in instructions)
            {
                Console.WriteLine("\t" + MsilInstructionRepresenter.Represent(instruction, method));
            }
        }

        private static void DumpIR(MethodInfo method)
        {
            var emiter = new IREmiter(method);

            foreach (var item in emiter.Emit().Split(new[] { Environment.NewLine }, StringSplitOptions.None))
            {
                Console.ForegroundColor = item.Contains("########## >")
                    ? ConsoleColor.Yellow
                    : ConsoleColor.Red;

                Console.WriteLine("\t" + item);
            }
        }
    }
}