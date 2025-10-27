using System.Runtime.InteropServices;

namespace PrevisionalAccountManager.ViewModels;

public static class CollectionExtensions
{
    public static bool IsNotEmpty<T>(this ICollection<T> source)
    {
        return source is { Count: > 0 };
    }

    public static Span<T> AsSpan<T>(this List<T> source)
    {
        return CollectionsMarshal.AsSpan(source);
    }
}