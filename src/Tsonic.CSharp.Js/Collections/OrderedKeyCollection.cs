using System.Collections.Generic;

namespace Tsonic.CSharp.Js;

public abstract class OrderedKeyCollection<TKey, TValue> where TKey : notnull
{
    private readonly Dictionary<TKey, int> _indexes = new();
    private readonly List<KeyValuePair<TKey, TValue>> _entries = new();
    private List<Cursor>? _cursors;

    private protected OrderedKeyCollection() { }

    private protected int KeyCount => _entries.Count;

    private protected void AddKey(TKey key, TValue value)
    {
        if (_indexes.TryAdd(key, _entries.Count)) _entries.Add(new(key, value));
    }

    private protected void SetKey(TKey key, TValue value)
    {
        if (_indexes.TryGetValue(key, out var index)) _entries[index] = new(_entries[index].Key, value);
        else AddKey(key, value);
    }

    private protected bool ContainsKey(TKey key) => _indexes.ContainsKey(key);

    private protected bool TryGetKey(TKey key, out TValue value)
    {
        if (_indexes.TryGetValue(key, out var index))
        {
            value = _entries[index].Value;
            return true;
        }
        value = default!;
        return false;
    }

    private sealed class Cursor
    {
        internal int Next;
    }

    private protected IEnumerable<KeyValuePair<TKey, TValue>> Enumerate()
    {
        var cursor = new Cursor();
        (_cursors ??= new List<Cursor>()).Add(cursor);
        try
        {
            while (cursor.Next < _entries.Count) yield return _entries[cursor.Next++];
        }
        finally
        {
            _cursors.Remove(cursor);
            if (_cursors.Count == 0) _cursors = null;
        }
    }

    private protected bool DeleteKey(TKey key)
    {
        if (!_indexes.Remove(key, out var index)) return false;
        _entries.RemoveAt(index);
        for (var remaining = index; remaining < _entries.Count; remaining++)
            _indexes[_entries[remaining].Key] = remaining;
        if (_cursors is not null)
            foreach (var cursor in _cursors)
                if (index < cursor.Next) cursor.Next--;
        return true;
    }

    private protected void ClearEntries()
    {
        _indexes.Clear();
        _entries.Clear();
        if (_cursors is not null)
            foreach (var cursor in _cursors) cursor.Next = 0;
    }
}
