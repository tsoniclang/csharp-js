using System;
using Tsonic.CSharp.Runtime;
using Xunit;

namespace Tsonic.CSharp.Js.Tests
{
    public class TsUnionTests
    {
        [Fact]
        public void RuntimeUnionBoxing_UsesClosedTsUnionCarrierAndActiveArmOperations()
        {
            var runtimeUnion = Union<int, string>.From2("ready");

            var value = TsValue.from(runtimeUnion);

            var union = Assert.IsType<TsUnion>(value.unwrap());
            Assert.Equal(2, union.ArmIndex);
            Assert.Equal(2, union.ArmCount);
            Assert.Equal("ready", union.value().unwrap());
            Assert.Equal(5, value.ReadCompatSlot("length").unwrap());
            Assert.Equal("string", TsValue.ApplyCompatTypeof(value));
        }

        [Fact]
        public void RuntimeUnionBoxing_PreservesUndefinedArmThroughCompatOperators()
        {
            var value = TsValue.from(TsUnion.From(1, 2, JSUndefined.value));

            var union = Assert.IsType<TsUnion>(value.unwrap());

            Assert.Same(JSUndefined.value, union.value().unwrap());
            Assert.Equal("fallback", TsValue.ApplyCompatBinary(value, "??", "fallback").unwrap());
            Assert.Equal("undefined", TsValue.ApplyCompatTypeof(value));
        }

        [Fact]
        public void RuntimeUnionBoxing_RejectsOpenClrObjectArm()
        {
            Assert.Throws<NotSupportedException>(() =>
                TsValue.from(Union<int, OpenClrObject>.From2(new OpenClrObject()))
            );
        }

        [Fact]
        public void CastCompat_ConvertsClosedTsUnionCarrierToRuntimeUnion()
        {
            var value = TsValue.from(TsUnion.From(2, 2, "ready"));

            var union = TsUnion.CastCompat<int, string>(value);

            Assert.True(union.Is2());
            Assert.Equal("ready", union.As2());
        }

        [Fact]
        public void CastCompat_ConvertsRawClosedArmValueToRuntimeUnion()
        {
            var union = TsUnion.CastCompat<int, string>(42);

            Assert.True(union.Is1());
            Assert.Equal(42, union.As1());
        }

        [Fact]
        public void CompatOperators_RejectOpenClrObjectsBeforeFallback()
        {
            var value = new OpenClrObject();

            Assert.Throws<NotSupportedException>(() => TsValue.ApplyCompatTypeof(value));
            Assert.Throws<NotSupportedException>(() => TsValue.ApplyCompatBinary(value, "+", 1));
            Assert.Throws<NotSupportedException>(() => TsValue.ApplyCompatBinaryBoolean(value, "===", value));
            Assert.Throws<NotSupportedException>(() => TsValue.ApplyCompatUnaryBoolean(value, "!"));
        }

        private sealed class OpenClrObject
        {
        }
    }
}
