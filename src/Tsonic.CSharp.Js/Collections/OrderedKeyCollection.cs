using System;
using System.Collections.Generic;

namespace Tsonic.CSharp.Js;

internal struct OrderedKeyCollection<TKey, TValue> where TKey : notnull
{
    private readonly OrderedDictionary<TKey, TValue> _entries;
    private List<Cursor>? _cursors;

    public OrderedKeyCollection()
    {
        _entries = new();
        _cursors = null;
    }

    public readonly int Count => _entries.Count;
    public readonly void Add(TKey key, TValue value) => _entries.TryAdd(key, value);
    public readonly void Set(TKey key, TValue value) => _entries[key] = value;
    public readonly bool Contains(TKey key) => _entries.ContainsKey(key);
    public readonly bool TryGet(TKey key, out TValue value) => _entries.TryGetValue(key, out value!);

    public readonly bool Delete(TKey key)
    {
        var index = _entries.IndexOf(key);
        if (index < 0) return false;
        _entries.RemoveAt(index);
        if (_cursors is not null)
            foreach (var cursor in _cursors)
                if (index < cursor.Next) cursor.Next--;
        return true;
    }

    public readonly void Clear()
    {
        _entries.Clear();
        if (_cursors is not null)
            foreach (var cursor in _cursors) cursor.Next = 0;
    }

    public IEnumerable<KeyValuePair<TKey, TValue>> Enumerate()
    {
        _cursors ??= new();
        return Enumerate(_entries, _cursors);
    }

    private static IEnumerable<KeyValuePair<TKey, TValue>> Enumerate(
        OrderedDictionary<TKey, TValue> entries, List<Cursor> cursors)
    {
        var cursor = new Cursor();
        cursors.Add(cursor);
        try
        {
            while (cursor.Next < entries.Count)
                yield return entries.GetAt(cursor.Next++);
        }
        finally
        {
            cursors.Remove(cursor);
        }
    }

    private sealed class Cursor
    {
        internal int Next;
    }
}
