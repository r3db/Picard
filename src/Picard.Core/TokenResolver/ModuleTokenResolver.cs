using System;
using System.Reflection;

namespace Alea
{
    internal sealed class ModuleTokenResolver : ITokenResolver
    {
        // Internal Instance Data
        private readonly Module module;

        // .Ctor
        internal ModuleTokenResolver(Module module)
        {
            this.module = module;
        }

        // Methods
        public Type ResolveType(int metadataToken)
        {
            return module.ResolveType(metadataToken);
        }

        public MemberInfo ResolveMember(int metadataToken)
        {
            return module.ResolveMember(metadataToken);
        }

        public FieldInfo ResolveField(int metadataToken)
        {
            return module.ResolveField(metadataToken);
        }

        public string ResolveString(int metadataToken)
        {
            return module.ResolveString(metadataToken);
        }
    }
}