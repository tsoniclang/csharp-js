using System;
using Tsonic.CSharp.Runtime;
using Xunit;
using IntegerKinds = Tsonic.CSharp.Runtime.Union<long, long>;
using IntegerKeys = Tsonic.CSharp.Js.UnionCollectionKey<long, Tsonic.CSharp.Runtime.SourceNumberCategory<long>, long, Tsonic.CSharp.Runtime.SourceBigIntCategory<long>>;
using NumberArms = Tsonic.CSharp.Runtime.Union<int, uint>;
using NumberKeys = Tsonic.CSharp.Js.UnionCollectionKey<int, Tsonic.CSharp.Runtime.SourceNumberCategory<int>, uint, Tsonic.CSharp.Runtime.SourceNumberCategory<uint>>;
using FloatingArms = Tsonic.CSharp.Runtime.Union<double, float>;
using FloatingKeys = Tsonic.CSharp.Js.UnionCollectionKey<double, Tsonic.CSharp.Runtime.SourceNumberCategory<double>, float, Tsonic.CSharp.Runtime.SourceNumberCategory<float>>;

namespace Tsonic.CSharp.Js.Tests;

[Collection("Native allocation cost")]
public sealed class SourceArraySearchTests
{
    [Fact]
    public void SourceNumberAndBigIntWithTheSamePhysicalCarrierRemainDistinct()
    {
        var number = IntegerKinds.From1(7);
        var integer = IntegerKinds.From2(7);
        var values = JSArray<IntegerKinds>.of(number, integer, number);
        Assert.True(ArrayLike.includes<IntegerKinds, IntegerKeys>(values, integer));
        Assert.Equal(0, ArrayLike.indexOf<IntegerKinds, IntegerKeys>(values, number));
        Assert.Equal(1, ArrayLike.indexOf<IntegerKinds, IntegerKeys>(values, integer));
        Assert.Equal(2, ArrayLike.lastIndexOf<IntegerKinds, IntegerKeys>(values, number));
        Assert.Equal(1, ArrayLike.lastIndexOf<IntegerKinds, IntegerKeys>(values, integer));
        Assert.False(ArrayLike.includes<IntegerKinds, IntegerKeys>(JSArray<IntegerKinds>.of(number), integer));
        Assert.False(number.Equals(integer));
    }

    [Fact]
    public void SameCategoryArmsShareSearchEqualityWithoutChangingNativeUnionEquality()
    {
        var first = NumberArms.From1(7);
        var second = NumberArms.From2(7);
        var values = JSArray<NumberArms>.of(first, NumberArms.From1(9), second);
        Assert.True(ArrayLike.includes<NumberArms, NumberKeys>(values, second));
        Assert.Equal(0, ArrayLike.indexOf<NumberArms, NumberKeys>(values, second));
        Assert.Equal(2, ArrayLike.lastIndexOf<NumberArms, NumberKeys>(values, first));
        Assert.False(first.Equals(second));
    }

    [Fact]
    public void StrictSearchRejectsNaNWhileIncludesAndBothZerosUseSameValueZero()
    {
        var values = JSArray<FloatingArms>.of(FloatingArms.From1(double.NaN), FloatingArms.From2(-0f));
        Assert.True(ArrayLike.includes<FloatingArms, FloatingKeys>(values, FloatingArms.From2(float.NaN)));
        Assert.Equal(-1, ArrayLike.indexOf<FloatingArms, FloatingKeys>(values, FloatingArms.From2(float.NaN)));
        Assert.Equal(-1, ArrayLike.lastIndexOf<FloatingArms, FloatingKeys>(values, FloatingArms.From2(float.NaN)));
        Assert.Equal(1, ArrayLike.indexOf<FloatingArms, FloatingKeys>(values, FloatingArms.From1(0d)));
        Assert.Equal(1, ArrayLike.lastIndexOf<FloatingArms, FloatingKeys>(values, FloatingArms.From1(-0d)));
        Assert.Equal(BitConverter.SingleToInt32Bits(-0f), BitConverter.SingleToInt32Bits(values[1].As2()));
    }

    [Fact]
    public void GenericOptionalSearchUsesTheCanonicalStorageOperationsAndKey()
    {
        CheckOptionalSearch<int, int?, ValueOptionalStorage<int>, SourceCollectionKey<int, SourceNumberCategory<int>>>(0, 1);
        CheckOptionalSearch<string, string?, ReferenceOptionalStorage<string>, SourceCollectionKey<string, SourceStringCategory<string>>>("", "present");
        CheckOptionalSearch<TsValue, TsValue, JsValueOptionalStorage, SourceCollectionKey<TsValue, SourceStoredValueCategory>>(
            TsValue.fromsource<long, SourceNumberCategory<long>>(7), TsValue.fromsource<long, SourceBigIntCategory<long>>(7));
        var floating = JSArray<double?>.of(null, double.NaN);
        Assert.True(ArrayLike.includes<double?, OptionalCollectionKey<double, double?, ValueOptionalStorage<double>, SourceCollectionKey<double, SourceNumberCategory<double>>>>(floating, double.NaN));
        Assert.Equal(-1, ArrayLike.indexOf<double?, OptionalCollectionKey<double, double?, ValueOptionalStorage<double>, SourceCollectionKey<double, SourceNumberCategory<double>>>>(floating, double.NaN));
    }

