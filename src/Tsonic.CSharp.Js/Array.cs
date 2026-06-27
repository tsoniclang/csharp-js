using System;
using System.Collections.Generic;
using System.Linq;
using SysMath = System.Math;

namespace Tsonic.CSharp.Js
{
    public static class Array
    {
        public static List<T> from<T>(IEnumerable<T> iterable)
        {
            return new List<T>(iterable);
        }

        public static List<T> from<T>(IEnumerable<T> iterable, Func<T, int, T> mapFunc)
        {
            var result = new List<T>();
            var index = 0;
            foreach (var item in iterable)
            {
                result.Add(mapFunc(item, index));
                index++;
            }
            return result;
        }

        public static List<T> from<T>(IEnumerable<T> iterable, Func<T, T> mapFunc)
        {
            var result = new List<T>();
            foreach (var item in iterable)
            {
                result.Add(mapFunc(item));
            }
            return result;
        }

        public static List<string> from(string source)
        {
            return source.Select(character => character.ToString()).ToList();
        }

        public static List<T> of<T>(params T[] items)
        {
            return new List<T>(items);
        }

        public static List<T> concat<T>(IEnumerable<T> array, params IEnumerable<T>[] items)
        {
            var result = new List<T>(array);
            foreach (var item in items)
            {
                result.AddRange(item);
            }
            return result;
        }

        public static bool isArray<T>(IReadOnlyList<T> value)
        {
            return value != null;
        }

        public static int push<T>(List<T> array, T item)
        {
            array.Add(item);
            return array.Count;
        }

        public static T? pop<T>(List<T> array)
        {
            return tryPop(array, out var value) ? value : default;
        }

        public static T? popValue<T>(List<T> array) where T : struct
        {
            return tryPop(array, out var value) ? value : null;
        }

        public static T? popReference<T>(List<T> array) where T : class
        {
            return tryPop(array, out var value) ? value : null;
        }

        private static bool tryPop<T>(List<T> array, out T value)
        {
            if (array.Count == 0)
            {
                value = default!;
                return false;
            }
            var index = array.Count - 1;
            value = array[index];
            array.RemoveAt(index);
            return true;
        }

        public static T? shift<T>(List<T> array)
        {
            return tryShift(array, out var value) ? value : default;
        }

        public static T? shiftValue<T>(List<T> array) where T : struct
        {
            return tryShift(array, out var value) ? value : null;
        }

        public static T? shiftReference<T>(List<T> array) where T : class
        {
            return tryShift(array, out var value) ? value : null;
        }

        private static bool tryShift<T>(List<T> array, out T value)
        {
            if (array.Count == 0)
            {
                value = default!;
                return false;
            }
            value = array[0];
            array.RemoveAt(0);
            return true;
        }

        public static int unshift<T>(List<T> array, T item)
        {
            array.Insert(0, item);
            return array.Count;
        }

        public static object? at<T>(IReadOnlyList<T> array, int index)
        {
            var actualIndex = index < 0 ? array.Count + index : index;
            if (actualIndex < 0 || actualIndex >= array.Count)
            {
                return null;
            }
            return array[actualIndex];
        }

        public static T? atValue<T>(IReadOnlyList<T> array, int index) where T : struct
        {
            var actualIndex = index < 0 ? array.Count + index : index;
            if (actualIndex < 0 || actualIndex >= array.Count)
            {
                return null;
            }
            return array[actualIndex];
        }

        public static T? atReference<T>(IReadOnlyList<T> array, int index) where T : class
        {
            var actualIndex = index < 0 ? array.Count + index : index;
            if (actualIndex < 0 || actualIndex >= array.Count)
            {
                return null;
            }
            return array[actualIndex];
        }

        public static bool includes<T>(IReadOnlyList<T> array, T searchElement, int fromIndex = 0)
        {
            return indexOf(array, searchElement, fromIndex) >= 0;
        }

        public static int indexOf<T>(IReadOnlyList<T> array, T searchElement, int fromIndex = 0)
        {
            var start = normalizeStart(fromIndex, array.Count);
            var comparer = EqualityComparer<T>.Default;
            for (var index = start; index < array.Count; index++)
            {
                if (comparer.Equals(array[index], searchElement))
                {
                    return index;
                }
            }
            return -1;
        }

