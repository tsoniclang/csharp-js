using System;

namespace Tsonic.CSharp.Js;

/// <summary>Presence-preserving indexed reads over existing JavaScript array storage.</summary>
public interface IArrayLike<T>
{
    /// <summary>The current number of array slots.</summary>
    int Length { get; }
    /// <summary>Reads an initialized integer index, distinguishing it from an invalid index.</summary>
    bool TryGet(int index, out T value);
    bool TryGetNumericProperty(double key, out T value);
}

/// <summary>Closed operations shared by typed and ordinary JavaScript arrays.</summary>
public static class ArrayLike
{
    public static bool HasIndex<T>(char index, IArrayLike<T> source) => source.TryGet(index, out _);
    public static bool HasIndex<T>(sbyte index, IArrayLike<T> source) => HasIndex((int)index, source);
    public static bool HasIndex<T>(byte index, IArrayLike<T> source) => source.TryGet(index, out _);
    public static bool HasIndex<T>(short index, IArrayLike<T> source) => HasIndex((int)index, source);
    public static bool HasIndex<T>(ushort index, IArrayLike<T> source) => source.TryGet(index, out _);
    public static bool HasIndex<T>(int index, IArrayLike<T> source) => index < 0
        ? source.TryGetNumericProperty(index, out _) : source.TryGet(index, out _);
    public static bool HasIndex<T>(uint index, IArrayLike<T> source) =>
        index < (uint)source.Length && source.TryGet((int)index, out _);
    public static bool HasIndex<T>(long index, IArrayLike<T> source) => index < 0
        ? HasNegativeIndex(index, source) : index < source.Length && source.TryGet((int)index, out _);
    public static bool HasIndex<T>(ulong index, IArrayLike<T> source) =>
        index < (ulong)source.Length && source.TryGet((int)index, out _);
    public static bool HasIndex<T>(nint index, IArrayLike<T> source) => index < 0
        ? HasNegativeIndex(index, source) : index < source.Length && source.TryGet((int)index, out _);
    public static bool HasIndex<T>(nuint index, IArrayLike<T> source) =>
        index < (nuint)source.Length && source.TryGet((int)index, out _);
    public static bool HasIndex<T>(Int128 index, IArrayLike<T> source) => index < 0
        ? HasNegativeIndex(index, source) : index < source.Length && source.TryGet((int)index, out _);
    public static bool HasIndex<T>(UInt128 index, IArrayLike<T> source) =>
        index < (UInt128)source.Length && source.TryGet((int)index, out _);

    public static bool HasIndex<T>(double index, IArrayLike<T> source) =>
        source.TryGetNumericProperty(index, out _);

    private static bool HasNegativeIndex<T>(Int128 index, IArrayLike<T> source)
    {
        var propertyKey = (double)index;
        return Int128.CreateSaturating(propertyKey) == index && source.TryGetNumericProperty(propertyKey, out _);
    }

    /// <summary>Reads a numeric element or returns the absent-value carrier.</summary>
    public static T? ReadValue<T>(IArrayLike<T> source, int index) where T : struct
    {
        if (index < 0) return ReadNumericProperty(source, index);
        return source.TryGet(index, out var value) ? value : null;
    }

    public static T? ReadNumericProperty<T>(IArrayLike<T> source, double key) where T : struct =>
        source.TryGetNumericProperty(key, out var value) ? value : null;

    /// <summary>Copies a statically proven dense array into independent ordinary-array storage.</summary>
    public static JSArray<T> CopyDense<T>(IArrayLike<T> source)
    {
        var length = source.Length;
        var result = JSArray<T>.createWithCapacity(length);
        for (var index = 0; index < length; index++)
        {
            if (!source.TryGet(index, out var value))
                throw new System.InvalidOperationException("Checked array density invariant violated.");
            result[index] = value;
        }
        return result;
    }
}
