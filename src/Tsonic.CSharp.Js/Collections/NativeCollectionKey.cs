using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Tsonic.CSharp.Js;

public readonly struct NativeCollectionKey<TValue> : ICollectionKey<TValue, NativeCollectionKey<TValue>>
{
    private readonly TValue _value;
    private NativeCollectionKey(TValue value) { _value = value; }
    public static NativeCollectionKey<TValue> Create(TValue value) => new(value);
    public TValue Value => JSKeyEquality.canonicalizeKeyedCollectionKey(_value);
    public bool IsNaN => JSKeyEquality.NativeIsNaN(_value);
    public bool Equals(NativeCollectionKey<TValue> other) => JSKeyEquality.NativeSameValueZero(_value, other._value);
    public override bool Equals(object? other) => other is NativeCollectionKey<TValue> key && Equals(key);
    public override int GetHashCode() => JSKeyEquality.NativeKeyHash(_value);
}

public readonly struct SourceCollectionKey<TValue, TCategory> : ICollectionKey<TValue, SourceCollectionKey<TValue, TCategory>>
    where TCategory : struct, ISourceValueCategory<TValue>
{
    private readonly TValue _value;
    private SourceCollectionKey(TValue value) { _value = value; }
    public static SourceCollectionKey<TValue, TCategory> Create(TValue value) => new(value);
    public TValue Value => CollectionKeyComparison.Canonicalize<TValue, TCategory>(_value);
    public bool IsNaN => CollectionKeyComparison.IsNaN<TValue, TCategory>(_value);
    public bool Equals(SourceCollectionKey<TValue, TCategory> other) => JSKeyEquality.HasNativeScalarEquality<TValue>()
        ? TCategory.Category(_value) == TCategory.Category(other._value) && JSKeyEquality.NativeSameValueZero(_value, other._value)
        : CollectionKeyComparison.Equal<TValue, TCategory, TValue, TCategory>(_value, other._value);
    public override bool Equals(object? other) => other is SourceCollectionKey<TValue, TCategory> key && Equals(key);
    public override int GetHashCode() => JSKeyEquality.HasNativeScalarEquality<TValue>()
        ? JSKeyEquality.NativeKeyHash(_value) : CollectionKeyComparison.Hash<TValue, TCategory>(_value);
}

internal static class CollectionKeyComparison
{
    internal static TValue Canonicalize<TValue, TCategory>(TValue value) where TCategory : struct, ISourceValueCategory<TValue>
    {
        var category = TCategory.Category(value);
        AuthenticateStored(value, category);
        return category is SourceValueCategory.Number or SourceValueCategory.BigInt
            ? JSKeyEquality.canonicalizeKeyedCollectionKey(value) : value;
    }

    internal static bool IsNaN<TValue, TCategory>(TValue value) where TCategory : struct, ISourceValueCategory<TValue>
    {
        var category = TCategory.Category(value);
        AuthenticateStored(value, category);
        return category is SourceValueCategory.Number or SourceValueCategory.BigInt && TryNumeric(value, out var number) && number.IsNaN;
    }

    internal static bool Equal<TLeft, TLeftCategory, TRight, TRightCategory>(TLeft left, TRight right)
        where TLeftCategory : struct, ISourceValueCategory<TLeft>
        where TRightCategory : struct, ISourceValueCategory<TRight>
    {
        var category = TLeftCategory.Category(left);
        var rightCategory = TRightCategory.Category(right);
        AuthenticateStored(left, category);
        AuthenticateStored(right, rightCategory);
        if (category != rightCategory) return false;
        if (category == SourceValueCategory.Absent) return true;
        if (TryStored(left, out var leftStored)) return EqualPayload(leftStored.unwrap(), right, category);
        if (TryStored(right, out var rightStored)) return EqualPayload(rightStored.unwrap(), left, category);
        if (category is not (SourceValueCategory.Object or SourceValueCategory.Function or SourceValueCategory.Symbol))
        {
            if (!JSKeyEquality.HasNativeScalarEquality<TLeft>()) return EqualPayload(TsValue.UnwrapClosedValue(left), right, category);
            if (!JSKeyEquality.HasNativeScalarEquality<TRight>()) return EqualPayload(TsValue.UnwrapClosedValue(right), left, category);
        }
        if (typeof(TLeft) == typeof(TRight))
            return EqualSameCategory(left, Unsafe.As<TRight, TLeft>(ref right), category);
        if (!typeof(TLeft).IsValueType && left is TRight typedLeft) return EqualSameCategory(typedLeft, right, category);
        if (!typeof(TRight).IsValueType && right is TLeft typedRight) return EqualSameCategory(left, typedRight, category);
        if (category is SourceValueCategory.Number or SourceValueCategory.BigInt &&
            TryNumeric(left, out var leftNumber) && TryNumeric(right, out var rightNumber))
            return leftNumber.Equals(rightNumber);
        return !typeof(TLeft).IsValueType && !typeof(TRight).IsValueType &&
            (category is SourceValueCategory.Object or SourceValueCategory.Function or SourceValueCategory.Symbol
                ? ReferenceEquals(left, right)
                : JSKeyEquality.NativeSameValueZero<object?>(left, right));
    }

