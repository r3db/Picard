using System;
using System.Text;
using Alea;

namespace Picard
{
    using NvvmProgram = _nvvmProgram;

    internal static class NvvmDriver
    {
        // Properties
        internal static unsafe Version Version
        {
            get
            {
                int majorIR;
                int minorIR;
                Nvvm.nvvmVersion(&majorIR, &minorIR);
                return new Version(majorIR, minorIR);
            }
        }

        internal static unsafe Version IRVersion
        {
            get
            {
                int majorIR;
                int minorIR;
                int majorDebug;
                int minorDebug;
                Nvvm.nvvmIRVersion(&majorIR, &minorIR, &majorDebug, &minorDebug);
                return new Version(majorIR, minorIR, majorDebug, minorDebug);
            }
        }

        // Methods
        internal static unsafe NvvmProgram CreateProgram()
        {
            NvvmProgram program;

            var result = Nvvm.nvvmCreateProgram(&program);

            if (result != nvvmResult.NVVM_SUCCESS)
            {
                throw new InvalidProgramException();
            }

            return program;
        }

        internal static unsafe void DestroyProgram(NvvmProgram program)
        {
            var result = Nvvm.nvvmDestroyProgram(&program);

            if (result != nvvmResult.NVVM_SUCCESS)
            {
                throw new InvalidProgramException();
            }
        }

        internal static void AddModuleToProgram(NvvmProgram program, string llvm)
        {
            var size = (ulong)Encoding.ASCII.GetByteCount(llvm);

            if (Nvvm.nvvmAddModuleToProgram(program, llvm, size, null) != nvvmResult.NVVM_SUCCESS)
            {
                throw new InvalidProgramException();
            }
        }

        internal static string CompileProgram(NvvmProgram program)
        {
            if (Nvvm.nvvmCompileProgram(program, 0, null) != nvvmResult.NVVM_SUCCESS)
            {
                throw new InvalidProgramException(GetProgramLog(program));
            }

            var buffer = new StringBuilder((int)GetCompiledResultSize(program));

            if (Nvvm.nvvmGetCompiledResult(program, buffer) != nvvmResult.NVVM_SUCCESS)
            {
                throw new InvalidProgramException();
            }

            return buffer.ToString();
        }
        
        // Helpers
        private static unsafe ulong GetCompiledResultSize(NvvmProgram program)
        {
            ulong size;

            if (Nvvm.nvvmGetCompiledResultSize(program, &size) != nvvmResult.NVVM_SUCCESS)
            {
                throw new InvalidProgramException();
            }

            return size;
        }

        private static unsafe ulong GetProgramLogSize(NvvmProgram program)
        {
            ulong size;

            if (Nvvm.nvvmGetProgramLogSize(program, &size) != nvvmResult.NVVM_SUCCESS)
            {
                throw new InvalidProgramException();
            }

            return size;
        }

        private static string GetProgramLog(NvvmProgram program)
        {
            var buffer = new StringBuilder((int)GetProgramLogSize(program));

            if (Nvvm.nvvmGetProgramLog(program, buffer) != nvvmResult.NVVM_SUCCESS)
            {
                throw new InvalidProgramException();
            }

            return buffer.ToString();
        }
    }
}