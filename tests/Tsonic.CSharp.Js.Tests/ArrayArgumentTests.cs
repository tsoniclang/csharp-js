using Xunit;

namespace Tsonic.CSharp.Js.Tests
{
    public partial class ArrayTests
    {
        [Fact]
        public void NumericArguments_PreserveInitializedAndSnapshotValues()
        {
            var source = new JSArray<byte>(3);
            source[0] = 65;
            source[2] = 255;
            var conversions = 0;
            var snapshot = Array.snapshotNumberArguments(source, value =>
            {
                conversions++;
                return value;
            });
            source[0] = 99;
            Assert.Equal(3, snapshot.Length);
            Assert.Equal(65, snapshot[0]);
            Assert.Equal(0, snapshot[1]);
            Assert.Equal(255, snapshot[2]);
            Assert.Equal(3, conversions);
            Assert.Empty(Array.snapshotNumberArguments(new JSArray<double>(), value => value));
        }

        [Fact]
        public void NumericArguments_DenseArraysRetainExactNumericValues()
        {
            var source = new[] { -0.0, double.NaN, double.PositiveInfinity, 1.5 };
            var snapshot = Array.snapshotNumberArguments(source, value => value);
            source[3] = 4;
            Assert.Equal(System.BitConverter.DoubleToInt64Bits(-0.0), System.BitConverter.DoubleToInt64Bits(snapshot[0]));
            Assert.True(double.IsNaN(snapshot[1]));
            Assert.Equal(double.PositiveInfinity, snapshot[2]);
            Assert.Equal(1.5, snapshot[3]);
            Assert.Empty(Array.snapshotNumberArguments(System.Array.Empty<byte>(), value => value));
        }
    }
}
