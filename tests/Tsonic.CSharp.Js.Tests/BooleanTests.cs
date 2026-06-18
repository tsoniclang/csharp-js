using Xunit;

namespace Tsonic.CSharp.Js.Tests
{
    public class BooleanTests
    {
        [Fact]
        public void toString_UsesJavaScriptLowercaseCasing()
        {
            Assert.Equal("true", true.toString());
            Assert.Equal("false", false.toString());
        }

        [Fact]
        public void valueOf_ReturnsUnderlyingBooleanValue()
        {
            Assert.True(true.valueOf());
            Assert.False(false.valueOf());
        }
    }
}
