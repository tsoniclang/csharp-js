using System;
using System.Collections.Generic;
using System.Numerics;
using Xunit;

namespace Tsonic.CSharp.Js.Tests
{
    using JsObjectStatics = Tsonic.CSharp.Js.Object;

    public class ObjectTests
    {
        [Fact]
        public void keys_values_entries_ReturnClosedJsArraysForJsObject()
        {
            var value = new JSObject
            {
                ["name"] = "tsonic",
                ["count"] = 3
            };

            var keys = JsObjectStatics.keys(value);
            var values = JsObjectStatics.values(value);
            var entries = JsObjectStatics.entries(value);

            Assert.IsType<JSArray<string>>(keys);
            Assert.IsType<JSArray<object?>>(values);
            Assert.IsType<JSArray<(string key, object? value)>>(entries);
            Assert.Equal(new[] { "name", "count" }, keys);
            Assert.Equal(new object?[] { "tsonic", 3 }, values);
            Assert.Equal(("name", (object?)"tsonic"), entries[0]);
            Assert.Equal(("count", (object?)3), entries[1]);
        }

        [Fact]
        public void is_UsesJavaScriptSameValueSemantics()
        {
            var objectValue = new JSObject();

            Assert.True(JsObjectStatics.@is(double.NaN, double.NaN));
            Assert.True(JsObjectStatics.@is(1, 1d));
            Assert.True(JsObjectStatics.@is("x", "x"));
            Assert.True(JsObjectStatics.@is(null, null));
            Assert.True(JsObjectStatics.@is(TsValue.undefined(), TsValue.undefined().unwrap()));
            Assert.True(JsObjectStatics.@is(objectValue, objectValue));
            Assert.False(JsObjectStatics.@is(0d, -0d));
            Assert.False(JsObjectStatics.@is(null, 0));
            Assert.True(JsObjectStatics.@is(TsValue.undefined(), null));
            Assert.False(JsObjectStatics.@is(new JSObject(), new JSObject()));
        }

        [Fact]
        public void is_PreservesExactNativeNumericDomains()
        {
            (object Left, object Right, bool Equal, string Name)[] cases =
            [
                (9007199254740993L, 9007199254740992L, false, "adjacent long"),
                (9007199254740993UL, 9007199254740992UL, false, "adjacent ulong"),
                (long.MaxValue, long.MaxValue - 1, false, "long maximum"),
                (long.MinValue, long.MinValue + 1, false, "long minimum"),
                (ulong.MaxValue, ulong.MaxValue - 1, false, "ulong maximum"),
                (9007199254740993M, 9007199254740992M, false, "decimal integer precision"),
                (1.0000000000000000000000000001M, 1M, false, "decimal fractional precision"),
                (1M, 1.00M, true, "decimal scale"),
                (1, 1D, true, "exact mixed integer and double"),
                (9007199254740993L, 9007199254740992D, false, "rounded mixed integer and double"),
                (long.MaxValue, 9223372036854775808D, false, "double beyond signed range"),
                (ulong.MaxValue, 18446744073709551616D, false, "double beyond unsigned range"),
                (0.1M, 0.1D, false, "decimal and binary fraction differ"),
                (0.5M, 0.5D, true, "exact decimal and binary fraction"),
                (0.1F, 0.1D, false, "single and double fraction differ"),
                ((Half)0.5, 0.5F, true, "half and single exact fraction"),
                (Int128.MinValue, Int128.MinValue + 1, false, "int128 minimum"),
                (UInt128.MaxValue, UInt128.MaxValue - 1, false, "uint128 maximum"),
                ((nint)(-7), -7L, true, "native signed width"),
                ((nuint)7, 7UL, true, "native unsigned width"),
                (new BigInteger(9007199254740993L), 9007199254740993L, true, "exact bigint and long"),
                (new BigInteger(9007199254740993L), 9007199254740992D, false, "bigint and rounded double"),
                (BigInteger.One << 1000, double.ScaleB(1, 1000), true, "large exact bigint and double"),
                (-(BigInteger.One << 1000), -double.ScaleB(1, 1000), true, "negative large exact bigint and double"),
                ((BigInteger.One << 1000) + 1, double.ScaleB(1, 1000), false, "large adjacent bigint"),
                (new BigInteger(double.MaxValue), double.MaxValue, true, "largest integral finite double"),
                (new BigInteger(double.MaxValue) + 1, double.MaxValue, false, "adjacent largest integral double"),
            ];
            foreach (var entry in cases)
            {
                Assert.True(JsObjectStatics.@is(entry.Left, entry.Right) == entry.Equal, entry.Name);
                Assert.True(JsObjectStatics.@is(entry.Right, entry.Left) == entry.Equal, entry.Name + " reverse");
            }
            Assert.False(JsObjectStatics.@is(9007199254740993L, 9007199254740992L));
            Assert.False(JsObjectStatics.@is(9007199254740993L, 9007199254740992D));
            Assert.False(JsObjectStatics.@is(0.1M, 0.1D));
        }