    internal static int Hash<TValue, TCategory>(TValue value)
        where TCategory : struct, ISourceValueCategory<TValue>
    {
        var category = TCategory.Category(value);
        AuthenticateStored(value, category);
        if (category == SourceValueCategory.Absent) return 0;
        return HashCode.Combine(category, TryStored(value, out var stored) ? PayloadHash(stored.unwrap(), category) : PayloadHash(value, category));
    }

    private static int PayloadHash<TValue>(TValue value, SourceValueCategory category)
    {
        var hash = category is SourceValueCategory.Number or SourceValueCategory.BigInt &&
            TryNumeric(value, out var number) ? number.GetHashCode()
            : category is SourceValueCategory.Object or SourceValueCategory.Function or SourceValueCategory.Symbol
                ? typeof(TValue).IsValueType || value is not null && value.GetType().IsValueType
                    ? EqualityComparer<TValue>.Default.GetHashCode(value!) : RuntimeHelpers.GetHashCode(value!)
                : JSKeyEquality.NativeKeyHash(value);
        return hash;
    }

    private static bool TryNumeric<TValue>(TValue value, out NativeNumericKey number) => TryStored(value, out var stored)
        ? NativeNumericKey.TryCreateBoxed(TsValue.UnwrapClosedValue(stored), out number)
        : !typeof(TValue).IsValueType
            ? NativeNumericKey.TryCreateBoxed(TsValue.UnwrapClosedValue(value), out number)
            : NativeNumericKey.TryCreate(value, out number);

    private static bool EqualPayload<TValue>(object? payload, TValue value, SourceValueCategory category)
    {
        var scalar = category is not (SourceValueCategory.Object or SourceValueCategory.Function or SourceValueCategory.Symbol);
        if (scalar) payload = TsValue.UnwrapClosedValue(payload);
        if (TryStored(value, out var stored)) return EqualSameCategory(payload, stored.unwrap(), category);
        if (payload is TValue typed) return EqualSameCategory(typed, value, category);
        if (category is SourceValueCategory.Number or SourceValueCategory.BigInt &&
            TryNumeric(payload, out var leftNumber) && TryNumeric(value, out var rightNumber))
            return leftNumber.Equals(rightNumber);
        return scalar && !JSKeyEquality.HasNativeScalarEquality<TValue>() &&
            EqualSameCategory(payload, TsValue.UnwrapClosedValue(value), category);
    }

    private static bool EqualSameCategory<TValue>(TValue left, TValue right, SourceValueCategory category) =>
        category is SourceValueCategory.Object or SourceValueCategory.Function or SourceValueCategory.Symbol
            ? typeof(TValue).IsValueType ? EqualityComparer<TValue>.Default.Equals(left, right)
                : ReferenceEquals(left, right) || left is not null && left.GetType().IsValueType && left.Equals(right)
            : JSKeyEquality.NativeSameValueZero(left, right);

    private static bool TryStored<TValue>(TValue value, out TsValue stored)
    {
        if (typeof(TValue) == typeof(TsValue)) { stored = Unsafe.As<TValue, TsValue>(ref value); return true; }
        if (!typeof(TValue).IsValueType && value is TsValue boxed) { stored = boxed; return true; }
        stored = default;
        return false;
    }

    private static void AuthenticateStored<TValue>(TValue value, SourceValueCategory category)
    {
        if (TryStored(value, out var stored) && stored.Category != category)
            throw new ArgumentException("A Source collection key must retain the stored value's authenticated category.", nameof(value));
    }
}
