/**
 * Closed JavaScript Map carrier.
 * Uses explicit SameValueZero key matching and insertion-order storage instead of native collection key semantics.
 */

using System;
using System.Collections;
using System.Collections.Generic;

namespace Tsonic.CSharp.Js
{
    /// <summary>
    /// JavaScript Map carrier with insertion order, overwrite ordering, null/undefined keys, NaN equality, and object identity.
    /// </summary>
    public class Map<K, V> : IEnumerable<(K key, V value)>
    {
        private readonly List<Entry> _entries = new();

        private struct Entry
        {
            public Entry(K key, V value)
            {
                Key = key;
                Value = value;
            }

            public K Key { get; }

            public V Value { get; set; }
        }

        // ==================== Constructors ====================

        /// <summary>
        /// Create empty Map
        /// </summary>
        public Map() { }

        /// <summary>
        /// Create Map from key-value pairs
        /// </summary>
        public Map(IEnumerable<(K key, V value)> entries)
        {
            foreach (var (key, value) in entries)
            {
                set(key, value);
            }
        }

        /// <summary>
        /// Create Map from KeyValuePairs
        /// </summary>
        public Map(IEnumerable<KeyValuePair<K, V>> entries)
        {
            foreach (var kvp in entries)
            {
                set(kvp.Key, kvp.Value);
            }
        }

        // ==================== Properties ====================

        /// <summary>
        /// Number of key-value pairs in the Map
        /// </summary>
        public int size => _entries.Count;

        // ==================== Core Methods ====================

        /// <summary>
        /// Get value for key, or default if not found
        /// </summary>
        public V? get(K key)
        {
            var index = indexOfKey(JSKeyEquality.canonicalizeKeyedCollectionKey(key));
            return index >= 0 ? _entries[index].Value : default;
        }

        /// <summary>
        /// Set value for key, returns the Map for chaining
        /// </summary>
        public Map<K, V> set(K key, V value)
        {
            var canonicalKey = JSKeyEquality.canonicalizeKeyedCollectionKey(key);
            var index = indexOfKey(canonicalKey);
            if (index >= 0)
            {
                _entries[index] = new Entry(_entries[index].Key, value);
            }
            else
            {
                _entries.Add(new Entry(canonicalKey, value));
            }
            return this;
        }

        /// <summary>
        /// Check if key exists in Map
        /// </summary>
        public bool has(K key)
        {
            return indexOfKey(JSKeyEquality.canonicalizeKeyedCollectionKey(key)) >= 0;
        }

        /// <summary>
        /// Delete key from Map, returns true if key existed
        /// </summary>
        public bool delete(K key)
        {
            var index = indexOfKey(JSKeyEquality.canonicalizeKeyedCollectionKey(key));
            if (index < 0)
            {
                return false;
            }

            _entries.RemoveAt(index);
            return true;
        }

        /// <summary>
        /// Remove all key-value pairs from Map
        /// </summary>
        public void clear()
        {
            _entries.Clear();
        }

        // ==================== Iteration Methods ====================

        /// <summary>
        /// Get all keys in insertion order
        /// </summary>
        public IEnumerable<K> keys()
        {
            foreach (var entry in _entries)
            {
                yield return entry.Key;
            }
        }

        /// <summary>
        /// Get all values in insertion order
        /// </summary>
        public IEnumerable<V> values()
        {
            foreach (var entry in _entries)
            {
                yield return entry.Value;
            }
        }

        /// <summary>
        /// Get all key-value pairs as tuples in insertion order
        /// </summary>
        public IEnumerable<(K key, V value)> entries()
        {
            foreach (var entry in _entries)
            {
                yield return (entry.Key, entry.Value);
            }
        }

        /// <summary>
        /// Execute callback for each key-value pair
        /// </summary>
        public void forEach(Action<V, K, Map<K, V>> callback)
        {
            foreach (var entry in _entries)
            {
                callback(entry.Value, entry.Key, this);
            }
        }

        /// <summary>
        /// Execute callback for each key-value pair (value and key only)
        /// </summary>
        public void forEach(Action<V, K> callback)
        {
            foreach (var entry in _entries)
            {
                callback(entry.Value, entry.Key);
            }
        }

        /// <summary>
        /// Execute callback for each value
        /// </summary>
        public void forEach(Action<V> callback)
        {
            foreach (var entry in _entries)
            {
                callback(entry.Value);
            }
        }

        // ==================== IEnumerable Implementation ====================

        /// <summary>
        /// Get enumerator for iterating key-value pairs
        /// </summary>
        public IEnumerator<(K key, V value)> GetEnumerator()
        {
            foreach (var entry in _entries)
            {
                yield return (entry.Key, entry.Value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private int indexOfKey(K key)
        {
            for (var index = 0; index < _entries.Count; index++)
            {
                if (JSKeyEquality.sameValueZero(_entries[index].Key, key))
                {
                    return index;
                }
            }

            return -1;
        }
    }
}
