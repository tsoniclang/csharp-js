using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Tsonic.CSharp.Js.Tests;

[Collection("JavaScript event loop")]
public sealed class LoopCostTests(ITestOutputHelper output)
{
    [Fact]
    public async Task ColdAndWarmCostsExactlyMatchAnIndependentDeferredNativeControl()
    {
        var assembly = typeof(LoopCostTests).Assembly.Location;
        var start = new ProcessStartInfo("dotnet")
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        start.ArgumentList.Add("exec");
        start.ArgumentList.Add("--runtimeconfig");
        start.ArgumentList.Add(Path.ChangeExtension(assembly, ".runtimeconfig.json"));
        start.ArgumentList.Add("--depsfile");
        start.ArgumentList.Add(Path.ChangeExtension(assembly, ".deps.json"));
        start.ArgumentList.Add(typeof(CostProbe.Program).Assembly.Location);
        using var process = Process.Start(start) ?? throw new InvalidOperationException("The isolated allocation probe did not start.");
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        try { await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(120)); }
        finally { if (!process.HasExited) process.Kill(entireProcessTree: true); }
        var measured = await stdout;
        var errors = await stderr;
        output.WriteLine(measured);
        Assert.True(process.ExitCode == 0, "Independent all-thread allocation control failed: " + measured + errors);
    }
}
