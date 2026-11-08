/**
 * Closed JavaScript Set carrier.
 * Uses native numeric value equality and insertion-order storage.
 */

using System;
using System.Collections;
using System.Collections.Generic;

namespace Tsonic.CSharp.Js
{
    /// <summary>
    /// JavaScript Set carrier with insertion order, null/undefined values, NaN equality, and object identity.
    /// </summary>
    public class Set<T, TKey> : IEnumerable<T> where TKey : struct, ICollectionKey<T, TKey>
    {
        private OrderedKeyCollection<TKey, byte> _members = new();

        private IEnumerable<T> _values
        {
            get
            {
                foreach (var entry in _members.Enumerate()) yield return entry.Key.Value;
            }
        }

        // ==================== Constructors ====================

        /// <summary>
        /// Create empty Set
        /// </summary>
        public Set() { }

        /// <summary>
        /// Create Set from values
        /// </summary>
        public Set(IEnumerable<T> values)
        {
            foreach (var value in values)
            {
                add(value);
            }
        }

        // ==================== Properties ====================

        /// <summary>
        /// Number of unique values in the Set
        /// </summary>
        public int size => _members.Count;

        // ==================== Core Methods ====================

        /// <summary>
        /// Add value to Set, returns the Set for chaining
        /// </summary>
        public Set<T, TKey> add(T value)
        {
            _members.Add(TKey.Create(value), 0);
            return this;
        }

        /// <summary>
        /// Check if value exists in Set
        /// </summary>
        public bool has(T value)
        {
            return _members.Contains(TKey.Create(value));
        }

        /// <summary>
        /// Delete value from Set, returns true if value existed
        /// </summary>
        public bool delete(T value)
        {
            return _members.Delete(TKey.Create(value));
        }

        /// <summary>
        /// Remove all values from Set
        /// </summary>
        public void clear()
        {
            _members.Clear();
        }

        // ==================== Iteration Methods ====================

        /// <summary>
        /// Get all values (same as values() in JS Set)
        /// </summary>
        public IEnumerable<T> keys()
        {
            return _values;
        }

        /// <summary>
        /// Get all values
        /// </summary>
        public IEnumerable<T> values()
        {
            return _values;
        }

        /// <summary>
        /// Get all entries as (value, value) tuples (JS Set behavior)
        /// </summary>
        public IEnumerable<(T, T)> entries()
        {
            foreach (var value in _values)
            {
                yield return (value, value);
            }
        }

        /// <summary>
        /// Execute callback for each value
        /// </summary>
        public void forEach(Action<T, T, Set<T, TKey>> callback)
        {
            foreach (var value in _values)
            {
                callback(value, value, this);
            }
        }

        /// <summary>
        /// Execute callback for each value (value twice, matching JS)
        /// </summary>
        public void forEach(Action<T, T> callback)
        {
            foreach (var value in _values)
            {
                callback(value, value);
            }
        }

        /// <summary>
        /// Execute callback for each value
        /// </summary>
        public void forEach(Action<T> callback)
        {
            foreach (var value in _values)
            {
                callback(value);
            }
        }

        // ==================== Set Operations ====================

        /// <summary>
        /// Return new Set with values in this Set but not in other
        /// </summary>
        public Set<T, TKey> difference(Set<T, TKey> other)
        {
            var result = new Set<T, TKey>();
            foreach (var value in _values)
            {
                if (!other.has(value))
                {
                    result.add(value);
                }
            }
            return result;
        }

        /// <summary>
        /// Return new Set with values in both Sets
        /// </summary>
        public Set<T, TKey> intersection(Set<T, TKey> other)
        {
            var result = new Set<T, TKey>();
            foreach (var value in _values)
            {
                if (other.has(value))
                {
                    result.add(value);
                }
            }
            return result;
        }

        /// <summary>
        /// Return new Set with values in either Set
        /// </summary>
        public Set<T, TKey> union(Set<T, TKey> other)
        {
            var result = new Set<T, TKey>(_values);
            foreach (var value in other)
            {
                result.add(value);
            }
            return result;
        }

        /// <summary>
        /// Return new Set with values in either Set but not both
        /// </summary>
        public Set<T, TKey> symmetricDifference(Set<T, TKey> other)
        {
            var result = new Set<T, TKey>();
            foreach (var value in _values)
            {
                if (!other.has(value))
                {
                    result.add(value);
                }
            }
            foreach (var value in other)
            {
                if (!this.has(value))
                {
                    result.add(value);
                }
            }
            return result;
        }

        /// <summary>
        /// Check if this Set is a subset of other
        /// </summary>
        public bool isSubsetOf(Set<T, TKey> other)
        {
            foreach (var value in _values)
            {
                if (!other.has(value))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Check if this Set is a superset of other
        /// </summary>
        public bool isSupersetOf(Set<T, TKey> other)
        {
            return other.isSubsetOf(this);
        }

        /// <summary>
        /// Check if this Set has no values in common with other
        /// </summary>
        public bool isDisjointFrom(Set<T, TKey> other)
        {
            foreach (var value in _values)
            {
                if (other.has(value))
                {
                    return false;
                }
            }
            return true;
        }

        // ==================== IEnumerable Implementation ====================

        /// <summary>
        /// Get enumerator for iterating values
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }
}
