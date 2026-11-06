using System;
using System.Collections.Generic;
using System.Linq;
using Tsonic.CSharp.Runtime;
using Xunit;

namespace Tsonic.CSharp.Js.Tests;

public sealed class NativeSurfaceTypeTests
{
    [Fact]
    public void EveryTypedArrayExposesItsNativeElementAndIntegerIndex()
    {
        AssertElement<Int8Array, sbyte>(new Int8Array(1));
        AssertElement<Uint8Array, byte>(new Uint8Array(1));
        AssertElement<Uint8ClampedArray, byte>(new Uint8ClampedArray(1));
        AssertElement<Int16Array, short>(new Int16Array(1));
        AssertElement<Uint16Array, ushort>(new Uint16Array(1));
        AssertElement<Int32Array, int>(new Int32Array(1));
        AssertElement<Uint32Array, uint>(new Uint32Array(1));
        AssertElement<Float32Array, float>(new Float32Array(1));
        AssertElement<Float64Array, double>(new Float64Array(1));
    }

    private static void AssertElement<TArray, TElement>(TArray value)
        where TArray : TypedArray<TArray, TElement>
        where TElement : unmanaged, System.Numerics.INumberBase<TElement>
    {
        var indexer = typeof(TArray).GetProperty("Item");
        Assert.NotNull(indexer);
        Assert.Equal(typeof(TElement), indexer.PropertyType);
        Assert.Equal(typeof(int), Assert.Single(indexer.GetIndexParameters()).ParameterType);
        IArrayLike<TElement> source = value;
        Assert.True(source.TryGet(0, out var initialized));
        Assert.Equal(TElement.Zero, initialized);
        Assert.False(source.TryGet(1, out _));
        Assert.Null(value.at(1));
        Assert.Equal(TElement.Zero, value.at(0));
    }

    [Fact]
    public void DataViewReadsRetainEveryNativeWidth()
    {
        var view = new DataView(new ArrayBuffer(32));
        view.setInt8(0, -1);
        view.setUint8(1, 255);
        view.setInt16(2, short.MinValue, true);
        view.setUint16(4, ushort.MaxValue, true);
        view.setInt32(6, int.MinValue, true);
        view.setUint32(10, uint.MaxValue, true);
        view.setFloat32(14, 1.25f, true);
        view.setFloat64(18, 1.25, true);
        sbyte signedByte = view.getInt8(0);
        byte unsignedByte = view.getUint8(1);
        short signedShort = view.getInt16(2, true);
        ushort unsignedShort = view.getUint16(4, true);
        int signedWord = view.getInt32(6, true);
        uint unsignedWord = view.getUint32(10, true);
        float single = view.getFloat32(14, true);
        double wide = view.getFloat64(18, true);
        Assert.Equal(-1, signedByte);
        Assert.Equal(byte.MaxValue, unsignedByte);
        Assert.Equal(short.MinValue, signedShort);
        Assert.Equal(ushort.MaxValue, unsignedShort);
        Assert.Equal(int.MinValue, signedWord);
        Assert.Equal(uint.MaxValue, unsignedWord);
        Assert.Equal(1.25f, single);
        Assert.Equal(1.25, wide);
    }

    [Fact]
    public void ConstrainedAtRetainsRealAbsenceAndInitializedZeroWithoutBoxing()
    {
        var numbers = new JSArray<int>(new[] { 0, 5 });
        int? zero = Tsonic.CSharp.Js.Array.atValue(numbers, 0);
        int? absent = Tsonic.CSharp.Js.Array.atValue(numbers, int.MaxValue);
        Assert.Equal(0, zero);
        Assert.Null(absent);
        Assert.Null(Tsonic.CSharp.Js.Array.atValue(numbers, int.MinValue));
        var text = new JSArray<string>(new[] { "kept" });
        string? present = Tsonic.CSharp.Js.Array.atReference(text, -1);
        string? missing = Tsonic.CSharp.Js.Array.atReference(text, 1);
        Assert.Same(text[0], present);
        Assert.Null(missing);
        Assert.DoesNotContain(typeof(JSArray<int>).GetMethods(), method => method.Name is "at" or "atValue" or "atReference");
        Assert.DoesNotContain(typeof(Tsonic.CSharp.Js.Array).GetMethods(), method => method.Name == "at");
    }

    [Fact]
    public void NativeCountsAndCodeUnitsHaveIntegerStorageAndNativeBounds()
    {
        var buffer = new ArrayBuffer(4);
        int length = buffer.byteLength;
        var view = new DataView(buffer, 1, 2);
        int offset = view.byteOffset;
        int viewLength = view.byteLength;
        int code = "\uFFFF".charCodeAt(0);
        var expression = new RegExp("x") { lastIndex = 2 };
        int index = expression.lastIndex;
        Assert.Equal(4, length);
        Assert.Equal(1, offset);
        Assert.Equal(2, viewLength);
        Assert.Equal(65535, code);
        Assert.Equal(2, index);
        Assert.Throws<IndexOutOfRangeException>(() => "".charCodeAt(0));
        Assert.Throws<RangeError>(() => new ArrayBuffer(-1));
        Assert.Throws<RangeError>(() => new Uint8Array(-1));
        Assert.Throws<RangeError>(() => new DataView(buffer, int.MaxValue));
    }

    [Fact]
    public void NumericEnumerableFactoriesPreserveOneExplicitConversion()
    {
        static IEnumerable<double> Values()
        {
            yield return 0;
            yield return 255;
            yield return 256;
        }
        var values = Uint8Array.From(Values());
        Assert.Equal(new byte[] { 0, 255, 0 }, values.ToArray());
    }

    [Fact]
    public void CommonArrayReadsKeepTheExactElementWithoutAFloatTransport()
    {
        const long exact = 9007199254740993;
        IArrayLike<long> source = new JSArray<long>(new[] { exact, 0L });
        long? first = ArrayLike.ReadValue(source, 0);
        long? zero = ArrayLike.ReadValue(source, 1);
        long? absent = ArrayLike.ReadValue(source, 2);
        Assert.Equal(exact, first);
        Assert.Equal(0L, zero);
        Assert.Null(absent);
        Assert.DoesNotContain(typeof(ArrayLike).GetMethods(), method => method.Name == "ReadNumber");
    }
}
