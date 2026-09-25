using System;

namespace Tsonic.CSharp.Js
{
    public partial class JSArray<T>
    {
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

    }
}
