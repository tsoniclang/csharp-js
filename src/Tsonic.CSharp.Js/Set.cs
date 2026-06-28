/**
 * Closed JavaScript Set carrier.
 * Uses explicit SameValueZero value matching and insertion-order storage instead of native collection equality semantics.
 */

using System;
using System.Collections;
using System.Collections.Generic;

namespace Tsonic.CSharp.Js
{
    /// <summary>
    /// JavaScript Set carrier with insertion order, null/undefined values, NaN equality, and object identity.
    /// </summary>
    public class Set<T> : IEnumerable<T>
    {
        private readonly List<T> _values = new();

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
        public int size => _values.Count;

        // ==================== Core Methods ====================

        /// <summary>
        /// Add value to Set, returns the Set for chaining
        /// </summary>
        public Set<T> add(T value)
        {
            var canonicalValue = JSKeyEquality.canonicalizeKeyedCollectionKey(value);
            if (indexOfValue(canonicalValue) < 0)
            {
                _values.Add(canonicalValue);
            }
            return this;
        }

        /// <summary>
        /// Check if value exists in Set
        /// </summary>
        public bool has(T value)
        {
            return indexOfValue(JSKeyEquality.canonicalizeKeyedCollectionKey(value)) >= 0;
        }

        /// <summary>
        /// Delete value from Set, returns true if value existed
        /// </summary>
        public bool delete(T value)
        {
            var index = indexOfValue(JSKeyEquality.canonicalizeKeyedCollectionKey(value));
            if (index < 0)
            {
                return false;
            }

            _values.RemoveAt(index);
            return true;
        }

        /// <summary>
        /// Remove all values from Set
        /// </summary>
        public void clear()
        {
            _values.Clear();
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
        public void forEach(Action<T, T, Set<T>> callback)
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
        public Set<T> difference(Set<T> other)
        {
            var result = new Set<T>();
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
        public Set<T> intersection(Set<T> other)
        {
            var result = new Set<T>();
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
        public Set<T> union(Set<T> other)
        {
            var result = new Set<T>(_values);
            foreach (var value in other)
            {
                result.add(value);
            }
            return result;
        }

        /// <summary>
        /// Return new Set with values in either Set but not both
        /// </summary>
        public Set<T> symmetricDifference(Set<T> other)
        {
            var result = new Set<T>();
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
        public bool isSubsetOf(Set<T> other)
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
        public bool isSupersetOf(Set<T> other)
        {
            return other.isSubsetOf(this);
        }

        /// <summary>
        /// Check if this Set has no values in common with other
        /// </summary>
        public bool isDisjointFrom(Set<T> other)
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

        private int indexOfValue(T value)
        {
            for (var index = 0; index < _values.Count; index++)
            {
                if (JSKeyEquality.sameValueZero(_values[index], value))
                {
                    return index;
                }
            }

            return -1;
        }
    }
}
