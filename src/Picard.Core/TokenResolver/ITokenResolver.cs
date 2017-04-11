using System;
using System.Reflection;

namespace Alea
{
    public interface ITokenResolver
    {
        Type ResolveType(int metadataToken);
        MemberInfo ResolveMember(int metadataToken);
        FieldInfo ResolveField(int metadataToken);
        string ResolveString(int metadataToken);
    }
}