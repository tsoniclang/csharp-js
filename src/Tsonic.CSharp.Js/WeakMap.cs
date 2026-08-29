/**
 * JavaScript WeakMap implementation
 * Wraps native .NET ConditionalWeakTable<K,V> with JavaScript WeakMap semantics
 */

using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace Tsonic.CSharp.Js
{
    /// <summary>
    /// JavaScript WeakMap - key-value collection with weakly-referenced keys
    /// Keys must be reference types and can be garbage collected
    /// </summary>
    public class WeakMap<K, V> where K : class
    {
        private readonly ConditionalWeakTable<K, StrongBox<V>> _table = new();

        // ==================== Constructors ====================

        /// <summary>
        /// Create empty WeakMap
        /// </summary>
        public WeakMap() { }

        public WeakMap(IEnumerable<(K, V)>? entries)
        {
            if (entries is null)
                return;
            foreach (var (key, value) in entries)
                set(key, value);
        }

        // ==================== Core Methods ====================

        /// <summary>
        /// Get value for key, or default if not found
        /// </summary>
        public V? get(K key)
        {
            if (_table.TryGetValue(key, out var box))
            {
                return box.Value;
            }
            return default;
        }

        internal bool TryGetValue(K key, out V value)
        {
            if (_table.TryGetValue(key, out var box))
            {
                value = box.Value!;
                return true;
            }
            value = default!;
            return false;
        }

        /// <summary>
        /// Set value for key, returns the WeakMap for chaining
        /// </summary>
        public WeakMap<K, V> set(K key, V value)
        {
            // Remove existing if present, then add new
            _table.Remove(key);
            _table.Add(key, new StrongBox<V>(value));
            return this;
        }

        /// <summary>
        /// Check if key exists in WeakMap
        /// </summary>
        public bool has(K key)
        {
            return _table.TryGetValue(key, out _);
        }

        /// <summary>
        /// Delete key from WeakMap, returns true if key existed
        /// </summary>
        public bool delete(K key)
        {
            return _table.Remove(key);
        }

        // Note: WeakMap is intentionally not iterable (matches JavaScript)
        // No keys(), values(), entries(), forEach(), size, or clear()
    }

    public static class WeakMap
    {
        public static V? getValue<K, V>(WeakMap<K, V> map, K key)
            where K : class
            where V : struct
        {
            ArgumentNullException.ThrowIfNull(map);
            return map.TryGetValue(key, out var value) ? value : null;
        }

        public static V? getReference<K, V>(WeakMap<K, V> map, K key)
            where K : class
            where V : class
        {
            ArgumentNullException.ThrowIfNull(map);
            return map.TryGetValue(key, out var value) ? value : null;
        }
    }
}