        [Fact]
        public void is_RetainsFloatingSameValueAndNativeIdentity()
        {
            Assert.True(JsObjectStatics.@is(float.NaN, double.NaN));
            Assert.True(JsObjectStatics.@is(Half.NaN, float.NaN));
            Assert.True(JsObjectStatics.@is(double.PositiveInfinity, float.PositiveInfinity));
            Assert.False(JsObjectStatics.@is(double.PositiveInfinity, float.NegativeInfinity));
            Assert.False(JsObjectStatics.@is(double.PositiveInfinity, ulong.MaxValue));
            Assert.False(JsObjectStatics.@is(0F, -0D));
            Assert.True(JsObjectStatics.@is(-0F, -0D));
            Assert.False(JsObjectStatics.@is(0M, -0D));
            Assert.True(JsObjectStatics.@is(0M, 0D));
            Assert.False(JsObjectStatics.@is(double.Epsilon, 0M));
            Assert.False(JsObjectStatics.@is(true, 1));
            Assert.False(JsObjectStatics.@is('1', 1));
            Assert.True(JsObjectStatics.@is(new string(['x']), new string(['x'])));
            var value = new EqualReference();
            Assert.True(JsObjectStatics.@is(value, value));
            Assert.False(JsObjectStatics.@is(value, new EqualReference()));
            Assert.True(JsObjectStatics.@is(TsValue.undefined(), null));
        }

        [Fact]
        public void is_KnownNativeComparisonsDoNotAllocate()
        {
            long[] left = [9007199254740993L, long.MaxValue, 7L, -7L];
            long[] right = [9007199254740992L, long.MaxValue, 8L, -7L];
            var count = 0;
            for (var index = 0; index < 40_000; index++)
            {
                if (JsObjectStatics.@is(left[index & 3], right[index & 3])) count++;
            }
            var before = GC.GetAllocatedBytesForCurrentThread();
            for (var index = 0; index < 40_000; index++)
            {
                if (JsObjectStatics.@is(left[index & 3], right[index & 3])) count++;
            }
            var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
            Assert.Equal(40_000, count);
            Assert.Equal(0, allocated);
        }

        private sealed class EqualReference
        {
            public override bool Equals(object? other) => other is EqualReference;
            public override int GetHashCode() => 0;
        }

