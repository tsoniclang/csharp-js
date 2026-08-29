using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Threading;
using Tsonic.CSharp.Runtime;

namespace Tsonic.CSharp.Js
{
    public sealed class Symbol : ITsClosedValueCarrier
    {
        private static readonly ConcurrentDictionary<string, Symbol> GlobalSymbols = new(StringComparer.Ordinal);
        private static long _nextIdentity;
        private readonly long _identity;
        private readonly string? _globalKey;

        private Symbol(string? description, string? globalKey)
        {
            this.description = description;
            _globalKey = globalKey;
            _identity = Interlocked.Increment(ref _nextIdentity);
        }

        public string? description { get; }

        public static Symbol create() => new(null, null);

        public static Symbol create(string description) => new(description, null);

        public static Symbol create(double description) =>
            new(description.ToString("G17", CultureInfo.InvariantCulture), null);

        public static Symbol @for(string key)
        {
            ArgumentNullException.ThrowIfNull(key);
            return GlobalSymbols.GetOrAdd(key, static value => new Symbol(value, value));
        }

        public static string? keyFor(Symbol symbol)
        {
            ArgumentNullException.ThrowIfNull(symbol);
            return symbol._globalKey;
        }

        public override bool Equals(object? other) => ReferenceEquals(this, other);

        public override int GetHashCode() => _identity.GetHashCode();

        public override string ToString() => $"Symbol({description ?? string.Empty})";
    }
}
