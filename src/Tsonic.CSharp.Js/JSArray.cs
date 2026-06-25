/**
 * JavaScript Array implementation with full JS semantics
 * Use this when you need resizable arrays with push/pop/splice etc.
 * For fixed-size arrays, use native T[] instead.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Tsonic.CSharp.Js
{
    /// <summary>
    /// JavaScript-style resizable array with full JS semantics.
    /// Backed by slots so empty array elements remain distinct from present default values.
    /// </summary>
    public interface IJSArray
    {
        int length { get; }

        bool hasIndex(int index);

        bool tryGetAtObject(int index, out object? value);
    }

    public class JSArray<T> : IEnumerable<T>, IJSArray
    {
        private readonly List<Slot> _slots;

        private readonly struct Slot
        {
            private readonly T _value;

            private Slot(bool isPresent, T value)
            {
                IsPresent = isPresent;
                _value = value;
            }

            public bool IsPresent { get; }

            public T Value => IsPresent ? _value : default(T)!;

            public static Slot Hole => default;

            public static Slot Present(T value)
            {
                return new Slot(true, value);
            }
        }

        // ==================== Constructors ====================

        /// <summary>
        /// Create empty JSArray
        /// </summary>
        public JSArray()
        {
            _slots = new List<Slot>();
        }

        /// <summary>
        /// Create JSArray with JavaScript Array(length) semantics.
        /// </summary>
        public JSArray(int length)
        {
            if (length < 0)
            {
                throw new ArgumentException("Invalid array length", nameof(length));
            }

            _slots = new List<Slot>(length);
            AddHoles(length);
        }

        public JSArray(double length)
            : this(ToArrayLength(length))
        {
        }

        internal static JSArray<T> createWithCapacity(int capacity)
        {
            return new JSArray<T>(capacity, false);
        }

        private JSArray(int capacity, bool _)
        {
            _slots = new List<Slot>(capacity);
        }

        /// <summary>
        /// Create JSArray from native array
        /// </summary>
        public JSArray(T[] source)
        {
            _slots = new List<Slot>(source.Length);
            AddPresentRange(source);
        }

        /// <summary>
        /// Create JSArray from List
        /// </summary>
        public JSArray(List<T> source)
        {
            _slots = new List<Slot>(source.Count);
            AddPresentRange(source);
        }

        /// <summary>
        /// Create JSArray from any enumerable
        /// </summary>
        public JSArray(IEnumerable<T> source)
        {
            _slots = new List<Slot>();
            AddPresentRange(source);
        }

        // ==================== Properties ====================

        /// <summary>
        /// Get array length
        /// </summary>
        public int length => _slots.Count;

        // ==================== Indexer ====================

        /// <summary>
        /// Get or set element at index
        /// </summary>
        public T this[int index]
        {
            get
            {
                return ReadValue(index);
            }
            set
            {
                if (index < 0)
                {
                    throw new ArgumentException("Array index cannot be negative", nameof(index));
                }

                EnsureLengthForIndex(index);

                _slots[index] = Slot.Present(value);
            }
        }

        /// <summary>
        /// Check whether an array index is present.
        /// </summary>
        public bool hasIndex(int index)
        {
            return IsPresent(index);
        }

        /// <summary>
        /// Read a present array index without conflating holes with present default values.
        /// </summary>
        public bool tryGetAt(int index, out T value)
        {
            if (IsPresent(index))
            {
                value = _slots[index].Value;
                return true;
            }

            value = default(T)!;
            return false;
        }

        public bool tryGetAtObject(int index, out object? value)
        {
            if (IsPresent(index))
            {
                value = _slots[index].Value;
                return true;
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Delete an array index, leaving a hole and preserving length.
        /// </summary>
        public bool deleteAt(int index)
        {
            if (index >= 0 && index < _slots.Count)
            {
                _slots[index] = Slot.Hole;
            }

            return true;
        }

        // ==================== Length Manipulation ====================

        /// <summary>
        /// Set array length (truncate or extend with holes)
        /// </summary>
        public void setLength(int newLength)
        {
            if (newLength < 0)
            {
                throw new ArgumentException("Invalid array length", nameof(newLength));
            }

            if (newLength < _slots.Count)
            {
                _slots.RemoveRange(newLength, _slots.Count - newLength);
            }
            else if (newLength > _slots.Count)
            {
                AddHoles(newLength - _slots.Count);
            }
        }

        // ==================== Basic Mutation Methods ====================

        /// <summary>
        /// Add element to end of array and return new length
        /// </summary>
        public int push(T item)
        {
            _slots.Add(Slot.Present(item));
            return _slots.Count;
        }

        /// <summary>
        /// Add multiple elements to end of array and return new length
        /// </summary>
        public int push(params T[] items)
        {
            AddPresentRange(items);
            return _slots.Count;
        }

        /// <summary>
        /// Remove and return last element
        /// </summary>
        public T pop()
        {
            if (_slots.Count == 0)
            {
                return default(T)!;
            }

            T item = _slots[_slots.Count - 1].Value;
            _slots.RemoveAt(_slots.Count - 1);
            return item;
        }

        /// <summary>
        /// Remove and return first element
        /// </summary>
        public T shift()
        {
            if (_slots.Count == 0)
            {
                return default(T)!;
            }

            T item = _slots[0].Value;
            _slots.RemoveAt(0);
            return item;
        }

        /// <summary>
        /// Add element to beginning of array and return new length
        /// </summary>
        public int unshift(T item)
        {
            _slots.Insert(0, Slot.Present(item));
            return _slots.Count;
        }

        /// <summary>
        /// Add multiple elements to beginning of array and return new length
        /// </summary>
        public int unshift(params T[] items)
        {
            for (int i = items.Length - 1; i >= 0; i--)
            {
                _slots.Insert(0, Slot.Present(items[i]));
            }

            return _slots.Count;
        }

        // ==================== Slicing Methods ====================

        /// <summary>
        /// Return shallow copy of portion of array
        /// </summary>
        public JSArray<T> slice(int start = 0, int? end = null)
        {
            int actualStart = start < 0 ? System.Math.Max(0, _slots.Count + start) : start;
            int actualEnd = end.HasValue
                ? (end.Value < 0 ? System.Math.Max(0, _slots.Count + end.Value) : end.Value)
                : _slots.Count;

            actualStart = System.Math.Min(actualStart, _slots.Count);
            actualEnd = System.Math.Min(actualEnd, _slots.Count);

            if (actualStart >= actualEnd)
            {
                return new JSArray<T>();
            }

            var result = JSArray<T>.createWithCapacity(actualEnd - actualStart);
            for (int i = actualStart; i < actualEnd; i++)
            {
                result._slots.Add(_slots[i]);
            }

            return result;
        }

        /// <summary>
        /// Add/remove elements at position
        /// </summary>
        public JSArray<T> splice(int start, int? deleteCount = null, params T[] items)
        {
            int actualStart = start < 0 ? System.Math.Max(0, _slots.Count + start) : System.Math.Min(start, _slots.Count);
            int actualDeleteCount = deleteCount ?? (_slots.Count - actualStart);
            actualDeleteCount = System.Math.Max(0, System.Math.Min(actualDeleteCount, _slots.Count - actualStart));

            var deleted = JSArray<T>.createWithCapacity(actualDeleteCount);
            for (int i = 0; i < actualDeleteCount; i++)
            {
                deleted._slots.Add(_slots[actualStart]);
                _slots.RemoveAt(actualStart);
            }

            for (int i = 0; i < items.Length; i++)
            {
                _slots.Insert(actualStart + i, Slot.Present(items[i]));
            }

            return deleted;
        }

        // ==================== Higher-Order Functions ====================

        /// <summary>
        /// Map array elements to new array (value only)
        /// </summary>
        public JSArray<TResult> map<TResult>(Func<T, TResult> callback)
        {
            var result = JSArray<TResult>.createWithCapacity(_slots.Count);
            result.setLength(_slots.Count);
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].IsPresent)
                {
                    result[i] = callback(_slots[i].Value);
                }
            }
            return result;
        }

        /// <summary>
        /// Map array elements to new array (value, index)
        /// </summary>
        public JSArray<TResult> map<TResult>(Func<T, int, TResult> callback)
        {
            var result = JSArray<TResult>.createWithCapacity(_slots.Count);
            result.setLength(_slots.Count);
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].IsPresent)
                {
                    result[i] = callback(_slots[i].Value, i);
                }
            }
            return result;
        }

        /// <summary>
        /// Map array elements to new array (value, index, array)
        /// </summary>
        public JSArray<TResult> map<TResult>(Func<T, int, JSArray<T>, TResult> callback)
        {
            var result = JSArray<TResult>.createWithCapacity(_slots.Count);
            result.setLength(_slots.Count);
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].IsPresent)
                {
                    result[i] = callback(_slots[i].Value, i, this);
                }
            }
            return result;
        }

        /// <summary>
        /// Filter array elements (value only)
        /// </summary>
        public JSArray<T> filter(Func<T, bool> callback)
        {
            var result = new JSArray<T>();
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].IsPresent && callback(_slots[i].Value))
                {
                    result.push(_slots[i].Value);
                }
            }
            return result;
        }

        /// <summary>
        /// Filter array elements (value, index)
        /// </summary>
        public JSArray<T> filter(Func<T, int, bool> callback)
        {
            var result = new JSArray<T>();
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].IsPresent && callback(_slots[i].Value, i))
                {
                    result.push(_slots[i].Value);
                }
            }
            return result;
        }

        /// <summary>
        /// Filter array elements (value, index, array)
        /// </summary>
        public JSArray<T> filter(Func<T, int, JSArray<T>, bool> callback)
        {
            var result = new JSArray<T>();
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].IsPresent && callback(_slots[i].Value, i, this))
                {
                    result.push(_slots[i].Value);
                }
            }
            return result;
        }

        /// <summary>
        /// Reduce array to single value (accumulator, value)
        /// </summary>
        public TResult reduce<TResult>(Func<TResult, T, TResult> callback, TResult initialValue)
        {
            TResult accumulator = initialValue;
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].IsPresent)
                {
                    accumulator = callback(accumulator, _slots[i].Value);
                }
            }
            return accumulator;
        }

        /// <summary>
        /// Reduce array to single value (accumulator, value, index)
        /// </summary>
        public TResult reduce<TResult>(Func<TResult, T, int, TResult> callback, TResult initialValue)
        {
            TResult accumulator = initialValue;
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].IsPresent)
                {
                    accumulator = callback(accumulator, _slots[i].Value, i);
                }
            }
            return accumulator;
        }

        /// <summary>
        /// Reduce array to single value (accumulator, value, index, array)
        /// </summary>
        public TResult reduce<TResult>(Func<TResult, T, int, JSArray<T>, TResult> callback, TResult initialValue)
        {
            TResult accumulator = initialValue;
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].IsPresent)
                {
                    accumulator = callback(accumulator, _slots[i].Value, i, this);
                }
            }
            return accumulator;
        }

        /// <summary>
        /// Reduce array to single value (no initial value)
        /// </summary>
        public T reduce(Func<T, T, T> callback)
        {
            bool hasAccumulator = false;
            T accumulator = default(T)!;
            for (int i = 0; i < _slots.Count; i++)
            {
                if (!_slots[i].IsPresent)
                {
                    continue;
                }

                if (!hasAccumulator)
                {
                    accumulator = _slots[i].Value;
                    hasAccumulator = true;
                }
                else
                {
                    accumulator = callback(accumulator, _slots[i].Value);
                }
            }

            if (!hasAccumulator)
            {
                throw new InvalidOperationException("Reduce of empty array with no initial value");
            }

            return accumulator;
        }

        /// <summary>
        /// Reduce array from right to left
        /// </summary>
        public TResult reduceRight<TResult>(Func<TResult, T, TResult> callback, TResult initialValue)
        {
            TResult accumulator = initialValue;
            for (int i = _slots.Count - 1; i >= 0; i--)
            {
                if (_slots[i].IsPresent)
                {
                    accumulator = callback(accumulator, _slots[i].Value);
                }
            }
            return accumulator;
        }

        /// <summary>
        /// Reduce array from right to left (accumulator, value, index)
        /// </summary>
        public TResult reduceRight<TResult>(Func<TResult, T, int, TResult> callback, TResult initialValue)
        {
            TResult accumulator = initialValue;
            for (int i = _slots.Count - 1; i >= 0; i--)
            {
                if (_slots[i].IsPresent)
                {
                    accumulator = callback(accumulator, _slots[i].Value, i);
                }
            }
            return accumulator;
        }

        /// <summary>
        /// Reduce array from right to left (accumulator, value, index, array)
        /// </summary>
        public TResult reduceRight<TResult>(Func<TResult, T, int, JSArray<T>, TResult> callback, TResult initialValue)
        {
            TResult accumulator = initialValue;
            for (int i = _slots.Count - 1; i >= 0; i--)
            {
                if (_slots[i].IsPresent)
                {
                    accumulator = callback(accumulator, _slots[i].Value, i, this);
                }
            }
            return accumulator;
        }

        /// <summary>
        /// Execute callback for each element (value only)
        /// </summary>
        public void forEach(Action<T> callback)
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].IsPresent)
                {
                    callback(_slots[i].Value);
                }
            }
        }

        /// <summary>
        /// Execute callback for each element (value, index)
        /// </summary>
        public void forEach(Action<T, int> callback)
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].IsPresent)
                {
                    callback(_slots[i].Value, i);
                }
            }
        }

        /// <summary>
        /// Execute callback for each element (value, index, array)
        /// </summary>
        public void forEach(Action<T, int, JSArray<T>> callback)
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].IsPresent)
                {
                    callback(_slots[i].Value, i, this);
                }
            }
        }

        // ==================== Search Methods ====================

        /// <summary>
        /// Find first element matching predicate
        /// </summary>
        public T find(Func<T, bool> callback)
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].IsPresent && callback(_slots[i].Value))
                {
                    return _slots[i].Value;
                }
            }
            return default(T)!;
        }

        /// <summary>
        /// Find first element matching predicate (value, index)
        /// </summary>
        public T find(Func<T, int, bool> callback)
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].IsPresent && callback(_slots[i].Value, i))
                {
                    return _slots[i].Value;
                }
            }
            return default(T)!;
        }

        /// <summary>
        /// Find first element matching predicate (value, index, array)
        /// </summary>
        public T find(Func<T, int, JSArray<T>, bool> callback)
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].IsPresent && callback(_slots[i].Value, i, this))
                {
                    return _slots[i].Value;
                }
            }
            return default(T)!;
        }

        /// <summary>
        /// Find index of first element matching predicate
        /// </summary>
        public int findIndex(Func<T, bool> callback)
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].IsPresent && callback(_slots[i].Value))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Find index of first element matching predicate (value, index)
        /// </summary>
        public int findIndex(Func<T, int, bool> callback)
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].IsPresent && callback(_slots[i].Value, i))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Find index of first element matching predicate (value, index, array)
        /// </summary>
        public int findIndex(Func<T, int, JSArray<T>, bool> callback)
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].IsPresent && callback(_slots[i].Value, i, this))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Find last element matching predicate
        /// </summary>
        public T findLast(Func<T, bool> callback)
        {
            for (int i = _slots.Count - 1; i >= 0; i--)
            {
                if (_slots[i].IsPresent && callback(_slots[i].Value))
                {
                    return _slots[i].Value;
                }
            }
            return default(T)!;
        }

        /// <summary>
        /// Find last element matching predicate (value, index)
        /// </summary>
        public T findLast(Func<T, int, bool> callback)
        {
            for (int i = _slots.Count - 1; i >= 0; i--)
            {
                if (_slots[i].IsPresent && callback(_slots[i].Value, i))
                {
                    return _slots[i].Value;
                }
            }
            return default(T)!;
        }

        /// <summary>
        /// Find last element matching predicate (value, index, array)
        /// </summary>
        public T findLast(Func<T, int, JSArray<T>, bool> callback)
        {
            for (int i = _slots.Count - 1; i >= 0; i--)
            {
                if (_slots[i].IsPresent && callback(_slots[i].Value, i, this))
                {
                    return _slots[i].Value;
                }
            }
            return default(T)!;
        }

        /// <summary>
        /// Find index of last element matching predicate
        /// </summary>
        public int findLastIndex(Func<T, bool> callback)
        {
            for (int i = _slots.Count - 1; i >= 0; i--)
            {
                if (_slots[i].IsPresent && callback(_slots[i].Value))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Find index of last element matching predicate (value, index)
        /// </summary>
        public int findLastIndex(Func<T, int, bool> callback)
        {
            for (int i = _slots.Count - 1; i >= 0; i--)
            {
                if (_slots[i].IsPresent && callback(_slots[i].Value, i))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Find index of last element matching predicate (value, index, array)
        /// </summary>
        public int findLastIndex(Func<T, int, JSArray<T>, bool> callback)
        {
            for (int i = _slots.Count - 1; i >= 0; i--)
            {
                if (_slots[i].IsPresent && callback(_slots[i].Value, i, this))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Find first index of element
        /// </summary>
        public int indexOf(T searchElement, int fromIndex = 0)
        {
            int start = NormalizeForwardSearchStart(fromIndex);
            for (int i = start; i < _slots.Count; i++)
            {
                if (_slots[i].IsPresent && EqualityComparer<T>.Default.Equals(_slots[i].Value, searchElement))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Find last index of element
        /// </summary>
        public int lastIndexOf(T searchElement, int? fromIndex = null)
        {
            int startIndex = fromIndex ?? _slots.Count - 1;
            if (startIndex < 0)
            {
                startIndex = _slots.Count + startIndex;
            }
            startIndex = System.Math.Min(startIndex, _slots.Count - 1);

            for (int i = startIndex; i >= 0; i--)
            {
                if (_slots[i].IsPresent && EqualityComparer<T>.Default.Equals(_slots[i].Value, searchElement))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Check if array includes element
        /// </summary>
        public bool includes(T searchElement, int fromIndex = 0)
        {
            return indexOf(searchElement, fromIndex) >= 0;
        }

        private int NormalizeForwardSearchStart(int fromIndex)
        {
            if (fromIndex >= _slots.Count)
            {
                return _slots.Count;
            }
            return fromIndex < 0
                ? System.Math.Max(_slots.Count + fromIndex, 0)
                : fromIndex;
        }

        /// <summary>
        /// Test if every element matches predicate
        /// </summary>
        public bool every(Func<T, bool> callback)
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].IsPresent && !callback(_slots[i].Value))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Test if every element matches predicate (value, index)
        /// </summary>
        public bool every(Func<T, int, bool> callback)
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].IsPresent && !callback(_slots[i].Value, i))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Test if every element matches predicate (value, index, array)
        /// </summary>
        public bool every(Func<T, int, JSArray<T>, bool> callback)
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].IsPresent && !callback(_slots[i].Value, i, this))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Test if any element matches predicate
        /// </summary>
        public bool some(Func<T, bool> callback)
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].IsPresent && callback(_slots[i].Value))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Test if any element matches predicate (value, index)
        /// </summary>
        public bool some(Func<T, int, bool> callback)
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].IsPresent && callback(_slots[i].Value, i))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Test if any element matches predicate (value, index, array)
        /// </summary>
        public bool some(Func<T, int, JSArray<T>, bool> callback)
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].IsPresent && callback(_slots[i].Value, i, this))
                {
                    return true;
                }
            }
            return false;
        }

        // ==================== Sorting Methods ====================

        /// <summary>
        /// Sort array in place and return the array
        /// </summary>
        public JSArray<T> sort(Func<T, T, double>? compareFunc = null)
        {
            var presentValues = new List<T>();
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].IsPresent)
                {
                    presentValues.Add(_slots[i].Value);
                }
            }

            if (compareFunc != null)
            {
                presentValues.Sort((a, b) =>
                {
                    double result = compareFunc(a, b);
                    return result < 0 ? -1 : result > 0 ? 1 : 0;
                });
            }
            else
            {
                presentValues.Sort((a, b) =>
                {
                    string aStr = a?.ToString() ?? "";
                    string bStr = b?.ToString() ?? "";
                    return string.Compare(aStr, bStr, StringComparison.Ordinal);
                });
            }

            int valueIndex = 0;
            for (; valueIndex < presentValues.Count; valueIndex++)
            {
                _slots[valueIndex] = Slot.Present(presentValues[valueIndex]);
            }

            for (int i = valueIndex; i < _slots.Count; i++)
            {
                _slots[i] = Slot.Hole;
            }

            return this;
        }

        /// <summary>
        /// Reverse array in place and return the array
        /// </summary>
        public JSArray<T> reverse()
        {
            _slots.Reverse();
            return this;
        }

        // ==================== Conversion Methods ====================

        /// <summary>
        /// Join array elements into string
        /// </summary>
        public string join(string separator = ",")
        {
            var parts = new List<string>();
            for (int i = 0; i < _slots.Count; i++)
            {
                parts.Add(_slots[i].IsPresent ? _slots[i].Value?.ToString() ?? "" : "");
            }
            return string.Join(separator, parts);
        }

        /// <summary>
        /// Convert to string
        /// </summary>
        public override string ToString()
        {
            return join(",");
        }

        /// <summary>
        /// Convert to locale string
        /// </summary>
        public string toLocaleString()
        {
            return join(",");
        }

        /// <summary>
        /// Concatenate arrays
        /// </summary>
        public JSArray<T> concat(params object[] items)
        {
            var result = JSArray<T>.createWithCapacity(_slots.Count);
            result._slots.AddRange(_slots);

            foreach (var item in items)
            {
                if (item is JSArray<T> jsArr)
                {
                    result._slots.AddRange(jsArr._slots);
                }
                else if (item is IJSArray genericJsArray)
                {
                    for (int i = 0; i < genericJsArray.length; i++)
                    {
                        result._slots.Add(genericJsArray.tryGetAtObject(i, out var value)
                            ? Slot.Present(CastArrayValue<T>(value))
                            : Slot.Hole);
                    }
                }
                else if (item is T value)
                {
                    result.push(value);
                }
            }

            return result;
        }

        /// <summary>
        /// Convert to native array
        /// </summary>
        public T[] toArray()
        {
            var result = new T[_slots.Count];
            for (int i = 0; i < _slots.Count; i++)
            {
                result[i] = _slots[i].Value;
            }

            return result;
        }

        /// <summary>
        /// Convert to List
        /// </summary>
        public List<T> toList()
        {
            var result = new List<T>(_slots.Count);
            for (int i = 0; i < _slots.Count; i++)
            {
                result.Add(_slots[i].Value);
            }

            return result;
        }

        // ==================== Iterator Methods ====================

        /// <summary>
        /// Get iterator for [index, value] pairs
        /// </summary>
        public IEnumerable<(int index, T value)> entries()
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                yield return (i, _slots[i].Value);
            }
        }

        /// <summary>
        /// Get iterator for keys (indices)
        /// </summary>
        public IEnumerable<int> keys()
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                yield return i;
            }
        }

        /// <summary>
        /// Get iterator for values
        /// </summary>
        public IEnumerable<T> values()
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                yield return _slots[i].Value;
            }
        }

        // ==================== Advanced Methods ====================

        /// <summary>
        /// Get element at index (supports negative indices)
        /// </summary>
        public object? at(int index)
        {
            int actualIndex = index < 0 ? _slots.Count + index : index;
            if (actualIndex < 0 || actualIndex >= _slots.Count)
            {
                return null;
            }
            return _slots[actualIndex].Value;
        }

        public TValue? atValue<TValue>(int index) where TValue : struct
        {
            object? value = at(index);
            return value == null ? null : (TValue)value;
        }

        public TReference? atReference<TReference>(int index) where TReference : class
        {
            return at(index) as TReference;
        }

        /// <summary>
        /// Flatten nested arrays by specified depth
        /// </summary>
        public JSArray<object> flat(int depth = 1)
        {
            var result = new JSArray<object>();
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].IsPresent)
                {
                    FlattenValue(_slots[i].Value, result, depth);
                }
            }

            return result;
        }

        private static void FlattenValue(object? item, JSArray<object> result, int depth)
        {
            if (depth > 0 && item is IJSArray jsArray)
            {
                for (int i = 0; i < jsArray.length; i++)
                {
                    if (jsArray.tryGetAtObject(i, out var nestedItem))
                    {
                        FlattenValue(nestedItem, result, depth - 1);
                    }
                }
            }
            else
            {
                result.push(item!);
            }
        }

        /// <summary>
        /// Map then flatten result
        /// </summary>
        public JSArray<TResult> flatMap<TResult>(Func<T, int, JSArray<T>, object> callback)
        {
            var result = new JSArray<TResult>();
            for (int i = 0; i < _slots.Count; i++)
            {
                if (!_slots[i].IsPresent)
                {
                    continue;
                }

                var mapped = callback(_slots[i].Value, i, this);

                if (mapped is IJSArray jsArr)
                {
                    for (int j = 0; j < jsArr.length; j++)
                    {
                        if (jsArr.tryGetAtObject(j, out var val))
                        {
                            result.push(CastArrayValue<TResult>(val));
                        }
                    }
                }
                else if (mapped is TResult singleValue)
                {
                    result.push(singleValue);
                }
            }
            return result;
        }

        /// <summary>
        /// Fill array with value
        /// </summary>
        public JSArray<T> fill(T value, int start = 0, int? end = null)
        {
            int actualStart = start < 0 ? System.Math.Max(0, _slots.Count + start) : start;
            int actualEnd = end.HasValue
                ? (end.Value < 0 ? _slots.Count + end.Value : end.Value)
                : _slots.Count;

            actualStart = System.Math.Min(System.Math.Max(actualStart, 0), _slots.Count);
            actualEnd = System.Math.Min(System.Math.Max(actualEnd, 0), _slots.Count);

            for (int i = actualStart; i < actualEnd; i++)
            {
                _slots[i] = Slot.Present(value);
            }
            return this;
        }

        /// <summary>
        /// Copy array section to another location
        /// </summary>
        public JSArray<T> copyWithin(int target, int start = 0, int? end = null)
        {
            int actualTarget = target < 0 ? System.Math.Max(0, _slots.Count + target) : target;
            int actualStart = start < 0 ? System.Math.Max(0, _slots.Count + start) : start;
            int actualEnd = end.HasValue
                ? (end.Value < 0 ? _slots.Count + end.Value : end.Value)
                : _slots.Count;

            actualTarget = System.Math.Min(System.Math.Max(actualTarget, 0), _slots.Count);
            actualStart = System.Math.Min(System.Math.Max(actualStart, 0), _slots.Count);
            actualEnd = System.Math.Min(System.Math.Max(actualEnd, 0), _slots.Count);

            int count = System.Math.Min(actualEnd - actualStart, _slots.Count - actualTarget);
            count = System.Math.Max(0, System.Math.Min(count, _slots.Count - actualStart));

            var temp = new List<Slot>();
            for (int i = 0; i < count; i++)
            {
                temp.Add(_slots[actualStart + i]);
            }

            for (int i = 0; i < count; i++)
            {
                _slots[actualTarget + i] = temp[i];
            }

            return this;
        }

        // ==================== Immutable Variants ====================

        /// <summary>
        /// Create new array with element replaced (immutable)
        /// </summary>
        public JSArray<T> with(int index, T value)
        {
            int actualIndex = index < 0 ? _slots.Count + index : index;
            if (actualIndex < 0 || actualIndex >= _slots.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            var result = CopySlots();
            result[actualIndex] = value;
            return result;
        }

        /// <summary>
        /// Create new reversed array (immutable)
        /// </summary>
        public JSArray<T> toReversed()
        {
            var result = CopySlots();
            result.reverse();
            return result;
        }

        /// <summary>
        /// Create new sorted array (immutable)
        /// </summary>
        public JSArray<T> toSorted(Func<T, T, double>? compareFunc = null)
        {
            var result = CopySlots();
            result.sort(compareFunc);
            return result;
        }

        /// <summary>
        /// Create new spliced array (immutable)
        /// </summary>
        public JSArray<T> toSpliced(int start, int? deleteCount = null, params T[] items)
        {
            var result = CopySlots();
            result.splice(start, deleteCount, items);
            return result;
        }

        // ==================== Static Factory Methods ====================

        /// <summary>
        /// Check if value is a JSArray
        /// </summary>
        public static bool isArray(object? value)
        {
            return value is IJSArray;
        }

        /// <summary>
        /// Create array from iterable
        /// </summary>
        public static JSArray<T> from(IEnumerable<T> iterable)
        {
            return new JSArray<T>(iterable);
        }

        /// <summary>
        /// Create array from iterable with map function
        /// </summary>
        public static JSArray<TResult> from<TSource, TResult>(IEnumerable<TSource> iterable, Func<TSource, int, TResult> mapFunc)
        {
            var result = new JSArray<TResult>();
            int index = 0;
            foreach (var item in iterable)
            {
                result.push(mapFunc(item, index++));
            }
            return result;
        }

        /// <summary>
        /// Create array from iterable with single-parameter map function.
        /// </summary>
        public static JSArray<TResult> from<TSource, TResult>(IEnumerable<TSource> iterable, Func<TSource, TResult> mapFunc)
        {
            var result = new JSArray<TResult>();
            foreach (var item in iterable)
            {
                result.push(mapFunc(item));
            }
            return result;
        }

        /// <summary>
        /// Create array from arguments
        /// </summary>
        public static JSArray<T> of(params T[] items)
        {
            return new JSArray<T>(items);
        }

        private void AddPresentRange(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                _slots.Add(Slot.Present(item));
            }
        }

        private static TValue CastArrayValue<TValue>(object? value)
        {
            if (value is TValue typed)
            {
                return typed;
            }

            if (value is null && default(TValue) is null)
            {
                return default(TValue)!;
            }

            throw new InvalidCastException("JSArray element is not assignable to the requested closed carrier element type.");
        }

        private bool IsPresent(int index)
        {
            return index >= 0 && index < _slots.Count && _slots[index].IsPresent;
        }

        private T ReadValue(int index)
        {
            return index >= 0 && index < _slots.Count ? _slots[index].Value : default(T)!;
        }

        private void EnsureLengthForIndex(int index)
        {
            while (_slots.Count <= index)
            {
                _slots.Add(Slot.Hole);
            }
        }

        private void AddHoles(int count)
        {
            for (int i = 0; i < count; i++)
            {
                _slots.Add(Slot.Hole);
            }
        }

        private static int ToArrayLength(double length)
        {
            if (double.IsNaN(length) || double.IsInfinity(length) || length < 0 || length > int.MaxValue || System.Math.Truncate(length) != length)
            {
                throw new ArgumentException("Invalid array length", nameof(length));
            }

            return (int)length;
        }

        private JSArray<T> CopySlots()
        {
            var result = JSArray<T>.createWithCapacity(_slots.Count);
            result._slots.AddRange(_slots);
            return result;
        }

        // ==================== IEnumerable Implementation ====================

        public IEnumerator<T> GetEnumerator()
        {
            return values().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