        [Fact]
        public void is_AllSelectedNativeScalarDomainsRemainUnboxed()
        {
            Assert.Equal(0, ComparisonAllocations<sbyte, sbyte>([1, -2], [1, -3]));
            Assert.Equal(0, ComparisonAllocations<byte, byte>([1, 2], [1, 3]));
            Assert.Equal(0, ComparisonAllocations<short, short>([1, -2], [1, -3]));
            Assert.Equal(0, ComparisonAllocations<ushort, ushort>([1, 2], [1, 3]));
            Assert.Equal(0, ComparisonAllocations<int, int>([1, -2], [1, -3]));
            Assert.Equal(0, ComparisonAllocations<uint, uint>([1, 2], [1, 3]));
            Assert.Equal(0, ComparisonAllocations<long, long>([9007199254740993L, -2], [9007199254740993L, -3]));
            Assert.Equal(0, ComparisonAllocations<ulong, ulong>([ulong.MaxValue, 2], [ulong.MaxValue, 3]));
            Assert.Equal(0, ComparisonAllocations<nint, nint>([1, -2], [1, -3]));
            Assert.Equal(0, ComparisonAllocations<nuint, nuint>([1, 2], [1, 3]));
            Assert.Equal(0, ComparisonAllocations<Int128, Int128>([Int128.MinValue, 2], [Int128.MinValue, 3]));
            Assert.Equal(0, ComparisonAllocations<UInt128, UInt128>([UInt128.MaxValue, 2], [UInt128.MaxValue, 3]));
            Assert.Equal(0, ComparisonAllocations<Half, Half>([(Half)0.5, Half.NaN], [(Half)0.5, Half.NaN]));
            Assert.Equal(0, ComparisonAllocations<float, float>([0.5F, -0F], [0.5F, 0F]));
            Assert.Equal(0, ComparisonAllocations<double, double>([double.NaN, -0D], [double.NaN, 0D]));
            Assert.Equal(0, ComparisonAllocations<decimal, decimal>([0.1M, 2M], [0.1M, 3M]));
            Assert.Equal(0, ComparisonAllocations<BigInteger, BigInteger>([BigInteger.One << 1000, 2], [BigInteger.One << 1000, 3]));
            Assert.Equal(0, ComparisonAllocations<bool, bool>([true, false], [true, true]));
            Assert.Equal(0, ComparisonAllocations<char, char>(['x', 'y'], ['x', 'z']));
            Assert.Equal(0, ComparisonAllocations<string, string>(["one", "two"], ["one", "three"]));
            Assert.Equal(0, ComparisonAllocations<long, double>([9007199254740993L, 1], [9007199254740992D, 1D]));
            Assert.Equal(0, ComparisonAllocations<decimal, double>([0.1M, 0.5M], [0.1D, 0.5D]));
            Assert.Equal(0, ComparisonAllocations<BigInteger, long>([new BigInteger(9007199254740993L), -7], [9007199254740993L, -7]));
        }

        private static long ComparisonAllocations<TValue, TOther>(TValue[] left, TOther[] right)
        {
            var count = 0;
            for (var index = 0; index < 40_000; index++)
                if (JsObjectStatics.@is(left[index & 1], right[index & 1])) count++;
            var before = GC.GetAllocatedBytesForCurrentThread();
            for (var index = 0; index < 40_000; index++)
                if (JsObjectStatics.@is(left[index & 1], right[index & 1])) count++;
            var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
            GC.KeepAlive(count);
            return allocated;
        }

        [Fact]
        public void keys_values_entries_UseEnumerableOwnArrayIndexesOnly()
        {
            var value = new JSArray<int>(5);
            value[1] = 20;
            value[3] = 40;

            Assert.Equal(new[] { "0", "1", "2", "3", "4" }, JsObjectStatics.keys(value));
            Assert.Equal(new object?[] { 0, 20, 0, 40, 0 }, JsObjectStatics.values(value));

            var entries = JsObjectStatics.entries(value);

            Assert.Equal(5, entries.Count);
            Assert.Equal(("1", (object?)20), entries[1]);
            Assert.Equal(("3", (object?)40), entries[3]);
        }

        [Fact]
        public void keys_values_entries_ReturnTypedArraysForReadOnlyDictionary()
        {
            IReadOnlyDictionary<string, int> value = new Dictionary<string, int>
            {
                ["first"] = 10,
                ["second"] = 20
            };

            var keys = JsObjectStatics.keys(value);
            var values = JsObjectStatics.values(value);
            var entries = JsObjectStatics.entries(value);

            Assert.IsType<JSArray<string>>(keys);
            Assert.IsType<JSArray<int>>(values);
            Assert.IsType<JSArray<(string key, int value)>>(entries);
            Assert.Equal(new[] { "first", "second" }, keys);
            Assert.Equal(new[] { 10, 20 }, values);
            Assert.Equal(("first", 10), entries[0]);
            Assert.Equal(("second", 20), entries[1]);
        }

