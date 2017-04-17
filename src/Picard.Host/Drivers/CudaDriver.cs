using System;
using System.Runtime.InteropServices;
using System.Text;
using Alea.CudaToolkit;

namespace Picard
{
    internal static class CudaDriver
    {
        internal static void Initialize()
        {
            var result = Cuda.cuInit(0);

            if (result != cudaError_enum.CUDA_SUCCESS)
            {
                throw new InvalidProgramException(result.ToString());
            }
        }

        internal static unsafe int GetDeviceCount()
        {
            int deviceCount;
            var result = Cuda.cuDeviceGetCount(&deviceCount);

            if (result != cudaError_enum.CUDA_SUCCESS)
            {
                throw new InvalidProgramException(result.ToString());
            }

            return deviceCount;
        }

        internal static string GetDeviceName(int index)
        {
            var name = new StringBuilder(100);
            var result = Cuda.cuDeviceGetName(name, 100, index);

            if (result != cudaError_enum.CUDA_SUCCESS)
            {
                throw new InvalidProgramException(result.ToString());
            }

            return name.ToString();
        }

        internal static unsafe Version GetComputeCapability(int device)
        {
            int major;
            int minor;
            var result = Cuda.cuDeviceComputeCapability(&major, &minor, device);

            if (result != cudaError_enum.CUDA_SUCCESS)
            {
                throw new InvalidProgramException(result.ToString());
            }

            return new Version(major, minor);
        }

        internal static unsafe int GetDevice(int index)
        {
            int cuDevice;
            var result = Cuda.cuDeviceGet(&cuDevice, index);

            if (result != cudaError_enum.CUDA_SUCCESS)
            {
                throw new InvalidProgramException(result.ToString());
            }

            return cuDevice;
        }

        internal static unsafe CUctx_st CreateContext(int index)
        {
            CUctx_st context;
            var result = Cuda.cuCtxCreate(&context, 0, index);

            if (result != cudaError_enum.CUDA_SUCCESS)
            {
                throw new InvalidProgramException(result.ToString());
            }

            return context;
        }
        
        internal static unsafe CUmod_st LoadModule(string ptx)
        {
            // Todo: We need to get the address of 'Ptx'!
            CUmod_st module;
            var result = cuModuleLoadDataEx(&module, ptx, 0, 0, IntPtr.Zero);

            if (result != cudaError_enum.CUDA_SUCCESS)
            {
                throw new InvalidProgramException(result.ToString());
            }

            return module;
        }

        internal static unsafe CUfunc_st ModuleGetFunction(CUmod_st module, string function)
        {
            CUfunc_st func;
            var result = Cuda.cuModuleGetFunction(&func, module, function);

            if (result != cudaError_enum.CUDA_SUCCESS)
            {
                throw new InvalidProgramException(result.ToString());
            }

            return func;
        }

        internal static void LaunchKernel(CUfunc_st function)
        {
            const uint threads = 5;
            const uint blocks = 1;

            var stream = new CUstream_st();

            var result = Cuda.cuLaunchKernel(function, blocks, 1, 1, threads, 1, 1, 0, stream, IntPtr.Zero, IntPtr.Zero);

            if (result != cudaError_enum.CUDA_SUCCESS)
            {
                throw new InvalidProgramException(result.ToString());
            }
        }

        internal static void CtxSynchronize()
        {
            var result = Cuda.cuCtxSynchronize();

            if (result != cudaError_enum.CUDA_SUCCESS)
            {
                throw new InvalidProgramException(result.ToString());
            }
        }

        // Todo: Remove!
        [DllImport("nvcuda.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private static extern unsafe cudaError_enum cuModuleLoadDataEx(CUmod_st* module, string ptx, int numOptions, [In] CUjit_option_enum options, IntPtr optionValues);
    }
}