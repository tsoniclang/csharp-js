using System;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace Tsonic.CSharp.Js.Tests;

public static class TypedArrayStringChecks
{
    public static void CheckValues()
    {
        Check<sbyte>([sbyte.MinValue, sbyte.MaxValue], "-128,127");
        Check<byte>([0, byte.MaxValue], "0,255");
        Check<short>([short.MinValue, short.MaxValue], "-32768,32767");
        Check<ushort>([0, ushort.MaxValue], "0,65535");
        Check<int>([int.MinValue, int.MaxValue], "-2147483648,2147483647");
        Check<uint>([0, uint.MaxValue], "0,4294967295");
        Check<long>([long.MinValue, 9007199254740993L, long.MaxValue], "-9223372036854775808,9007199254740993,9223372036854775807");
        Check<ulong>([0, ulong.MaxValue], "0,18446744073709551615");
        Check<nint>([nint.MinValue, nint.MaxValue], nint.MinValue.ToString(CultureInfo.InvariantCulture) + "," + nint.MaxValue.ToString(CultureInfo.InvariantCulture));
        Check<nuint>([0, nuint.MaxValue], "0," + nuint.MaxValue.ToString(CultureInfo.InvariantCulture));
        Check<Int128>([Int128.MinValue, Int128.MaxValue], "-170141183460469231731687303715884105728,170141183460469231731687303715884105727");
        Check<UInt128>([0, UInt128.MaxValue], "0,340282366920938463463374607431768211455");
        Check<Half>([(Half)1.25, Half.NaN, Half.PositiveInfinity], "1.25,NaN,Infinity");
        Check<float>([1.25f, -0f, float.NegativeInfinity], "1.25,0,-Infinity");
        Check<double>([1.25, -0d, 1e20, 1e21, 1e-6, 1e-7, double.NaN], "1.25,0,100000000000000000000,1e+21,0.000001,1e-7,NaN");
        Check<decimal>([1.25m, decimal.MaxValue], "1.25,79228162514264337593543950335");
        Check<BigInteger>([BigInteger.Pow(10, 500)], "1" + new string('0', 500));
        Check<bool>([true, false], "true,false");
        Check<long?>([1, null, 9007199254740993L], "1,,9007199254740993");
        Check<string?>(["native", null, "😀"], "native,,😀");
        Check<Tsonic.CSharp.Runtime.TsValue>([Tsonic.CSharp.Runtime.TsValue.from(9007199254740993L), Tsonic.CSharp.Runtime.TsValue.undefined()], "9007199254740993,");
    }

    public static void CheckCosts()
    {
        CheckNumericCost((byte)125);
        CheckNumericCost((short)-125);
        CheckNumericCost(125);
        CheckNumericCost(9007199254740993L);
        CheckNumericCost(ulong.MaxValue);
        CheckNumericCost(UInt128.MaxValue);
        CheckNumericCost((Half)1.25);
        CheckNumericCost(1.25f);
        CheckNumericCost(1.25d);
        CheckNumericCost(1.25m);
        CheckNullableNumericCost(9007199254740993L);
        CheckNullableNumericCost(1.25d);
        CheckBoxedCost();
    }

    private static void Check<TValue>(TValue[] values, string expected)
    {
        var array = JSArray<TValue>.of(values);
        var actual = Globals.String(array);
        if (actual != expected) throw new InvalidOperationException($"{typeof(TValue)} array String mismatch: {actual}");
    }

    private static void CheckNumericCost<TValue>(TValue value) where TValue : INumberBase<TValue>
    {
        var array = new JSArray<TValue>();
        for (var index = 0; index < 256; index++) array.Add(value);
        Func<string> native = () => NativeWrite(array);
        Func<string> generated = () => Globals.String(array);
        if (native() != generated()) throw new InvalidOperationException("Native formatting mismatch");
        var expected = Measure(native);
        var actual = Measure(generated);
        if (actual != expected) throw new InvalidOperationException($"{typeof(TValue)} native array allocation mismatch: {actual} != {expected}");
    }

    private static string NativeWrite<TValue>(JSArray<TValue> array) where TValue : INumberBase<TValue>
    {
        var output = new StringBuilder();
        Span<char> buffer = stackalloc char[128];
        for (var index = 0; index < array.Count; index++)
        {
            if (index != 0) output.Append(',');
            if (!array[index].TryFormat(buffer, out var length, default, CultureInfo.InvariantCulture))
                throw new InvalidOperationException("Native test formatter exhausted its buffer");
            output.Append(buffer[..length]);
        }
        return output.ToString();
    }

    private static void CheckNullableNumericCost<TValue>(TValue value) where TValue : struct, INumberBase<TValue>
    {
        var array = new JSArray<TValue?>();
        for (var index = 0; index < 256; index++) array.Add(index % 3 == 0 ? null : value);
        string Native()
        {
            var output = new StringBuilder();
            Span<char> buffer = stackalloc char[128];
            for (var index = 0; index < array.Count; index++)
            {
                if (index != 0) output.Append(',');
                if (array[index] is not { } present) continue;
                if (!present.TryFormat(buffer, out var length, default, CultureInfo.InvariantCulture))
                    throw new InvalidOperationException("Native nullable test formatter exhausted its buffer");
                output.Append(buffer[..length]);
            }
            return output.ToString();
        }
        Func<string> generated = () => Globals.String(array);
        if (Native() != generated()) throw new InvalidOperationException("Nullable native formatting mismatch");
        var expected = Measure(Native);
        var actual = Measure(generated);
        if (actual != expected) throw new InvalidOperationException($"{typeof(TValue)} nullable native allocation mismatch: {actual} != {expected}");
    }

    private static long Measure(Func<string> conversion)
    {
        for (var index = 0; index < 100; index++) GC.KeepAlive(conversion());
        var start = GC.GetAllocatedBytesForCurrentThread();
        for (var index = 0; index < 100; index++) GC.KeepAlive(conversion());
        return GC.GetAllocatedBytesForCurrentThread() - start;
    }

    private static void CheckBoxedCost()
    {
        var array = new JSArray<object>();
        for (var index = 0; index < 256; index++) array.Add(9007199254740993L);
        string Native()
        {
            var output = new StringBuilder();
            Span<char> buffer = stackalloc char[128];
            for (var index = 0; index < array.Count; index++)
            {
                if (index != 0) output.Append(',');
                var value = (ISpanFormattable)array[index];
                if (!value.TryFormat(buffer, out var length, default, CultureInfo.InvariantCulture))
                    throw new InvalidOperationException("Native boxed test formatter exhausted its buffer");
                output.Append(buffer[..length]);
            }
            return output.ToString();
        }
        Func<string> generated = () => Globals.String(array);
        if (Native() != generated()) throw new InvalidOperationException("Boxed native formatting mismatch");
        var expected = Measure(Native);
        var actual = Measure(generated);
        if (actual != expected) throw new InvalidOperationException($"Closed native allocation mismatch: {actual} != {expected}");
    }
}
