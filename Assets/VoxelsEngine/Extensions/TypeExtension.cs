using System;

namespace VoxelsEngine.Extensions
{
    public static class TypeExtension
    {
        public static bool HasDefaultConstructor(this Type t)
        {
            return t.IsValueType || t.GetConstructor(Type.EmptyTypes) != null;
        }
    }
}