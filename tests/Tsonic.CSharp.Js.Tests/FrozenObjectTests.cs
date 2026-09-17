using Xunit;

namespace Tsonic.CSharp.Js.Tests;

public class FrozenObjectTests
{
    [Fact]
    public void FreezeRetainsReferenceIdentityWithoutFreezingNestedObjects()
    {
        var nested = new object();
        var value = new Box(nested);
        var alias = value;
        Assert.Same(value, FrozenObject.Freeze(value));
        Assert.Same(value, FrozenObject.Freeze(value));
        Assert.True(FrozenObject.IsFrozen(alias));
        Assert.False(FrozenObject.IsFrozen(nested));
        Assert.False(FrozenObject.IsFrozen(new Box(nested)));
        Assert.Throws<TypeError>(() => FrozenObject.CheckWrite(alias));
        FrozenObject.CheckWrite(nested);
    }

    private sealed record Box(object Child);
}
