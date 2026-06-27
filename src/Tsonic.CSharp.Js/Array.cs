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

        public static int push<T>(JSArray<T> array, T item)
        {
            return array.push(item);
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

        public static T? popValue<T>(JSArray<T> array) where T : struct
        {
            return tryPop(array, out var value) ? value : null;
        }

        public static T? popReference<T>(JSArray<T> array) where T : class
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

        private static bool tryPop<T>(JSArray<T> array, out T value)
        {
            var index = array.length - 1;
            if (index < 0)
            {
                value = default!;
                return false;
            }

            var present = array.tryGetAt(index, out value);
            array.setLength(index);
            return present;
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

        public static T? shiftValue<T>(JSArray<T> array) where T : struct
        {
            return tryShift(array, out var value) ? value : null;
        }

        public static T? shiftReference<T>(JSArray<T> array) where T : class
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

        private static bool tryShift<T>(JSArray<T> array, out T value)
        {
            if (array.length == 0)
            {
                value = default!;
                return false;
            }

            var present = array.tryGetAt(0, out value);
            array.shift();
            return present;
        }

        public static int unshift<T>(List<T> array, T item)
        {
            array.Insert(0, item);
            return array.Count;
        }

        public static int unshift<T>(JSArray<T> array, T item)
        {
            return array.unshift(item);
        }

        public static object? at<T>(IReadOnlyList<T> array, int index)
        {
            var actualIndex = index < 0 ? array.Count + index : index;
            if (actualIndex < 0 || actualIndex >= array.Count)
            {
                return JSUndefined.value;
            }
            return array[actualIndex];
        }

        public static object? at<T>(JSArray<T> array, int index)
        {
            var actualIndex = normalizeAtIndex(index, array.length);
            return actualIndex >= 0 && array.tryGetAtObject(actualIndex, out var value) ? value : JSUndefined.value;
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

        public static T? atValue<T>(JSArray<T> array, int index) where T : struct
        {
            var actualIndex = normalizeAtIndex(index, array.length);
            return actualIndex >= 0 && array.tryGetAt(actualIndex, out var value) ? value : null;
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

        public static T? atReference<T>(JSArray<T> array, int index) where T : class
        {
            var actualIndex = normalizeAtIndex(index, array.length);
            return actualIndex >= 0 && array.tryGetAt(actualIndex, out var value) ? value : null;
        }

        public static bool includes<T>(IReadOnlyList<T> array, T searchElement, int fromIndex = 0)
        {
            var start = normalizeStart(fromIndex, array.Count);
            for (var index = start; index < array.Count; index++)
            {
                if (JSKeyEquality.sameValueZero(array[index], searchElement))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool includes<T>(JSArray<T> array, T searchElement, int fromIndex = 0)
        {
            return array.includes(searchElement, fromIndex);
        }

        public static int indexOf<T>(IReadOnlyList<T> array, T searchElement, int fromIndex = 0)
        {
            var start = normalizeStart(fromIndex, array.Count);
            for (var index = start; index < array.Count; index++)
            {
                if (JSKeyEquality.strictEquals(array[index], searchElement))
                {
                    return index;
                }
            }
            return -1;
        }

        public static int indexOf<T>(JSArray<T> array, T searchElement, int fromIndex = 0)
        {
            return array.indexOf(searchElement, fromIndex);
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
            for (var index = start; index >= 0; index--)
            {
                if (JSKeyEquality.strictEquals(array[index], searchElement))
                {
                    return index;
                }
            }
            return -1;
        }

        public static int lastIndexOf<T>(JSArray<T> array, T searchElement, int fromIndex = int.MaxValue)
        {
            return fromIndex == int.MaxValue
                ? array.lastIndexOf(searchElement)
                : array.lastIndexOf(searchElement, fromIndex);
        }

        public static string join<T>(IReadOnlyList<T> array, string separator = ",")
        {
            return string.Join(separator, array);
        }

        public static string join<T>(JSArray<T> array, string separator = ",")
        {
            return array.join(separator);
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

        public static JSArray<T> slice<T>(JSArray<T> array, int start = 0, int? end = null)
        {
            return array.slice(start, end);
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

        public static JSArray<T> splice<T>(JSArray<T> array, int start, int? deleteCount = null, params T[] items)
        {
            return array.splice(start, deleteCount, items);
        }

        public static List<T> reverse<T>(List<T> array)
        {
            array.Reverse();
            return array;
        }

        public static JSArray<T> reverse<T>(JSArray<T> array)
        {
            return array.reverse();
        }

        public static List<T> sort<T>(List<T> array, Func<T, T, double> compareFn)
        {
            array.Sort((left, right) => SysMath.Sign(compareFn(left, right)));
            return array;
        }

        public static JSArray<T> sort<T>(JSArray<T> array, Func<T, T, double> compareFn)
        {
            return array.sort(compareFn);
        }

        public static List<T> fill<T>(List<T> array, T value, int start = 0, int? end = null)
        {
            var actualStart = normalizeStart(start, array.Count);
            var actualEnd = normalizeEnd(end, array.Count);
            for (var index = actualStart; index < actualEnd; index++)
            {
                array[index] = value;
            }
            return array;
        }

        public static JSArray<T> fill<T>(JSArray<T> array, T value, int start = 0, int? end = null)
        {
            return array.fill(value, start, end);
        }

        public static List<T> copyWithin<T>(List<T> array, int target, int start = 0, int? end = null)
        {
            var actualTarget = normalizeStart(target, array.Count);
            var actualStart = normalizeStart(start, array.Count);
            var actualEnd = normalizeEnd(end, array.Count);
            var count = SysMath.Max(0, SysMath.Min(actualEnd - actualStart, array.Count - actualTarget));
            var values = array.GetRange(actualStart, count);
            for (var offset = 0; offset < values.Count; offset++)
            {
                array[actualTarget + offset] = values[offset];
            }
            return array;
        }

        public static JSArray<T> copyWithin<T>(JSArray<T> array, int target, int start = 0, int? end = null)
        {
            return array.copyWithin(target, start, end);
        }

        public static List<T> toReversed<T>(IReadOnlyList<T> array)
        {
            var result = new List<T>(array);
            result.Reverse();
            return result;
        }

        public static JSArray<T> toReversed<T>(JSArray<T> array)
        {
            return array.toReversed();
        }

        public static List<T> toSorted<T>(IReadOnlyList<T> array, Func<T, T, double> compareFn)
        {
            var result = new List<T>(array);
            result.Sort((left, right) => SysMath.Sign(compareFn(left, right)));
            return result;
        }

        public static JSArray<T> toSorted<T>(JSArray<T> array, Func<T, T, double> compareFn)
        {
            return array.toSorted(compareFn);
        }

        public static List<T> toSpliced<T>(IReadOnlyList<T> array, int start, int? deleteCount = null, params T[] items)
        {
            var result = new List<T>(array);
            splice(result, start, deleteCount, items);
            return result;
        }

        public static JSArray<T> toSpliced<T>(JSArray<T> array, int start, int? deleteCount = null, params T[] items)
        {
            return array.toSpliced(start, deleteCount, items);
        }

        public static List<T> with<T>(IReadOnlyList<T> array, int index, T value)
        {
            var actualIndex = normalizeAtIndex(index, array.Count);
            if (actualIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            var result = new List<T>(array);
            result[actualIndex] = value;
            return result;
        }

        public static JSArray<T> with<T>(JSArray<T> array, int index, T value)
        {
            return array.with(index, value);
        }

        public static IEnumerable<int> keys<T>(IReadOnlyList<T> array)
        {
            for (var index = 0; index < array.Count; index++)
            {
                yield return index;
            }
        }

        public static IEnumerable<int> keys<T>(JSArray<T> array)
        {
            return array.keys();
        }

        public static IEnumerable<T> values<T>(IReadOnlyList<T> array)
        {
            return array;
        }

        public static IEnumerable<T> values<T>(JSArray<T> array)
        {
            return array.values();
        }

        public static IEnumerable<(int index, T value)> entries<T>(IReadOnlyList<T> array)
        {
            for (var index = 0; index < array.Count; index++)
            {
                yield return (index, array[index]);
            }
        }

        public static IEnumerable<(int index, T value)> entries<T>(JSArray<T> array)
        {
            return array.entries();
        }

        public static void forEach<T>(IReadOnlyList<T> array, Action<T> callback)
        {
            for (var index = 0; index < array.Count; index++)
            {
                callback(array[index]);
            }
        }

        public static void forEach<T>(JSArray<T> array, Action<T> callback)
        {
            array.forEach(callback);
        }

        public static void forEach<T>(IReadOnlyList<T> array, Action<T, int> callback)
        {
            for (var index = 0; index < array.Count; index++)
            {
                callback(array[index], index);
            }
        }

        public static void forEach<T>(JSArray<T> array, Action<T, int> callback)
        {
            array.forEach(callback);
        }

        public static void forEach<T>(IReadOnlyList<T> array, Action<T, int, IReadOnlyList<T>> callback)
        {
            for (var index = 0; index < array.Count; index++)
            {
                callback(array[index], index, array);
            }
        }

        public static void forEach<T>(JSArray<T> array, Action<T, int, JSArray<T>> callback)
        {
            array.forEach(callback);
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

        public static bool some<T>(JSArray<T> array, Func<T, bool> callback)
        {
            return array.some(callback);
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

        public static bool some<T>(JSArray<T> array, Func<T, int, bool> callback)
        {
            return array.some(callback);
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

        public static bool some<T>(JSArray<T> array, Func<T, int, JSArray<T>, bool> callback)
        {
            return array.some(callback);
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

        public static bool every<T>(JSArray<T> array, Func<T, bool> callback)
        {
            return array.every(callback);
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

        public static bool every<T>(JSArray<T> array, Func<T, int, bool> callback)
        {
            return array.every(callback);
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

        public static bool every<T>(JSArray<T> array, Func<T, int, JSArray<T>, bool> callback)
        {
            return array.every(callback);
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

        public static JSArray<T> filter<T>(JSArray<T> array, Func<T, bool> callback)
        {
            return array.filter(callback);
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

        public static JSArray<T> filter<T>(JSArray<T> array, Func<T, int, bool> callback)
        {
            return array.filter(callback);
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

        public static JSArray<T> filter<T>(JSArray<T> array, Func<T, int, JSArray<T>, bool> callback)
        {
            return array.filter(callback);
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

        public static T? findValue<T>(JSArray<T> array, Func<T, bool> callback) where T : struct
        {
            return tryFind(array, callback, out var value) ? value : null;
        }

        public static T? findReference<T>(JSArray<T> array, Func<T, bool> callback) where T : class
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

        private static bool tryFind<T>(JSArray<T> array, Func<T, bool> callback, out T value)
        {
            for (var index = 0; index < array.length; index++)
            {
                if (array.tryGetAt(index, out value) && callback(value))
                {
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

        public static T? findValue<T>(JSArray<T> array, Func<T, int, bool> callback) where T : struct
        {
            return tryFind(array, callback, out var value) ? value : null;
        }

        public static T? findReference<T>(JSArray<T> array, Func<T, int, bool> callback) where T : class
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

        private static bool tryFind<T>(JSArray<T> array, Func<T, int, bool> callback, out T value)
        {
            for (var index = 0; index < array.length; index++)
            {
                if (array.tryGetAt(index, out value) && callback(value, index))
                {
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

        public static T? findValue<T>(JSArray<T> array, Func<T, int, JSArray<T>, bool> callback) where T : struct
        {
            return tryFind(array, callback, out var value) ? value : null;
        }

        public static T? findReference<T>(JSArray<T> array, Func<T, int, JSArray<T>, bool> callback) where T : class
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

        private static bool tryFind<T>(JSArray<T> array, Func<T, int, JSArray<T>, bool> callback, out T value)
        {
            for (var index = 0; index < array.length; index++)
            {
                if (array.tryGetAt(index, out value) && callback(value, index, array))
                {
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

        public static T? findLastValue<T>(JSArray<T> array, Func<T, bool> callback) where T : struct
        {
            return tryFindLast(array, callback, out var value) ? value : null;
        }

        public static T? findLastReference<T>(JSArray<T> array, Func<T, bool> callback) where T : class
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

        private static bool tryFindLast<T>(JSArray<T> array, Func<T, bool> callback, out T value)
        {
            for (var index = array.length - 1; index >= 0; index--)
            {
                if (array.tryGetAt(index, out value) && callback(value))
                {
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

        public static T? findLastValue<T>(JSArray<T> array, Func<T, int, bool> callback) where T : struct
        {
            return tryFindLast(array, callback, out var value) ? value : null;
        }

        public static T? findLastReference<T>(JSArray<T> array, Func<T, int, bool> callback) where T : class
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

        private static bool tryFindLast<T>(JSArray<T> array, Func<T, int, bool> callback, out T value)
        {
            for (var index = array.length - 1; index >= 0; index--)
            {
                if (array.tryGetAt(index, out value) && callback(value, index))
                {
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

        public static T? findLastValue<T>(JSArray<T> array, Func<T, int, JSArray<T>, bool> callback) where T : struct
        {
            return tryFindLast(array, callback, out var value) ? value : null;
        }

        public static T? findLastReference<T>(JSArray<T> array, Func<T, int, JSArray<T>, bool> callback) where T : class
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

        private static bool tryFindLast<T>(JSArray<T> array, Func<T, int, JSArray<T>, bool> callback, out T value)
        {
            for (var index = array.length - 1; index >= 0; index--)
            {
                if (array.tryGetAt(index, out value) && callback(value, index, array))
                {
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

        public static int findIndex<T>(JSArray<T> array, Func<T, bool> callback)
        {
            return array.findIndex(callback);
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

        public static int findIndex<T>(JSArray<T> array, Func<T, int, bool> callback)
        {
            return array.findIndex(callback);
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

        public static int findIndex<T>(JSArray<T> array, Func<T, int, JSArray<T>, bool> callback)
        {
            return array.findIndex(callback);
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

        public static int findLastIndex<T>(JSArray<T> array, Func<T, bool> callback)
        {
            return array.findLastIndex(callback);
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

        public static int findLastIndex<T>(JSArray<T> array, Func<T, int, bool> callback)
        {
            return array.findLastIndex(callback);
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

        public static int findLastIndex<T>(JSArray<T> array, Func<T, int, JSArray<T>, bool> callback)
        {
            return array.findLastIndex(callback);
        }

        public static List<TResult> map<T, TResult>(IReadOnlyList<T> array, Func<T, TResult> callback)
        {
            var result = new List<TResult>(array.Count);
            for (var index = 0; index < array.Count; index++)
            {
                result.Add(callback(array[index]));
            }
            return result;
        }

        public static JSArray<TResult> map<T, TResult>(JSArray<T> array, Func<T, TResult> callback)
        {
            return array.map(callback);
        }

        public static List<TResult> map<T, TResult>(IReadOnlyList<T> array, Func<T, int, TResult> callback)
        {
            var result = new List<TResult>(array.Count);
            for (var index = 0; index < array.Count; index++)
            {
                result.Add(callback(array[index], index));
            }
            return result;
        }

        public static JSArray<TResult> map<T, TResult>(JSArray<T> array, Func<T, int, TResult> callback)
        {
            return array.map(callback);
        }

        public static List<TResult> map<T, TResult>(IReadOnlyList<T> array, Func<T, int, IReadOnlyList<T>, TResult> callback)
        {
            var result = new List<TResult>(array.Count);
            for (var index = 0; index < array.Count; index++)
            {
                result.Add(callback(array[index], index, array));
            }
            return result;
        }

        public static JSArray<TResult> map<T, TResult>(JSArray<T> array, Func<T, int, JSArray<T>, TResult> callback)
        {
            return array.map(callback);
        }

        private static int normalizeStart(int start, int length)
        {
            if (start < 0)
            {
                return SysMath.Max(0, length + start);
            }
            return SysMath.Min(start, length);
        }

        private static int normalizeEnd(int? end, int length)
        {
            if (!end.HasValue)
            {
                return length;
            }

            if (end.Value < 0)
            {
                return SysMath.Max(0, length + end.Value);
            }

            return SysMath.Min(end.Value, length);
        }

        private static int normalizeAtIndex(int index, int length)
        {
            var actualIndex = index < 0 ? length + index : index;
            return actualIndex < 0 || actualIndex >= length ? -1 : actualIndex;
        }
    }
}
