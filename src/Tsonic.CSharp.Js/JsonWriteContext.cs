using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Tsonic.CSharp.Js
{
    public sealed class JsonWriteContext
    {
        private sealed class ReferenceComparer : IEqualityComparer<object>
        {
            public static readonly ReferenceComparer Instance = new();

            public new bool Equals(object? left, object? right) => ReferenceEquals(left, right);

            public int GetHashCode(object value) => RuntimeHelpers.GetHashCode(value);
        }

        private readonly HashSet<object> _active = new(ReferenceComparer.Instance);

        public bool enter(object value) => _active.Add(value);

        public void exit(object value) => _active.Remove(value);
    }
}
