using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Tsonic.CSharp.TestTools.ForbiddenSyntax;
using Xunit;

namespace Tsonic.CSharp.Js.Tests
{
    public class NoReflectionSemanticsTests
    {
        [Fact]
        public void JsRuntimeSources_DoNotUseReflectionOrDynamicSemantics()
        {
            var repositoryRoot = FindRepositoryRoot();
            var csharpRuntimeRoot = FindSiblingRepositoryRoot(
                repositoryRoot,
                "csharp-runtime",
                Path.Combine("src", "Tsonic.CSharp.Runtime")
            );
            var sourceRoots = new[]
            {
                Path.Combine(repositoryRoot, "src", "Tsonic.CSharp.Js"),
                Path.Combine(csharpRuntimeRoot, "src"),
            };
            foreach (var sourceRoot in sourceRoots)
            {
                Assert.True(Directory.Exists(sourceRoot), $"Missing runtime source root: {sourceRoot}");
                foreach (var sourceFile in Directory.EnumerateFiles(sourceRoot, "*.cs", SearchOption.AllDirectories))
                {
                    var text = File.ReadAllText(sourceFile);
                    var violations = FindForbiddenSyntax(text);
                    Assert.True(violations.Count == 0,
                        $"{sourceFile} contains forbidden runtime syntax: {string.Join(", ", violations)}");
                }
            }
        }

        [Theory]
        [InlineData("class Value { dynamic field; }", "dynamic")]
        [InlineData("class Value { @dynamic field; }", "dynamic")]
        [InlineData("class Value { \\u0064ynamic field; }", "dynamic")]
        [InlineData("using System /* retained */ . Reflection;", "System.Reflection")]
        [InlineData("var result = target.GetProperty(name);", "GetProperty")]
        [InlineData("var result = target.GetProperties();", "GetProperties")]
        [InlineData("var result = target.GetMethod(name);", "GetMethod")]
        [InlineData("var result = target.GetMethods();", "GetMethods")]
        [InlineData("MethodInfo /* retained */ . Invoke(target, values);", "MethodInfo.Invoke")]
        [InlineData("target.MakeGenericMethod(arguments);", "MakeGenericMethod")]
        [InlineData("Activator.CreateInstance(type);", "Activator.CreateInstance")]
        [InlineData("Assembly.Load(path);", "Assembly.Load")]
        [InlineData("var text = $\"{((dynamic)value).field}\";", "dynamic")]
        [InlineData("#if NEVER_SELECTED\nclass Value { dynamic field; }\n#endif", "dynamic")]
        [InlineData("#if NEVER_SELECTED\n#if ALSO_NOT_SELECTED\nAssembly.Load(path);\n#endif\n#endif", "Assembly.Load")]
        public void SourceGuardRejectsEveryForbiddenOperation(string source, string expected)
        {
            Assert.Contains(expected, FindForbiddenSyntax(source));
        }

        [Theory]
        [InlineData("throw new NotSupportedException(\"does not expose dynamic properties\");")]
        [InlineData("// dynamic System.Reflection\n/* Activator.CreateInstance */\nclass Value {}")]
        [InlineData("var text = @\"System.Reflection and GetProperty\";")]
        [InlineData("var text = \"\"\"dynamic Assembly.Load\"\"\";")]
        [InlineData("var text = $\"dynamic {value} GetMethod\";")]
        [InlineData("var text = $$\"\"\"System.Reflection {{value}} dynamic\"\"\";")]
        [InlineData("#if NEVER_SELECTED\nvar text = \"dynamic\";\n#endif")]
        public void SourceGuardDoesNotTreatProseAsExecutableSyntax(string source)
        {
            Assert.Empty(FindForbiddenSyntax(source));
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

        private static string FindSiblingRepositoryRoot(
            string startDirectory,
            string repositoryName,
            string requiredRelativePath
        )
        {
            var directory = new DirectoryInfo(startDirectory);
            while (directory != null)
            {
                var candidate = Path.Combine(directory.FullName, repositoryName);
                if (Directory.Exists(Path.Combine(candidate, requiredRelativePath)))
                {
                    return candidate;
                }
                directory = directory.Parent;
            }
            throw new InvalidOperationException($"Could not locate {repositoryName} repository root.");
        }
    }
}
