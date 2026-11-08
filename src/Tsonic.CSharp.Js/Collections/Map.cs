/**
 * Closed JavaScript Map carrier.
 * Uses native numeric key equality and insertion-order storage.
 */

using System;
using System.Collections;
using System.Collections.Generic;

namespace Tsonic.CSharp.Js
{
    /// <summary>
    /// JavaScript Map carrier with insertion order, overwrite ordering, null/undefined keys, NaN equality, and object identity.
    /// </summary>
    public class Map<K, V, TKey> : IEnumerable<(K key, V value)> where TKey : struct, ICollectionKey<K, TKey>
    {
        private OrderedKeyCollection<TKey, V> _entries = new();

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
        /// Get value for key, or JavaScript undefined if not found
        /// </summary>
        public object? get(K key)
        {
            return _entries.TryGet(TKey.Create(key), out var value) ? value : null;
        }

        public bool tryGet(K key, out V value)
        {
            return _entries.TryGet(TKey.Create(key), out value!);
        }

        /// <summary>
        /// Set value for key, returns the Map for chaining
        /// </summary>
        public Map<K, V, TKey> set(K key, V value)
        {
            _entries.Set(TKey.Create(key), value);
            return this;
        }

        /// <summary>
        /// Check if key exists in Map
        /// </summary>
        public bool has(K key)
        {
            return _entries.Contains(TKey.Create(key));
        }

        /// <summary>
        /// Delete key from Map, returns true if key existed
        /// </summary>
        public bool delete(K key)
        {
            return _entries.Delete(TKey.Create(key));
        }

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
            foreach (var entry in _entries.Enumerate())
            {
                yield return entry.Key.Value;
            }
        }

        /// <summary>
        /// Get all values in insertion order
        /// </summary>
        public IEnumerable<V> values()
        {
            foreach (var entry in _entries.Enumerate())
            {
                yield return entry.Value;
            }
        }

        /// <summary>
        /// Get all key-value pairs as tuples in insertion order
        /// </summary>
        public IEnumerable<(K key, V value)> entries()
        {
            foreach (var entry in _entries.Enumerate())
            {
                yield return (entry.Key.Value, entry.Value);
            }
        }

        /// <summary>
        /// Execute callback for each key-value pair
        /// </summary>
        public void forEach(Action<V, K, Map<K, V, TKey>> callback)
        {
            foreach (var entry in _entries.Enumerate())
            {
                callback(entry.Value, entry.Key.Value, this);
            }
        }

        /// <summary>
        /// Execute callback for each key-value pair (value and key only)
        /// </summary>
        public void forEach(Action<V, K> callback)
        {
            foreach (var entry in _entries.Enumerate())
            {
                callback(entry.Value, entry.Key.Value);
            }
        }

        /// <summary>
        /// Execute callback for each value
        /// </summary>
        public void forEach(Action<V> callback)
        {
            foreach (var entry in _entries.Enumerate())
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
            foreach (var entry in _entries.Enumerate())
            {
                yield return (entry.Key.Value, entry.Value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }

    public static class Map
    {
        public static TValue? getValue<TKey, TValue, TKeyWitness>(Map<TKey, TValue, TKeyWitness> map, TKey key) where TValue : struct where TKeyWitness : struct, ICollectionKey<TKey, TKeyWitness>
        {
            return map.tryGet(key, out var value) ? value : null;
        }

        public static TValue getOptional<TKey, TValue, TKeyWitness>(Map<TKey, TValue, TKeyWitness> map, TKey key) where TKeyWitness : struct, ICollectionKey<TKey, TKeyWitness>
        {
            return map.tryGet(key, out var value) ? value : default!;
        }
    }
}
