using System;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace Tsonic.CSharp.Js.Tests
{
    public class NoReflectionSemanticsTests
    {
        [Fact]
        public void JsRuntimeSources_DoNotUseReflectionOrDynamicSemantics()
        {
            var repositoryRoot = FindRepositoryRoot();
            var sourceRoots = new[]
            {
                Path.Combine(repositoryRoot, "src", "Tsonic.CSharp.Js"),
                Path.GetFullPath(Path.Combine(repositoryRoot, "..", "csharp-runtime", "src")),
            };
            var bannedPatterns = new[]
            {
                new Regex(@"\bdynamic\b"),
                new Regex(@"System\.Reflection"),
                new Regex(@"\bGetProperty\b"),
                new Regex(@"\bGetProperties\b"),
                new Regex(@"\bGetMethod\b"),
                new Regex(@"\bGetMethods\b"),
                new Regex(@"\bMethodInfo\.Invoke\b"),
                new Regex(@"\bMakeGenericMethod\b"),
                new Regex(@"\bActivator\.CreateInstance\b"),
                new Regex(@"\bAssembly\.Load\b"),
            };

            foreach (var sourceRoot in sourceRoots)
            {
                Assert.True(Directory.Exists(sourceRoot), $"Missing runtime source root: {sourceRoot}");
                foreach (var sourceFile in Directory.EnumerateFiles(sourceRoot, "*.cs", SearchOption.AllDirectories))
                {
                    var text = File.ReadAllText(sourceFile);
                    foreach (var pattern in bannedPatterns)
                    {
                        Assert.DoesNotMatch(pattern, text);
                    }
                }
            }
        }

        private static string FindRepositoryRoot()
        {
            var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (directory != null)
            {
                if (Directory.Exists(Path.Combine(directory.FullName, "src", "Tsonic.CSharp.Js")))
                {
                    return directory.FullName;
                }
                directory = directory.Parent;
            }
            throw new InvalidOperationException("Could not locate csharp-js repository root.");
        }
    }
}