        [Fact]
        public void keys_values_entries_ReturnTypedArraysForMutableDictionaryInterface()
        {
            IDictionary<string, string> value = new Dictionary<string, string>
            {
                ["name"] = "tsonic",
                ["target"] = "csharp"
            };

            var keys = JsObjectStatics.keys(value);
            var values = JsObjectStatics.values(value);
            var entries = JsObjectStatics.entries(value);

            Assert.IsType<JSArray<string>>(keys);
            Assert.IsType<JSArray<string>>(values);
            Assert.IsType<JSArray<(string key, string value)>>(entries);
            Assert.Equal(new[] { "name", "target" }, keys);
            Assert.Equal(new[] { "tsonic", "csharp" }, values);
            Assert.Equal(("name", "tsonic"), entries[0]);
            Assert.Equal(("target", "csharp"), entries[1]);
        }

        [Fact]
        public void keys_values_entries_ReturnTypedArraysForConcreteDictionaryWithoutAmbiguousOverload()
        {
            var value = new Dictionary<string, bool>
            {
                ["ok"] = true,
                ["done"] = false
            };

            var keys = JsObjectStatics.keys(value);
            var values = JsObjectStatics.values(value);
            var entries = JsObjectStatics.entries(value);

            Assert.IsType<JSArray<string>>(keys);
            Assert.IsType<JSArray<bool>>(values);
            Assert.IsType<JSArray<(string key, bool value)>>(entries);
            Assert.Equal(new[] { "ok", "done" }, keys);
            Assert.Equal(new[] { true, false }, values);
            Assert.Equal(("ok", true), entries[0]);
            Assert.Equal(("done", false), entries[1]);
        }

        [Fact]
        public void hasOwn_SupportsClosedTypedDictionaryWithoutReflection()
        {
            var value = new Dictionary<string, int>
            {
                ["answer"] = 42
            };

            Assert.True(JsObjectStatics.hasOwn(value, "answer"));
            Assert.False(JsObjectStatics.hasOwn(value, "missing"));
        }

        [Fact]
        public void keys_values_entries_SupportStringBoxingWithoutReflection()
        {
            Assert.Equal(new[] { "0", "1", "2" }, JsObjectStatics.keys("abc"));
            Assert.Equal(new object?[] { "a", "b", "c" }, JsObjectStatics.values("abc"));

            var entries = JsObjectStatics.entries("abc");

            Assert.Equal(("0", (object?)"a"), entries[0]);
            Assert.Equal(("1", (object?)"b"), entries[1]);
            Assert.Equal(("2", (object?)"c"), entries[2]);
        }

        [Fact]
        public void keys_values_entries_ReturnEmptyForScalarBoxing()
        {
            Assert.Empty(JsObjectStatics.keys(42));
            Assert.Empty(JsObjectStatics.values(true));
            Assert.Empty(JsObjectStatics.entries(3.14));
        }

        [Fact]
        public void keys_RejectsNullReceiver()
        {
            Assert.Throws<TypeError>(() => JsObjectStatics.keys(null));
        }

        [Fact]
        public void keys_RejectsUndefinedReceiver()
        {
            Assert.Throws<TypeError>(() => JsObjectStatics.keys(TsValue.undefined().unwrap()));
            Assert.Throws<TypeError>(() => JsObjectStatics.keys(TsValue.undefined()));
        }

        [Fact]
        public void keys_RejectsUnsupportedClrObjectWithoutReflection()
        {
            Assert.Throws<NotSupportedException>(() => JsObjectStatics.keys(new { name = "not-a-js-carrier" }));
        }

        [Fact]
        public void keys_values_entries_UseClosedTsObjectCarrier()
        {
            var value = TsValue.from(new TsObject(new Dictionary<string, object?>
            {
                ["name"] = "tsonic",
                ["missing"] = TsValue.undefined().unwrap(),
                ["nil"] = null
            }));

            Assert.Equal(new[] { "name", "missing", "nil" }, JsObjectStatics.keys(value));
            Assert.Equal(new object?[] { "tsonic", TsValue.undefined().unwrap(), null }, JsObjectStatics.values(value));

            var entries = JsObjectStatics.entries(value);

            Assert.Equal(("name", (object?)"tsonic"), entries[0]);
            Assert.Equal(("missing", (object?)TsValue.undefined().unwrap()), entries[1]);
            Assert.Equal(("nil", (object?)null), entries[2]);
        }

