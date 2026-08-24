using System;
using System.Collections;
using System.Collections.Generic;

namespace Tsonic.CSharp.Js;

public sealed class RegExpStringIterator : IEnumerable<RegExpExecArray>, IEnumerator<RegExpExecArray>
{
    private readonly RegExp _expression;
    private readonly string _input;
    private readonly bool _global;
    private bool _completed;

    internal RegExpStringIterator(RegExp expression, string input)
    {
        _expression = expression.CloneForIteration();
        _input = input;
        _global = expression.global;
    }

    public RegExpExecArray Current { get; private set; } = null!;
    object IEnumerator.Current => Current;
    public IEnumerator<RegExpExecArray> GetEnumerator() => this;
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool MoveNext()
    {
        if (_completed) return false;
        var result = _expression.exec(_input);
        if (result is null)
        {
            _completed = true;
            return false;
        }
        Current = result;
        if (!_global)
        {
            _completed = true;
        }
        else if (result.value.Length == 0)
        {
            _expression.AdvanceAfterEmptyMatch(_input);
        }
        return true;
    }

    public void Reset() => throw new NotSupportedException();
    public void Dispose() { }
}
