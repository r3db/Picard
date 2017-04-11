using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Alea
{
    // Todo: Refactor!
    // Todo: Rename!
    public static class ReflectionExtensions
    {
        internal static bool IsLightweightMethod(this MethodBase method)
        {
            return method is DynamicMethod || typeof(DynamicMethod).GetNestedType("RTDynamicMethod", BindingFlags.NonPublic).IsInstanceOfType(method);
        }

        public static ITokenResolver GetTokenResolver(this MethodBase method)
        {
            var dynamicMethod = TryGetDynamicMethod(method as MethodInfo) ?? method as DynamicMethod;
            return dynamicMethod != null
                ? new DynamicMethodTokenResolver(dynamicMethod)
                : (ITokenResolver)new ModuleTokenResolver(method.Module);
        }

        public static byte[] GetILBytes(this MethodBase method)
        {
            var dynamicMethod = TryGetDynamicMethod(method as MethodInfo) ?? method as DynamicMethod;
            return dynamicMethod != null
                ? GetILBytes(dynamicMethod)
                : method.GetMethodBody()?.GetILAsByteArray();
        }

        internal static byte[] GetILBytes(this DynamicMethod dynamicMethod)
        {
            var resolver = typeof(DynamicMethod).GetField("m_resolver", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(dynamicMethod);
            if (resolver == null) throw new ArgumentException("The dynamic method's IL has not been finalized.");
            return (byte[])resolver.GetType().GetField("m_code", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(resolver);
        }

        internal static DynamicMethod TryGetDynamicMethod(MethodInfo rtDynamicMethod)
        {
            var typeRTDynamicMethod = typeof(DynamicMethod).GetNestedType("RTDynamicMethod", BindingFlags.NonPublic);
            return typeRTDynamicMethod.IsInstanceOfType(rtDynamicMethod)
                ? (DynamicMethod)typeRTDynamicMethod.GetField("m_owner", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(rtDynamicMethod)
                : null;
        }
    }
}