        [Fact]
        public void keys_values_entries_UseClosedTsArrayPresentSlotsOnly()
        {
            var array = new TsArray();
            array.WriteDynamicElement(2, "third");
            array.WriteDynamicElement(1, TsValue.undefined().unwrap());

            Assert.Equal(new[] { "1", "2" }, JsObjectStatics.keys(TsValue.from(array)));
            Assert.Equal(new object?[] { TsValue.undefined().unwrap(), "third" }, JsObjectStatics.values(TsValue.from(array)));

            var entries = JsObjectStatics.entries(TsValue.from(array));

            Assert.Equal(("1", (object?)TsValue.undefined().unwrap()), entries[0]);
            Assert.Equal(("2", (object?)"third"), entries[1]);
        }

        [Fact]
        public void assign_MutatesTargetFromClosedSourcesAndReturnsTarget()
        {
            var target = new JSObject
            {
                ["name"] = "old"
            };
            var source = new JSObject
            {
                ["name"] = "new",
                ["active"] = true
            };
            var sparse = new JSArray<int>(4);
            sparse[2] = 9;

            var result = JsObjectStatics.assign(target, null, TsValue.undefined().unwrap(), TsValue.undefined(), source, sparse, "xy");

            Assert.Same(target, result);
            Assert.Equal("new", target["name"]);
            Assert.Equal(true, target["active"]);
            Assert.Equal("x", target["0"]);
            Assert.Equal("y", target["1"]);
            Assert.Equal(9, target["2"]);
            Assert.True(target.hasOwnProperty("3"));
            Assert.Equal(0, target["3"]);
        }

        [Fact]
        public void assign_CopiesIntoDictionaryTarget()
        {
            var target = new Dictionary<string, object?>();
            var source = new JSObject
            {
                ["answer"] = 42
            };

            var result = JsObjectStatics.assign(target, source);

            Assert.Same(target, result);
            Assert.Equal(42, target["answer"]);
        }

        [Fact]
        public void assign_CopiesFromClosedTsObjectAndSkipsUndefinedSources()
        {
            var target = new JSObject();
            var source = TsValue.from(new TsObject(new Dictionary<string, object?>
            {
                ["name"] = "closed",
                ["nil"] = null,
                ["missing"] = TsValue.undefined().unwrap()
            }));

            var result = JsObjectStatics.assign(target, TsValue.undefined(), source);

            Assert.Same(target, result);
            Assert.Equal("closed", target["name"]);
            Assert.Null(target["nil"]);
            Assert.Same(TsValue.undefined().unwrap(), target["missing"]);
        }

        [Fact]
        public void assign_CopiesFromClosedTsArrayPresentSlotsOnly()
        {
            var target = new JSObject();
            var source = new TsArray();
            source.WriteDynamicElement(2, "third");
            source.WriteDynamicElement(1, TsValue.undefined().unwrap());

            var result = JsObjectStatics.assign(target, source);

            Assert.Same(target, result);
            Assert.False(target.hasOwnProperty("0"));
            Assert.Same(TsValue.undefined().unwrap(), target["1"]);
            Assert.Equal("third", target["2"]);
        }

        [Fact]
        public void assign_CopiesTypedDictionarySourcesWithoutReflection()
        {
            var target = new Dictionary<string, int>
            {
                ["answer"] = 1
            };
            var first = new Dictionary<string, int>
            {
                ["answer"] = 42
            };
            var second = new Dictionary<string, int>
            {
                ["extra"] = 7
            };

            var result = JsObjectStatics.assign(target, first, second);

            Assert.Same(target, result);
            Assert.Equal(42, target["answer"]);
            Assert.Equal(7, target["extra"]);
        }

        [Fact]
        public void assign_RejectsNullTarget()
        {
            Assert.Throws<TypeError>(() => JsObjectStatics.assign((JSObject)null!));
        }

        [Fact]
        public void assign_RejectsUnsupportedSourceWithoutReflection()
        {
            Assert.Throws<NotSupportedException>(() =>
                JsObjectStatics.assign(new JSObject(), new { name = "not-a-js-carrier" })
            );
        }
    }
}
