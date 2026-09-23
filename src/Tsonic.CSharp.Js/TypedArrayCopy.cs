using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Tsonic.CSharp.Js;

internal static class TypedArrayCopy
{
    public static void Checked<TSource, TDestination>(ReadOnlySpan<TSource> source, Span<TDestination> destination)
        where TSource : unmanaged, INumberBase<TSource>
        where TDestination : unmanaged, INumberBase<TDestination>
    {
        if (source.Length != destination.Length) throw new ArgumentException("Typed array copy lengths differ.");
        if (typeof(TSource) == typeof(TDestination))
        {
            MemoryMarshal.Cast<TSource, TDestination>(source).CopyTo(destination);
            return;
        }
        source = SnapshotOverlap(source, destination);
        for (var index = 0; index < source.Length; index++)
            destination[index] = TDestination.CreateChecked(source[index]);
    }

    public static void Clamped<TSource>(ReadOnlySpan<TSource> source, Span<byte> destination)
        where TSource : unmanaged, INumberBase<TSource>
    {
        if (source.Length != destination.Length) throw new ArgumentException("Typed array copy lengths differ.");
        if (typeof(TSource) == typeof(byte))
        {
            MemoryMarshal.Cast<TSource, byte>(source).CopyTo(destination);
            return;
        }
        source = SnapshotOverlap(source, destination);
        for (var index = 0; index < source.Length; index++)
        {
            var value = source[index];
            destination[index] = TSource.IsInteger(value)
                ? byte.CreateSaturating(value)
                : TypedArrayNumbers.ToUint8Clamp(double.CreateChecked(value));
        }
    }

    private static ReadOnlySpan<TSource> SnapshotOverlap<TSource, TDestination>(ReadOnlySpan<TSource> source, Span<TDestination> destination)
        where TSource : unmanaged
        where TDestination : unmanaged =>
        MemoryMarshal.AsBytes(source).Overlaps(MemoryMarshal.AsBytes(destination))
            ? source.ToArray() : source;
}
