using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Alea
{
    internal static class Program
    {
        private static void Main()
        {
            Action action0 = () =>
            {
                Console.WriteLine("Some String 1");
            };

            Expression<Action> action1 = () => Console.WriteLine(234);

            var method0 = action0.Method;
            var method1 = action1.Compile().Method;

            DumpIL(method0);
            DumpIR(method0);
            DumpIL(method1);
            DumpIR(method1);
        }

        private static void DumpIL(MethodInfo method)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            var instructions = new MsilInstructionDecoder(method.GetILBytes(), method.GetTokenResolver()).DecodeAll();

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