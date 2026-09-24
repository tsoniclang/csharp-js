/**
 * JavaScript Array implementation with native dense storage
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
    /// JavaScript-style resizable array with native dense storage.
    /// Backed by contiguous initialized native elements.
    /// </summary>
    public partial class JSArray<T> : List<T>, IReadOnlyList<T>, IEnumerable<T>, IDynamicArray, IArrayLike<T>
    {
        private List<T> _values => this;

        // ==================== Constructors ====================

        /// <summary>
        /// Create empty JSArray
        /// </summary>
        public JSArray()
        {
        }

        /// <summary>
        /// Create a dense array of native default values.
        /// </summary>
        public JSArray(int length)
        {
            if (length < 0)
            {
                throw new RangeError("Invalid array length");
            }

            Capacity = length;
            AddDefaults(length);
        }

        public JSArray(double length)
            : this(ToArrayLength(length))
        {
        }

        internal static JSArray<T> createWithCapacity(int capacity)
        {
            return new JSArray<T>(capacity, false);
        }

        private JSArray(int capacity, bool _) : base(capacity)
        {
        }

        /// <summary>
        /// Create JSArray from native array
        /// </summary>
        public JSArray(T[] source) : base(source)
        {
        }

        /// <summary>
        /// Create JSArray from List
        /// </summary>
        public JSArray(List<T> source) : base(source)
        {
        }

        /// <summary>
        /// Create JSArray from any enumerable
        /// </summary>
        public JSArray(IEnumerable<T> source) : base(source)
        {
        }

        // ==================== Properties ====================

        /// <summary>
        /// Get array length
        /// </summary>
        public int length => _values.Count;

        // ==================== Indexer ====================

        /// <summary>
        /// Get or set element at index
        /// </summary>
        public new T this[int index]
        {
            get
            {
                return index < 0 ? this[(double)index] : ReadValue(index);
            }
            set
            {
                if (index < 0)
                {
                    this[(double)index] = value;
                    return;
                }

                EnsureLengthForIndex(index);

                _values[index] = value;
            }
        }

        /// <summary>
        /// Check whether an array index is present.
        /// </summary>
        public bool hasIndex(int index)
        {
            return index < 0 ? hasIndex((double)index) : IsPresent(index);
        }

        /// <summary>
        /// Read an initialized array index, distinguishing it from an out-of-bounds index.
        /// </summary>
        public bool tryGetAt(int index, out T value)
        {
            if (IsPresent(index))
            {
                value = _values[index];
                return true;
            }

            value = default(T)!;
            return false;
        }

        public bool tryGetAtObject(int index, out object? value)
        {
            if (IsPresent(index))
            {
                value = _values[index];
                return true;
            }

            value = null;
            return false;
        }

        int IArrayLike<T>.Length => length;

        bool IArrayLike<T>.TryGet(double index, out T value) => tryGetAt(index, out value);

        public bool trySetAtObject(int index, object? value)
        {
            if (index < 0)
            {
                throw new ArgumentException("Array index cannot be negative", nameof(index));
            }

            if (value is TsValue tsValue)
            {
                return trySetAtObject(index, tsValue.unwrap());
            }

            if (value is T typed)
            {
                this[index] = typed;
                return true;
            }

            if (value is null && default(T) is null)
            {
                this[index] = default!;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Reject deletion of an initialized element because dense arrays cannot contain holes.
        /// </summary>
        public bool deleteAt(int index)
        {
            if (index < 0) return deleteAt((double)index);
            if (index >= 0 && index < _values.Count)
            {
                throw new TypeError("Deleting a dense array element would create a hole; use splice");
            }

            return true;
        }

        int IDynamicArray.Length => length;

        bool IDynamicArray.HasIndex(int index) => hasIndex(index);

        bool IDynamicArray.TryGetAt(int index, out object? value) =>
            tryGetAtObject(index, out value);

        bool IDynamicArray.TrySetAt(int index, object? value) =>
            trySetAtObject(index, value);

        int IDynamicArray.SetLength(int newLength) => setLength(newLength);

        bool IDynamicArray.DeleteAt(int index) => deleteAt(index);

        // ==================== Length Manipulation ====================

        /// <summary>
        /// Truncate the initialized array.
        /// </summary>
        public int setLength(int newLength)
        {
            if (newLength < 0)
            {
                throw new ArgumentException("Invalid array length", nameof(newLength));
            }

            if (newLength < _values.Count)
            {
                _values.RemoveRange(newLength, _values.Count - newLength);
            }
            else if (newLength > _values.Count)
            {
                throw new RangeError("Dense array growth requires initialized values; use push or fill at construction");
            }

            return newLength;
        }

        // ==================== Basic Mutation Methods ====================

        /// <summary>
        /// Add element to end of array and return new length
        /// </summary>
        public int push(T item)
        {
            _values.Add(item);
            return _values.Count;
        }

        /// <summary>
        /// Add multiple elements to end of array and return new length
        /// </summary>
        public int push(params T[] items)
        {
            AddPresentRange(items);
            return _values.Count;
        }

        /// <summary>
        /// Remove and return last element
        /// </summary>
        public T pop()
        {
            if (_values.Count == 0)
            {
                return default(T)!;
            }

            T item = _values[_values.Count - 1];
            _values.RemoveAt(_values.Count - 1);
            return item;
        }

        /// <summary>
        /// Remove and return first element
        /// </summary>
        public T shift()
        {
            if (_values.Count == 0)
            {
                return default(T)!;
            }

            T item = _values[0];
            _values.RemoveAt(0);
            return item;
        }

        /// <summary>
        /// Add element to beginning of array and return new length
        /// </summary>
        public int unshift(T item)
        {
            _values.Insert(0, item);
            return _values.Count;
        }

        /// <summary>
        /// Add multiple elements to beginning of array and return new length
        /// </summary>
        public int unshift(params T[] items)
        {
            for (int i = items.Length - 1; i >= 0; i--)
            {
                _values.Insert(0, items[i]);
            }

            return _values.Count;
        }

        // ==================== Slicing Methods ====================

        /// <summary>
        /// Return shallow copy of portion of array
        /// </summary>
        public JSArray<T> slice(int start = 0, int? end = null)
        {
            int actualStart = start < 0 ? System.Math.Max(0, _values.Count + start) : start;
            int actualEnd = end.HasValue
                ? (end.Value < 0 ? System.Math.Max(0, _values.Count + end.Value) : end.Value)
                : _values.Count;

            actualStart = System.Math.Min(actualStart, _values.Count);
            actualEnd = System.Math.Min(actualEnd, _values.Count);

            if (actualStart >= actualEnd)
            {
                return new JSArray<T>();
            }

            var result = JSArray<T>.createWithCapacity(actualEnd - actualStart);
            for (int i = actualStart; i < actualEnd; i++)
            {
                result._values.Add(_values[i]);
            }

            return result;
        }

        /// <summary>
        /// Add/remove elements at position
        /// </summary>
        public JSArray<T> splice(int start, int? deleteCount = null, params T[] items)
        {
            int actualStart = start < 0 ? System.Math.Max(0, _values.Count + start) : System.Math.Min(start, _values.Count);
            int actualDeleteCount = deleteCount ?? (_values.Count - actualStart);
            actualDeleteCount = System.Math.Max(0, System.Math.Min(actualDeleteCount, _values.Count - actualStart));

            var deleted = JSArray<T>.createWithCapacity(actualDeleteCount);
            deleted._values.AddRange(_values.GetRange(actualStart, actualDeleteCount));
            _values.RemoveRange(actualStart, actualDeleteCount);
            _values.InsertRange(actualStart, items);

            return deleted;
        }

        // ==================== Higher-Order Functions ====================

        /// <summary>
        /// Map array elements to new array (value only)
        /// </summary>
        public JSArray<TResult> map<TResult>(Func<T, TResult> callback)
        {
            var result = JSArray<TResult>.createWithCapacity(_values.Count);
            for (int i = 0, length = _values.Count; i < length; i++)
            {
                if (!IsPresent(i)) throw new TypeError("Array.map cannot create holes after its source is shortened");
                if (IsPresent(i))
                {
                    result.push(callback(_values[i]));
                }
            }
            return result;
        }

        /// <summary>
        /// Map array elements to new array (value, index)
        /// </summary>
        public JSArray<TResult> map<TResult>(Func<T, int, TResult> callback)
        {
            var result = JSArray<TResult>.createWithCapacity(_values.Count);
            for (int i = 0, length = _values.Count; i < length; i++)
            {
                if (!IsPresent(i)) throw new TypeError("Array.map cannot create holes after its source is shortened");
                if (IsPresent(i))
                {
                    result.push(callback(_values[i], i));
                }
            }
            return result;
        }

        /// <summary>
        /// Map array elements to new array (value, index, array)
        /// </summary>
        public JSArray<TResult> map<TResult>(Func<T, int, JSArray<T>, TResult> callback)
        {
            var result = JSArray<TResult>.createWithCapacity(_values.Count);
            for (int i = 0, length = _values.Count; i < length; i++)
            {
                if (!IsPresent(i)) throw new TypeError("Array.map cannot create holes after its source is shortened");
                if (IsPresent(i))
                {
                    result.push(callback(_values[i], i, this));
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
            for (int i = 0; i < _values.Count; i++)
            {
                if (IsPresent(i) && callback(_values[i]))
                {
                    result.push(_values[i]);
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
            for (int i = 0; i < _values.Count; i++)
            {
                if (IsPresent(i) && callback(_values[i], i))
                {
                    result.push(_values[i]);
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
            for (int i = 0; i < _values.Count; i++)
            {
                if (IsPresent(i) && callback(_values[i], i, this))
                {
                    result.push(_values[i]);
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
            for (int i = 0; i < _values.Count; i++)
            {
                if (IsPresent(i))
                {
                    accumulator = callback(accumulator, _values[i]);
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
            for (int i = 0; i < _values.Count; i++)
            {
                if (IsPresent(i))
                {
                    accumulator = callback(accumulator, _values[i], i);
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
            for (int i = 0; i < _values.Count; i++)
            {
                if (IsPresent(i))
                {
                    accumulator = callback(accumulator, _values[i], i, this);
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
            for (int i = 0; i < _values.Count; i++)
            {
                if (!IsPresent(i))
                {
                    continue;
                }

                if (!hasAccumulator)
                {
                    accumulator = _values[i];
                    hasAccumulator = true;
                }
                else
                {
                    accumulator = callback(accumulator, _values[i]);
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
            for (int i = _values.Count - 1; i >= 0; i--)
            {
                if (IsPresent(i))
                {
                    accumulator = callback(accumulator, _values[i]);
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
            for (int i = _values.Count - 1; i >= 0; i--)
            {
                if (IsPresent(i))
                {
                    accumulator = callback(accumulator, _values[i], i);
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
            for (int i = _values.Count - 1; i >= 0; i--)
            {
                if (IsPresent(i))
                {
                    accumulator = callback(accumulator, _values[i], i, this);
                }
            }
            return accumulator;
        }

        /// <summary>
        /// Execute callback for each element (value only)
        /// </summary>
        public void forEach(Action<T> callback)
        {
            for (int i = 0; i < _values.Count; i++)
            {
                if (IsPresent(i))
                {
                    callback(_values[i]);
                }
            }
        }

        /// <summary>
        /// Execute callback for each element (value, index)
        /// </summary>
        public void forEach(Action<T, int> callback)
        {
            for (int i = 0; i < _values.Count; i++)
            {
                if (IsPresent(i))
                {
                    callback(_values[i], i);
                }
            }
        }

        /// <summary>
        /// Execute callback for each element (value, index, array)
        /// </summary>
        public void forEach(Action<T, int, JSArray<T>> callback)
        {
            for (int i = 0; i < _values.Count; i++)
            {
                if (IsPresent(i))
                {
                    callback(_values[i], i, this);
                }
            }
        }

        // ==================== Search Methods ====================

        /// <summary>
        /// Find first element matching predicate
        /// </summary>
        public T find(Func<T, bool> callback)
        {
            for (int i = 0; i < _values.Count; i++)
            {
                if (IsPresent(i) && callback(_values[i]))
                {
                    return _values[i];
                }
            }
            return default(T)!;
        }

        /// <summary>
        /// Find first element matching predicate (value, index)
        /// </summary>
        public T find(Func<T, int, bool> callback)
        {
            for (int i = 0; i < _values.Count; i++)
            {
                if (IsPresent(i) && callback(_values[i], i))
                {
                    return _values[i];
                }
            }
            return default(T)!;
        }

        /// <summary>
        /// Find first element matching predicate (value, index, array)
        /// </summary>
        public T find(Func<T, int, JSArray<T>, bool> callback)
        {
            for (int i = 0; i < _values.Count; i++)
            {
                if (IsPresent(i) && callback(_values[i], i, this))
                {
                    return _values[i];
                }
            }
            return default(T)!;
        }

        /// <summary>
        /// Find index of first element matching predicate
        /// </summary>
        public int findIndex(Func<T, bool> callback)
        {
            for (int i = 0; i < _values.Count; i++)
            {
                if (IsPresent(i) && callback(_values[i]))
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
            for (int i = 0; i < _values.Count; i++)
            {
                if (IsPresent(i) && callback(_values[i], i))
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
            for (int i = 0; i < _values.Count; i++)
            {
                if (IsPresent(i) && callback(_values[i], i, this))
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
            for (int i = _values.Count - 1; i >= 0; i--)
            {
                if (IsPresent(i) && callback(_values[i]))
                {
                    return _values[i];
                }
            }
            return default(T)!;
        }

        /// <summary>
        /// Find last element matching predicate (value, index)
        /// </summary>
        public T findLast(Func<T, int, bool> callback)
        {
            for (int i = _values.Count - 1; i >= 0; i--)
            {
                if (IsPresent(i) && callback(_values[i], i))
                {
                    return _values[i];
                }
            }
            return default(T)!;
        }

        /// <summary>
        /// Find last element matching predicate (value, index, array)
        /// </summary>
        public T findLast(Func<T, int, JSArray<T>, bool> callback)
        {
            for (int i = _values.Count - 1; i >= 0; i--)
            {
                if (IsPresent(i) && callback(_values[i], i, this))
                {
                    return _values[i];
                }
            }
            return default(T)!;
        }

        /// <summary>
        /// Find index of last element matching predicate
        /// </summary>
        public int findLastIndex(Func<T, bool> callback)
        {
            for (int i = _values.Count - 1; i >= 0; i--)
            {
                if (IsPresent(i) && callback(_values[i]))
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
            for (int i = _values.Count - 1; i >= 0; i--)
            {
                if (IsPresent(i) && callback(_values[i], i))
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
            for (int i = _values.Count - 1; i >= 0; i--)
            {
                if (IsPresent(i) && callback(_values[i], i, this))
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
            for (int i = start; i < _values.Count; i++)
            {
                if (IsPresent(i) && JSKeyEquality.strictEquals(_values[i], searchElement))
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
            int startIndex = fromIndex ?? _values.Count - 1;
            if (startIndex < 0)
            {
                startIndex = _values.Count + startIndex;
            }
            startIndex = System.Math.Min(startIndex, _values.Count - 1);

            for (int i = startIndex; i >= 0; i--)
            {
                if (IsPresent(i) && JSKeyEquality.strictEquals(_values[i], searchElement))
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
            int start = NormalizeForwardSearchStart(fromIndex);
            for (int i = start; i < _values.Count; i++)
            {
                if (IsPresent(i)
                    ? JSKeyEquality.sameValueZero(_values[i], searchElement)
                    : JSKeyEquality.sameValueZeroUndefined(searchElement))
                {
                    return true;
                }
            }
            return false;
        }

        private int NormalizeForwardSearchStart(int fromIndex)
        {
            if (fromIndex >= _values.Count)
            {
                return _values.Count;
            }
            return fromIndex < 0
                ? System.Math.Max(_values.Count + fromIndex, 0)
                : fromIndex;
        }

        /// <summary>
        /// Test if every element matches predicate
        /// </summary>
        public bool every(Func<T, bool> callback)
        {
            for (int i = 0; i < _values.Count; i++)
            {
                if (IsPresent(i) && !callback(_values[i]))
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
            for (int i = 0; i < _values.Count; i++)
            {
                if (IsPresent(i) && !callback(_values[i], i))
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
            for (int i = 0; i < _values.Count; i++)
            {
                if (IsPresent(i) && !callback(_values[i], i, this))
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
            for (int i = 0; i < _values.Count; i++)
            {
                if (IsPresent(i) && callback(_values[i]))
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
            for (int i = 0; i < _values.Count; i++)
            {
                if (IsPresent(i) && callback(_values[i], i))
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
            for (int i = 0; i < _values.Count; i++)
            {
                if (IsPresent(i) && callback(_values[i], i, this))
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
            var values = _values.ToArray();
            var order = new int[values.Length];
            for (var index = 0; index < order.Length; index++) order[index] = index;
            string[]? keys = null;
            if (compareFunc is null)
            {
                keys = new string[values.Length];
                for (var index = 0; index < values.Length; index++) keys[index] = Globals.String(values[index]);
            }
            System.Array.Sort(order, (left, right) =>
            {
                var comparison = compareFunc is null
                    ? string.CompareOrdinal(keys![left], keys[right])
                    : compareFunc(values[left], values[right]);
                return comparison < 0 ? -1 : comparison > 0 ? 1 : left.CompareTo(right);
            });
            for (var index = 0; index < order.Length; index++)
            {
                if (index < _values.Count) _values[index] = values[order[index]];
                else _values.Add(values[order[index]]);
            }
            return this;
        }

        /// <summary>
        /// Reverse array in place and return the array
        /// </summary>
        public JSArray<T> reverse()
        {
            _values.Reverse();
            return this;
        }

        // ==================== Conversion Methods ====================

        /// <summary>
        /// Join array elements into string
        /// </summary>
        public string join(string separator = ",")
        {
            return Array.join((IReadOnlyList<T>)this, separator);
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
            var result = JSArray<T>.createWithCapacity(_values.Count);
            result._values.AddRange(_values);

            foreach (var item in items)
            {
                if (item is JSArray<T> jsArr)
                {
                    result._values.AddRange(jsArr._values);
                }
                else if (item is IDynamicArray genericJsArray)
                {
                    for (int i = 0; i < genericJsArray.Length; i++)
                    {
                        result._values.Add(genericJsArray.TryGetAt(i, out var value)
                            ? CastArrayValue<T>(value)
                            : default!);
                    }
                }
                else if (item is T value)
                {
                    result.push(value);
                }
                else if (item is IEnumerable<T> values)
                {
                    result.AddPresentRange(values);
                }
            }

            return result;
        }

        /// <summary>
        /// Convert to native array
        /// </summary>
        public T[] toArray()
        {
            var result = new T[_values.Count];
            for (int i = 0; i < _values.Count; i++)
            {
                result[i] = _values[i];
            }

            return result;
        }

        /// <summary>
        /// Convert to List
        /// </summary>
        public List<T> toList()
        {
            var result = new List<T>(_values.Count);
            for (int i = 0; i < _values.Count; i++)
            {
                result.Add(_values[i]);
            }

            return result;
        }

        // ==================== Iterator Methods ====================

        /// <summary>
        /// Get iterator for [index, value] pairs
        /// </summary>
        public IEnumerable<(int index, T value)> entries()
        {
            for (int i = 0; i < _values.Count; i++)
            {
                yield return (i, _values[i]);
            }
        }

        /// <summary>
        /// Get iterator for keys (indices)
        /// </summary>
        public IEnumerable<int> keys()
        {
            for (int i = 0; i < _values.Count; i++)
            {
                yield return i;
            }
        }

        /// <summary>
        /// Get iterator for values
        /// </summary>
        public IEnumerable<T> values()
        {
            for (int i = 0; i < _values.Count; i++)
            {
                yield return _values[i];
            }
        }

        // ==================== Advanced Methods ====================

        /// <summary>
        /// Get element at index (supports negative indices)
        /// </summary>
        public object? at(int index)
        {
            int actualIndex = index < 0 ? _values.Count + index : index;
            if (actualIndex < 0 || actualIndex >= _values.Count)
            {
                return null;
            }
            return IsPresent(actualIndex) ? _values[actualIndex] : null;
        }

        public TValue? atValue<TValue>(int index) where TValue : struct
        {
            object? value = at(index);
            return value is null ? null : (TValue)value;
        }

        public TReference? atReference<TReference>(int index) where TReference : class
        {
            object? value = at(index);
            return value as TReference;
        }

        /// <summary>
        /// Flatten nested arrays by specified depth
        /// </summary>
        public JSArray<object> flat(int depth = 1)
        {
            var result = new JSArray<object>();
            for (int i = 0; i < _values.Count; i++)
            {
                if (IsPresent(i))
                {
                    FlattenValue(_values[i], result, depth);
                }
            }

            return result;
        }

        private static void FlattenValue(object? item, JSArray<object> result, int depth)
        {
            if (depth > 0 && item is IDynamicArray jsArray)
            {
                for (int i = 0; i < jsArray.Length; i++)
                {
                    if (jsArray.TryGetAt(i, out var nestedItem))
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
            for (int i = 0; i < _values.Count; i++)
            {
                if (!IsPresent(i))
                {
                    continue;
                }

                var mapped = callback(_values[i], i, this);

                if (mapped is IDynamicArray jsArr)
                {
                    for (int j = 0; j < jsArr.Length; j++)
                    {
                        if (jsArr.TryGetAt(j, out var val))
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
            int actualStart = start < 0 ? System.Math.Max(0, _values.Count + start) : start;
            int actualEnd = end.HasValue
                ? (end.Value < 0 ? _values.Count + end.Value : end.Value)
                : _values.Count;

            actualStart = System.Math.Min(System.Math.Max(actualStart, 0), _values.Count);
            actualEnd = System.Math.Min(System.Math.Max(actualEnd, 0), _values.Count);

            for (int i = actualStart; i < actualEnd; i++)
            {
                _values[i] = value;
            }
            return this;
        }

        /// <summary>
        /// Copy array section to another location
        /// </summary>
        public JSArray<T> copyWithin(int target, int start = 0, int? end = null)
        {
            int actualTarget = target < 0 ? System.Math.Max(0, _values.Count + target) : target;
            int actualStart = start < 0 ? System.Math.Max(0, _values.Count + start) : start;
            int actualEnd = end.HasValue
                ? (end.Value < 0 ? _values.Count + end.Value : end.Value)
                : _values.Count;

            actualTarget = System.Math.Min(System.Math.Max(actualTarget, 0), _values.Count);
            actualStart = System.Math.Min(System.Math.Max(actualStart, 0), _values.Count);
            actualEnd = System.Math.Min(System.Math.Max(actualEnd, 0), _values.Count);

            int count = System.Math.Min(actualEnd - actualStart, _values.Count - actualTarget);
            count = System.Math.Max(0, System.Math.Min(count, _values.Count - actualStart));

            var values = System.Runtime.InteropServices.CollectionsMarshal.AsSpan(_values);
            values.Slice(actualStart, count).CopyTo(values.Slice(actualTarget, count));

            return this;
        }

        // ==================== Immutable Variants ====================

        /// <summary>
        /// Create new array with element replaced (immutable)
        /// </summary>
        public JSArray<T> with(int index, T value)
        {
            int actualIndex = index < 0 ? _values.Count + index : index;
            if (actualIndex < 0 || actualIndex >= _values.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            var result = CopyValues();
            result[actualIndex] = value;
            return result;
        }

        /// <summary>
        /// Create new reversed array (immutable)
        /// </summary>
        public JSArray<T> toReversed()
        {
            var result = CopyValues();
            result.reverse();
            return result;
        }

        /// <summary>
        /// Create new sorted array (immutable)
        /// </summary>
        public JSArray<T> toSorted(Func<T, T, double>? compareFunc = null)
        {
            var result = CopyValues();
            result.sort(compareFunc);
            return result;
        }

        /// <summary>
        /// Create new spliced array (immutable)
        /// </summary>
        public JSArray<T> toSpliced(int start, int? deleteCount = null, params T[] items)
        {
            var result = CopyValues();
            result.splice(start, deleteCount, items);
            return result;
        }

        // ==================== Static Factory Methods ====================

        /// <summary>
        /// Check if value is a JSArray
        /// </summary>
        public static bool isArray(object? value)
        {
            return value is IDynamicArray;
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
        public static JSArray<T> of(params ReadOnlySpan<T> items)
        {
            var result = createWithCapacity(items.Length);
            foreach (var item in items) result.push(item);
            return result;
        }

        private void AddPresentRange(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                _values.Add(item);
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
            return index >= 0 && index < _values.Count;
        }

        private T ReadValue(int index)
        {
            return index >= 0 && index < _values.Count ? _values[index] : default(T)!;
        }

        private void EnsureLengthForIndex(int index)
        {
            if (index > _values.Count || index == int.MaxValue)
                throw new RangeError("Array assignment exceeds initialized dense storage");
            if (index == _values.Count) _values.Add(default!);
        }

        private void AddDefaults(int count)
        {
            System.Runtime.InteropServices.CollectionsMarshal.SetCount(_values, checked(_values.Count + count));
        }

        private static int ToArrayLength(double length)
        {
            if (double.IsNaN(length) || double.IsInfinity(length) || length < 0 || length > int.MaxValue || System.Math.Truncate(length) != length)
            {
                throw new RangeError("Invalid array length");
            }

            return (int)length;
        }

        private JSArray<T> CopyValues()
        {
            var result = JSArray<T>.createWithCapacity(_values.Count);
            result._values.AddRange(_values);
            return result;
        }

        // ==================== IEnumerable Implementation ====================

        public new IEnumerator<T> GetEnumerator()
        {
            return values().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
