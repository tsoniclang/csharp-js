namespace Tsonic.CSharp.Js;

/// <summary>
/// The Source key category of a union arm: the ECMAScript Type that SameValueZero compares first. Values of different
/// categories are never the same Map/Set key (Q77: 5n and 5 are distinct). A CLR carrier alone does not decide it:
/// a Q1-defaulted number and an explicit int64 (bigint) can both be stored as long, so the compiler names the
/// category of each arm from the Host typeof fact (N3'). A "function" arm is an Object for key identity.
/// </summary>
public enum CollectionKeyCategory : byte
{
    Number,
    BigInt,
    String,
    Boolean,
    Symbol,
    Object,
}

/// <summary>A type-level key category marker; the category is a JIT-time constant of each union key instantiation.</summary>
public interface ICollectionKeyCategory
{
    static abstract CollectionKeyCategory Category { get; }
}

public readonly struct NumberKeyCategory : ICollectionKeyCategory
{
    public static CollectionKeyCategory Category => CollectionKeyCategory.Number;
}

public readonly struct BigIntKeyCategory : ICollectionKeyCategory
{
    public static CollectionKeyCategory Category => CollectionKeyCategory.BigInt;
}

public readonly struct StringKeyCategory : ICollectionKeyCategory
{
    public static CollectionKeyCategory Category => CollectionKeyCategory.String;
}

public readonly struct BooleanKeyCategory : ICollectionKeyCategory
{
    public static CollectionKeyCategory Category => CollectionKeyCategory.Boolean;
}

public readonly struct SymbolKeyCategory : ICollectionKeyCategory
{
    public static CollectionKeyCategory Category => CollectionKeyCategory.Symbol;
}

public readonly struct ObjectKeyCategory : ICollectionKeyCategory
{
    public static CollectionKeyCategory Category => CollectionKeyCategory.Object;
}
