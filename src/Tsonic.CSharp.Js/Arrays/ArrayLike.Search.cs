using System;

namespace Tsonic.CSharp.Js;

public static partial class ArrayLike
{
    public static bool includes<T, TKey>(IArrayLike<T> source, T searchElement, int fromIndex = 0)
        where TKey : struct, ICollectionKey<T, TKey>
    {
        var length = source.Length;
        var start = fromIndex < 0 ? System.Math.Max(length + fromIndex, 0) : fromIndex;
        if (start >= length) return false;
        var search = TKey.Create(searchElement);
        for (var index = start; index < length; index++)
            if (TKey.Create(ReadSearchElement(source, index)).Equals(search)) return true;
        return false;
    }

    public static int indexOf<T, TKey>(IArrayLike<T> source, T searchElement, int fromIndex = 0)
        where TKey : struct, ICollectionKey<T, TKey>
    {
        var length = source.Length;
        var start = fromIndex < 0 ? System.Math.Max(length + fromIndex, 0) : fromIndex;
        if (start >= length) return -1;
        var search = TKey.Create(searchElement);
        if (search.IsNaN) return -1;
        for (var index = start; index < length; index++)
            if (TKey.Create(ReadSearchElement(source, index)).Equals(search)) return index;
        return -1;
    }

    public static int lastIndexOf<T, TKey>(IArrayLike<T> source, T searchElement, int? fromIndex = null)
        where TKey : struct, ICollectionKey<T, TKey>
    {
        var length = source.Length;
        var start = fromIndex ?? length - 1;
        start = start < 0 ? length + start : System.Math.Min(start, length - 1);
        if (start < 0) return -1;
        var search = TKey.Create(searchElement);
        if (search.IsNaN) return -1;
        for (var index = start; index >= 0; index--)
            if (TKey.Create(ReadSearchElement(source, index)).Equals(search)) return index;
        return -1;
    }

    private static T ReadSearchElement<T>(IArrayLike<T> source, int index) => source.TryGet(index, out var value)
        ? value : throw new InvalidOperationException("Checked array search requires initialized dense storage.");
}
