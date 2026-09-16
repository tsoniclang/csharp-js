using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Tsonic.CSharp.TestTools;

public static class ForbiddenSyntax
{
    public static IReadOnlyCollection<string> FindForbiddenSyntax(string source)
    {
        string[][] bannedSequences =
        [
            ["dynamic"],
            ["System", ".", "Reflection"],
            ["GetProperty"],
            ["GetProperties"],
            ["GetMethod"],
            ["GetMethods"],
            ["MethodInfo", ".", "Invoke"],
            ["MakeGenericMethod"],
            ["Activator", ".", "CreateInstance"],
            ["Assembly", ".", "Load"],
        ];
        var violations = new SortedSet<string>(StringComparer.Ordinal);
        var pending = new Queue<string>();
        pending.Enqueue(source);
        while (pending.TryDequeue(out var fragment))
        {
            var root = CSharpSyntaxTree.ParseText(fragment).GetRoot();
            var tokens = root.DescendantTokens().Select(token =>
                token.IsKind(SyntaxKind.IdentifierToken) || token.IsKind(SyntaxKind.DotToken)
                    ? token.ValueText : string.Empty).ToArray();
            foreach (var sequence in bannedSequences)
            {
                for (var start = 0; start <= tokens.Length - sequence.Length; start++)
                {
                    if (tokens.AsSpan(start, sequence.Length).SequenceEqual(sequence))
                        violations.Add(string.Concat(sequence));
                }
            }
            foreach (var trivia in root.DescendantTrivia(descendIntoTrivia: true))
            {
                if (!trivia.IsKind(SyntaxKind.DisabledTextTrivia)) continue;
                var disabled = trivia.ToFullString();
                if (disabled.Length >= fragment.Length)
                    throw new InvalidOperationException("Disabled source must be a strict fragment of its containing syntax.");
                pending.Enqueue(disabled);
            }
        }
        return violations;
    }
}
