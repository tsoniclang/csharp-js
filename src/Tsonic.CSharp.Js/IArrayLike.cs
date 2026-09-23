namespace Tsonic.CSharp.Js;

/// <summary>Presence-preserving indexed reads over existing JavaScript array storage.</summary>
public interface IArrayLike<T>
{
    /// <summary>The current number of array slots.</summary>
    int Length { get; }
    /// <summary>Reads an initialized integer index, distinguishing it from an invalid index.</summary>
    bool TryGet(double index, out T value);
}

/// <summary>Closed operations shared by typed and ordinary JavaScript arrays.</summary>
public static class ArrayLike
{
    public static bool HasIndex<T>(double index, JSArray<T> source) =>
        source.hasIndex(index);

    /// <summary>Reads a numeric element or returns the absent-value carrier.</summary>
    public static double? ReadNumber(IArrayLike<double> source, double index) =>
        source.TryGet(index, out var value) ? value : null;

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