    [Fact]
    public void SearchBoundsRetainNegativeOffsetsAndDoNotOverflowOrReadEmptyStorage()
    {
        var values = JSArray<int>.of(7, 9, 7);
        Assert.Equal(2, ArrayLike.indexOf<int, SourceCollectionKey<int, SourceNumberCategory<int>>>(values, 7, -1));
        Assert.Equal(0, ArrayLike.indexOf<int, SourceCollectionKey<int, SourceNumberCategory<int>>>(values, 7, int.MinValue));
        Assert.Equal(-1, ArrayLike.indexOf<int, SourceCollectionKey<int, SourceNumberCategory<int>>>(values, 7, int.MaxValue));
        Assert.False(ArrayLike.includes<int, SourceCollectionKey<int, SourceNumberCategory<int>>>(values, 7, int.MaxValue));
        Assert.Equal(0, ArrayLike.lastIndexOf<int, SourceCollectionKey<int, SourceNumberCategory<int>>>(values, 7, -2));
        Assert.Equal(-1, ArrayLike.lastIndexOf<int, SourceCollectionKey<int, SourceNumberCategory<int>>>(values, 7, int.MinValue));
        Assert.Equal(2, ArrayLike.lastIndexOf<int, SourceCollectionKey<int, SourceNumberCategory<int>>>(values, 7, int.MaxValue));
        Assert.False(ArrayLike.includes<int, SourceCollectionKey<int, SourceNumberCategory<int>>>(JSArray<int>.of(), 0));
        Assert.Equal(-1, ArrayLike.indexOf<int, SourceCollectionKey<int, SourceNumberCategory<int>>>(JSArray<int>.of(), 0));
        Assert.Equal(-1, ArrayLike.lastIndexOf<int, SourceCollectionKey<int, SourceNumberCategory<int>>>(JSArray<int>.of(), 0));
    }

    [Fact]
    public void ExactSourceNumberSearchDoesNotRoundWideIntegerOperandsThroughDouble()
    {
        var values = JSArray<Union<long, double>>.of(Union<long, double>.From1(9007199254740993));
        Assert.True(ArrayLike.includes<Union<long, double>, UnionCollectionKey<long, SourceNumberCategory<long>, double, SourceNumberCategory<double>>>(values, Union<long, double>.From1(9007199254740993)));
        Assert.False(ArrayLike.includes<Union<long, double>, UnionCollectionKey<long, SourceNumberCategory<long>, double, SourceNumberCategory<double>>>(values, Union<long, double>.From2(9007199254740992d)));
    }

    [Fact]
    public void SourceReferenceSearchUsesIdentityRatherThanUserValueEquality()
    {
        var first = new EqualLookingValue();
        var second = new EqualLookingValue();
        var values = JSArray<EqualLookingValue>.of(first);
        Assert.True(first.Equals(second));
        Assert.True(ArrayLike.includes<EqualLookingValue, SourceCollectionKey<EqualLookingValue, SourceObjectCategory<EqualLookingValue>>>(values, first));
        Assert.False(ArrayLike.includes<EqualLookingValue, SourceCollectionKey<EqualLookingValue, SourceObjectCategory<EqualLookingValue>>>(values, second));
    }

    [Fact]
    public void StaticSearchDoesNotBoxCopyOrAllocatePerLookup()
    {
        var values = JSArray<NumberArms>.of(NumberArms.From1(7), NumberArms.From1(9), NumberArms.From1(7));
        var search = NumberArms.From2(7);
        for (var index = 0; index < 10000; index++)
        {
            ArrayLike.includes<NumberArms, NumberKeys>(values, search);
            ArrayLike.indexOf<NumberArms, NumberKeys>(values, search);
            ArrayLike.lastIndexOf<NumberArms, NumberKeys>(values, search);
        }
        var before = GC.GetAllocatedBytesForCurrentThread();
        var found = 0;
        for (var index = 0; index < 10000; index++)
        {
            if (ArrayLike.includes<NumberArms, NumberKeys>(values, search)) found++;
            found += ArrayLike.indexOf<NumberArms, NumberKeys>(values, search);
            found += ArrayLike.lastIndexOf<NumberArms, NumberKeys>(values, search);
        }
        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - before);
        Assert.Equal(30000, found);
    }

    private static void CheckOptionalSearch<TPayload, TStorage, TOps, TKey>(TPayload first, TPayload second)
        where TOps : struct, IOptionalStorage<TPayload, TStorage>
        where TKey : struct, ICollectionKey<TPayload, TKey>
    {
        var absent = TOps.From1(null);
        var firstStorage = TOps.From2(first);
        var secondStorage = TOps.From2(second);
        var values = JSArray<TStorage>.of(absent, firstStorage, secondStorage, absent);
        Assert.True(ArrayLike.includes<TStorage, OptionalCollectionKey<TPayload, TStorage, TOps, TKey>>(values, absent));
        Assert.Equal(0, ArrayLike.indexOf<TStorage, OptionalCollectionKey<TPayload, TStorage, TOps, TKey>>(values, absent));
        Assert.Equal(3, ArrayLike.lastIndexOf<TStorage, OptionalCollectionKey<TPayload, TStorage, TOps, TKey>>(values, absent));
        Assert.Equal(1, ArrayLike.indexOf<TStorage, OptionalCollectionKey<TPayload, TStorage, TOps, TKey>>(values, firstStorage));
        Assert.Equal(2, ArrayLike.indexOf<TStorage, OptionalCollectionKey<TPayload, TStorage, TOps, TKey>>(values, secondStorage));
    }

    private sealed class EqualLookingValue
    {
        public override bool Equals(object? other) => other is EqualLookingValue;
        public override int GetHashCode() => 0;
    }
}
