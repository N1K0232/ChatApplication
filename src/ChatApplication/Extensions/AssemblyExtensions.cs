using System.Reflection;

namespace ChatApplication.Extensions;

public static class AssemblyExtensions
{
    public static string GetAttribute<T>(this Assembly assembly, Func<T, string> value) where T : Attribute
    {
        var attribute = GetAttributeInternal<T>(assembly);
        return value.Invoke(attribute);
    }

    private static T GetAttributeInternal<T>(Assembly assembly) where T : Attribute
        => (T)Attribute.GetCustomAttribute(assembly, typeof(T));
}