        public static int lastIndexOf<T>(IReadOnlyList<T> array, T searchElement, int fromIndex = int.MaxValue)
        {
            var start = fromIndex == int.MaxValue ? array.Count - 1 : fromIndex;
            if (start < 0)
            {
                start = array.Count + start;
            }
            if (start >= array.Count)
            {
                start = array.Count - 1;
            }
            var comparer = EqualityComparer<T>.Default;
            for (var index = start; index >= 0; index--)
            {
                if (comparer.Equals(array[index], searchElement))
                {
                    return index;
                }
            }
            return -1;
        }

        public static string join<T>(IReadOnlyList<T> array, string separator = ",")
        {
            return string.Join(separator, array);
        }

        public static List<T> slice<T>(IReadOnlyList<T> array, int start = 0, int? end = null)
        {
            var actualStart = normalizeStart(start, array.Count);
            var actualEnd = end ?? array.Count;
            if (actualEnd < 0)
            {
                actualEnd = SysMath.Max(0, array.Count + actualEnd);
            }
            actualEnd = SysMath.Min(actualEnd, array.Count);
            var count = SysMath.Max(0, actualEnd - actualStart);
            var result = new List<T>(count);
            for (var index = 0; index < count; index++)
            {
                result.Add(array[actualStart + index]);
            }
            return result;
        }

        public static List<T> splice<T>(List<T> array, int start, int? deleteCount = null, params T[] items)
        {
            var actualStart = normalizeStart(start, array.Count);
            var actualDeleteCount = SysMath.Max(0, SysMath.Min(deleteCount ?? array.Count - actualStart, array.Count - actualStart));
            var removed = array.GetRange(actualStart, actualDeleteCount);
            array.RemoveRange(actualStart, actualDeleteCount);
            if (items.Length > 0)
            {
                array.InsertRange(actualStart, items);
            }
            return removed;
        }

        public static List<T> reverse<T>(List<T> array)
        {
            array.Reverse();
            return array;
        }

        public static List<T> sort<T>(List<T> array, Func<T, T, double> compareFn)
        {
            array.Sort((left, right) => SysMath.Sign(compareFn(left, right)));
            return array;
        }

        public static void forEach<T>(IReadOnlyList<T> array, Action<T> callback)
        {
            for (var index = 0; index < array.Count; index++)
            {
                callback(array[index]);
            }
        }

        public static void forEach<T>(IReadOnlyList<T> array, Action<T, int> callback)
        {
            for (var index = 0; index < array.Count; index++)
            {
                callback(array[index], index);
            }
        }

        public static void forEach<T>(IReadOnlyList<T> array, Action<T, int, IReadOnlyList<T>> callback)
        {
            for (var index = 0; index < array.Count; index++)
            {
                callback(array[index], index, array);
            }
        }

