using System;
using System.Collections.Generic;
using QuickJs = Tsonic.CSharp.Js.RegExpRuntime.Engine.QuickJs;

namespace Tsonic.CSharp.Js.RegExpRuntime.Engine;

internal sealed class EcmaRegExpProgram
{
    private const long MinimumExecutionSteps = 1_000_000;
    private const long MaximumExecutionSteps = 50_000_000;
    private const long StepsPerInputCodeUnit = 4_096;
    private readonly QuickJs.QuickJsRegExpEngine _engine;

    private EcmaRegExpProgram(
        QuickJs.QuickJsRegExpEngine engine,
        RegExpFlags flags)
    {
        _engine = engine;
        Flags = flags;
        var names = new List<KeyValuePair<string, int>>();
        for (var index = 1; index < engine.CaptureCount; index += 1)
        {
            var name = engine.GetGroupName(index);
            if (name is not null)
            {
                names.Add(new KeyValuePair<string, int>(name, index));
            }
        }
        NamedCaptures = names;
    }

    public int CaptureCount => _engine.CaptureCount;
    public IReadOnlyList<KeyValuePair<string, int>> NamedCaptures { get; }
    public RegExpFlags Flags { get; }

    public static EcmaRegExpProgram Compile(string pattern, RegExpFlags flags)
    {
        try
        {
            return new EcmaRegExpProgram(
                QuickJs.QuickJsRegExpEngine.Compile(
                    pattern,
                    ToEngineFlags(flags)),
                flags);
        }
        catch (QuickJs.RegExpSyntaxException error)
        {
            throw new JsRegExpSyntaxException(error.Message);
        }
    }

    public EcmaRegExpMatch? Find(string input, int start, bool sticky)
    {
        try
        {
            var result = _engine.Execute(
                input,
                start,
                ExecutionBudget(input.Length));
            if (!result.Success || sticky && result.Index != start)
            {
                return null;
            }
            return new EcmaRegExpMatch(result);
        }
        catch (QuickJs.RegExpResourceLimitException error)
        {
            throw new RangeError(error.Message);
        }
    }

    private static long ExecutionBudget(int inputLength)
    {
        var scaled = checked((long)inputLength * StepsPerInputCodeUnit);
        return System.Math.Clamp(
            scaled,
            MinimumExecutionSteps,
            MaximumExecutionSteps);
    }

    private static QuickJs.RegExpFlags ToEngineFlags(RegExpFlags flags)
    {
        var result = QuickJs.RegExpFlags.None;
        if (flags.Global) result |= QuickJs.RegExpFlags.Global;
        if (flags.IgnoreCase) result |= QuickJs.RegExpFlags.IgnoreCase;
        if (flags.Multiline) result |= QuickJs.RegExpFlags.Multiline;
        if (flags.DotAll) result |= QuickJs.RegExpFlags.DotAll;
        if (flags.Unicode) result |= QuickJs.RegExpFlags.Unicode;
        if (flags.Sticky) result |= QuickJs.RegExpFlags.Sticky;
        if (flags.HasIndices) result |= QuickJs.RegExpFlags.Indices;
        if (flags.UnicodeSets) result |= QuickJs.RegExpFlags.UnicodeSets;
        return result;
    }
}

internal sealed class EcmaRegExpMatch
{
    public EcmaRegExpMatch(QuickJs.RegExpMatchResult result)
    {
        Start = result.Index;
        End = checked(result.Index + result.Length);
        var groups = result.Groups ?? [];
        CaptureStarts = new int[groups.Length];
        CaptureEnds = new int[groups.Length];
        for (var index = 0; index < groups.Length; index += 1)
        {
            var group = groups[index];
            CaptureStarts[index] = group.Success ? group.Index : -1;
            CaptureEnds[index] = group.Success
                ? checked(group.Index + group.Length)
                : -1;
        }
    }

    public int Start { get; }
    public int End { get; }
    public int[] CaptureStarts { get; }
    public int[] CaptureEnds { get; }
}

internal static class EcmaRegExpProgramCache
{
    private const int Capacity = 256;
    private const int MaximumSourceCodeUnits = 4_194_304;
    private const string EngineVersion =
        "quickjs-libregexp-jint-9817530a-unicode17-budget-v1";
    private static readonly object Gate = new();
    private static readonly Dictionary<
        ProgramKey,
        LinkedListNode<CacheEntry>
    > Entries = new();
    private static readonly LinkedList<CacheEntry> Recency = new();
    private static int RetainedSourceCodeUnits;

    public static EcmaRegExpProgram GetOrCompile(
        string pattern,
        RegExpFlags flags)
    {
        var key = new ProgramKey(
            pattern,
            flags.CanonicalFlags,
            EngineVersion);
        lock (Gate)
        {
            if (Entries.TryGetValue(key, out var existing))
            {
                Recency.Remove(existing);
                Recency.AddFirst(existing);
                return existing.Value.Program;
            }
        }

        var compiled = EcmaRegExpProgram.Compile(pattern, flags);
        lock (Gate)
        {
            if (Entries.TryGetValue(key, out var raced))
            {
                Recency.Remove(raced);
                Recency.AddFirst(raced);
                return raced.Value.Program;
            }
            var sourceCodeUnits = checked(
                key.Pattern.Length + key.Flags.Length);
            var node = Recency.AddFirst(new CacheEntry(
                key,
                compiled,
                sourceCodeUnits));
            Entries.Add(key, node);
            RetainedSourceCodeUnits = checked(
                RetainedSourceCodeUnits + sourceCodeUnits);
            while (Entries.Count > Capacity ||
                RetainedSourceCodeUnits > MaximumSourceCodeUnits)
            {
                var last = Recency.Last!;
                Recency.RemoveLast();
                Entries.Remove(last.Value.Key);
                RetainedSourceCodeUnits = checked(
                    RetainedSourceCodeUnits -
                    last.Value.SourceCodeUnits);
            }
        }
        return compiled;
    }

    private readonly record struct ProgramKey(
        string Pattern,
        string Flags,
        string EngineVersion);

    private sealed record CacheEntry(
        ProgramKey Key,
        EcmaRegExpProgram Program,
        int SourceCodeUnits);
}