        public static bool some<T>(IReadOnlyList<T> array, Func<T, bool> callback)
        {
            for (var index = 0; index < array.Count; index++)
            {
                if (callback(array[index]))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool some<T>(IReadOnlyList<T> array, Func<T, int, bool> callback)
        {
            for (var index = 0; index < array.Count; index++)
            {
                if (callback(array[index], index))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool some<T>(IReadOnlyList<T> array, Func<T, int, IReadOnlyList<T>, bool> callback)
        {
            for (var index = 0; index < array.Count; index++)
            {
                if (callback(array[index], index, array))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool every<T>(IReadOnlyList<T> array, Func<T, bool> callback)
        {
            for (var index = 0; index < array.Count; index++)
            {
                if (!callback(array[index]))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool every<T>(IReadOnlyList<T> array, Func<T, int, bool> callback)
        {
            for (var index = 0; index < array.Count; index++)
            {
                if (!callback(array[index], index))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool every<T>(IReadOnlyList<T> array, Func<T, int, IReadOnlyList<T>, bool> callback)
        {
            for (var index = 0; index < array.Count; index++)
            {
                if (!callback(array[index], index, array))
                {
                    return false;
                }
            }
            return true;
        }

        public static List<T> filter<T>(IReadOnlyList<T> array, Func<T, bool> callback)
        {
            var result = new List<T>();
            for (var index = 0; index < array.Count; index++)
            {
                if (callback(array[index]))
                {
                    result.Add(array[index]);
                }
            }
            return result;
        }

        public static List<T> filter<T>(IReadOnlyList<T> array, Func<T, int, bool> callback)
        {
            var result = new List<T>();
            for (var index = 0; index < array.Count; index++)
            {
                if (callback(array[index], index))
                {
                    result.Add(array[index]);
                }
            }
            return result;
        }

        public static List<T> filter<T>(IReadOnlyList<T> array, Func<T, int, IReadOnlyList<T>, bool> callback)
        {
            var result = new List<T>();
            for (var index = 0; index < array.Count; index++)
            {
                if (callback(array[index], index, array))
                {
                    result.Add(array[index]);
                }
            }
            return result;
        }

        public static T? find<T>(IReadOnlyList<T> array, Func<T, bool> callback)
        {
            return tryFind(array, callback, out var value) ? value : default;
        }

        public static T? findValue<T>(IReadOnlyList<T> array, Func<T, bool> callback) where T : struct
        {
            return tryFind(array, callback, out var value) ? value : null;
        }

        public static T? findReference<T>(IReadOnlyList<T> array, Func<T, bool> callback) where T : class
        {
            return tryFind(array, callback, out var value) ? value : null;
        }

        private static bool tryFind<T>(IReadOnlyList<T> array, Func<T, bool> callback, out T value)
        {
            for (var index = 0; index < array.Count; index++)
            {
                if (callback(array[index]))
                {
                    value = array[index];
                    return true;
                }
            }
            value = default!;
            return false;
        }

        public static T? find<T>(IReadOnlyList<T> array, Func<T, int, bool> callback)
        {
            return tryFind(array, callback, out var value) ? value : default;
        }

        public static T? findValue<T>(IReadOnlyList<T> array, Func<T, int, bool> callback) where T : struct
        {
            return tryFind(array, callback, out var value) ? value : null;
        }

        public static T? findReference<T>(IReadOnlyList<T> array, Func<T, int, bool> callback) where T : class
        {
            return tryFind(array, callback, out var value) ? value : null;
        }

        private static bool tryFind<T>(IReadOnlyList<T> array, Func<T, int, bool> callback, out T value)
        {
            for (var index = 0; index < array.Count; index++)
            {
                if (callback(array[index], index))
                {
                    value = array[index];
                    return true;
                }
            }
            value = default!;
            return false;
        }

        public static T? find<T>(IReadOnlyList<T> array, Func<T, int, IReadOnlyList<T>, bool> callback)
        {
            return tryFind(array, callback, out var value) ? value : default;
        }

        public static T? findValue<T>(IReadOnlyList<T> array, Func<T, int, IReadOnlyList<T>, bool> callback) where T : struct
        {
            return tryFind(array, callback, out var value) ? value : null;
        }

        public static T? findReference<T>(IReadOnlyList<T> array, Func<T, int, IReadOnlyList<T>, bool> callback) where T : class
        {
            return tryFind(array, callback, out var value) ? value : null;
        }

        private static bool tryFind<T>(IReadOnlyList<T> array, Func<T, int, IReadOnlyList<T>, bool> callback, out T value)
        {
            for (var index = 0; index < array.Count; index++)
            {
                if (callback(array[index], index, array))
                {
                    value = array[index];
                    return true;
                }
            }
            value = default!;
            return false;
        }

        public static T? findLast<T>(IReadOnlyList<T> array, Func<T, bool> callback)
        {
            return tryFindLast(array, callback, out var value) ? value : default;
        }

        public static T? findLastValue<T>(IReadOnlyList<T> array, Func<T, bool> callback) where T : struct
        {
            return tryFindLast(array, callback, out var value) ? value : null;
        }

        public static T? findLastReference<T>(IReadOnlyList<T> array, Func<T, bool> callback) where T : class
        {
            return tryFindLast(array, callback, out var value) ? value : null;
        }

        private static bool tryFindLast<T>(IReadOnlyList<T> array, Func<T, bool> callback, out T value)
        {
            for (var index = array.Count - 1; index >= 0; index--)
            {
                if (callback(array[index]))
                {
                    value = array[index];
                    return true;
                }
            }
            value = default!;
            return false;
        }

        public static T? findLast<T>(IReadOnlyList<T> array, Func<T, int, bool> callback)
        {
            return tryFindLast(array, callback, out var value) ? value : default;
        }

        public static T? findLastValue<T>(IReadOnlyList<T> array, Func<T, int, bool> callback) where T : struct
        {
            return tryFindLast(array, callback, out var value) ? value : null;
        }

        public static T? findLastReference<T>(IReadOnlyList<T> array, Func<T, int, bool> callback) where T : class
        {
            return tryFindLast(array, callback, out var value) ? value : null;
        }

        private static bool tryFindLast<T>(IReadOnlyList<T> array, Func<T, int, bool> callback, out T value)
        {
            for (var index = array.Count - 1; index >= 0; index--)
            {
                if (callback(array[index], index))
                {
                    value = array[index];
                    return true;
                }
            }
            value = default!;
            return false;
        }

        public static T? findLast<T>(IReadOnlyList<T> array, Func<T, int, IReadOnlyList<T>, bool> callback)
        {
            return tryFindLast(array, callback, out var value) ? value : default;
        }

        public static T? findLastValue<T>(IReadOnlyList<T> array, Func<T, int, IReadOnlyList<T>, bool> callback) where T : struct
        {
            return tryFindLast(array, callback, out var value) ? value : null;
        }

        public static T? findLastReference<T>(IReadOnlyList<T> array, Func<T, int, IReadOnlyList<T>, bool> callback) where T : class
        {
            return tryFindLast(array, callback, out var value) ? value : null;
        }

        private static bool tryFindLast<T>(IReadOnlyList<T> array, Func<T, int, IReadOnlyList<T>, bool> callback, out T value)
        {
            for (var index = array.Count - 1; index >= 0; index--)
            {
                if (callback(array[index], index, array))
                {
                    value = array[index];
                    return true;
                }
            }
            value = default!;
            return false;
        }

        public static int findIndex<T>(IReadOnlyList<T> array, Func<T, bool> callback)
        {
            for (var index = 0; index < array.Count; index++)
            {
                if (callback(array[index]))
                {
                    return index;
                }
            }
            return -1;
        }

        public static int findIndex<T>(IReadOnlyList<T> array, Func<T, int, bool> callback)
        {
            for (var index = 0; index < array.Count; index++)
            {
                if (callback(array[index], index))
                {
                    return index;
                }
            }
            return -1;
        }

        public static int findIndex<T>(IReadOnlyList<T> array, Func<T, int, IReadOnlyList<T>, bool> callback)
        {
            for (var index = 0; index < array.Count; index++)
            {
                if (callback(array[index], index, array))
                {
                    return index;
                }
            }
            return -1;
        }

        public static int findLastIndex<T>(IReadOnlyList<T> array, Func<T, bool> callback)
        {
            for (var index = array.Count - 1; index >= 0; index--)
            {
                if (callback(array[index]))
                {
                    return index;
                }
            }
            return -1;
        }

        public static int findLastIndex<T>(IReadOnlyList<T> array, Func<T, int, bool> callback)
        {
            for (var index = array.Count - 1; index >= 0; index--)
            {
                if (callback(array[index], index))
                {
                    return index;
                }
            }
            return -1;
        }

        public static int findLastIndex<T>(IReadOnlyList<T> array, Func<T, int, IReadOnlyList<T>, bool> callback)
        {
            for (var index = array.Count - 1; index >= 0; index--)
            {
                if (callback(array[index], index, array))
                {
                    return index;
                }
            }
            return -1;
        }

        private static int normalizeStart(int start, int length)
        {
            if (start < 0)
            {
                return SysMath.Max(0, length + start);
            }
            return SysMath.Min(start, length);
        }
    }
}
