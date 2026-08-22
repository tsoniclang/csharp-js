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
            Assert.Equal(5, value.ReadDynamicSlot("length").unwrap());
            Assert.Equal("string", TsValue.ApplyDynamicTypeof(value));
        }

        [Fact]
        public void RuntimeUnionBoxing_PreservesUndefinedArmThroughDynamicOperators()
        {
            var value = TsValue.from(TsUnion.From(1, 2, Undefined.value));

            var union = Assert.IsType<TsUnion>(value.unwrap());

            Assert.Same(Undefined.value, union.value().unwrap());
            Assert.Equal(
                "fallback",
                TsValue.ApplyDynamicLogical(value, "??", () => "fallback").unwrap());
            Assert.Equal("undefined", TsValue.ApplyDynamicTypeof(value));
        }

        [Fact]
        public void RuntimeUnionBoxing_RejectsOpenClrObjectArm()
        {
            Assert.Throws<NotSupportedException>(() =>
                TsValue.from(Union<int, OpenClrObject>.From2(new OpenClrObject()))
            );
        }

        [Fact]
        public void CastDynamic_ConvertsClosedTsUnionCarrierToRuntimeUnion()
        {
            var value = TsValue.from(TsUnion.From(2, 2, "ready"));

            var union = TsUnion.CastDynamic<int, string>(value);

            Assert.True(union.Is2());
            Assert.Equal("ready", union.As2());
        }

        [Fact]
        public void CastDynamic_PreservesClosedHigherArityUnionArm()
        {
            var value = TsValue.from(TsUnion.From(8, 8, "last"));

            var union = TsUnion.CastDynamic<int, bool, double, string, byte, short, long, string>(value);

            Assert.True(union.Is8());
            Assert.Equal("last", union.As8());
        }

        [Fact]
        public void CastDynamic_ConvertsRawClosedArmValueToRuntimeUnion()
        {
            var union = TsUnion.CastDynamic<int, string>(42);

            Assert.True(union.Is1());
            Assert.Equal(42, union.As1());
        }

        [Fact]
        public void CastDynamic_RejectsMismatchedClosedUnionArmCount()
        {
            var value = TsValue.from(TsUnion.From(3, 3, "extra"));

            Assert.Throws<TypeError>(() => TsUnion.CastDynamic<int, string>(value));
        }

        [Fact]
        public void CastDynamic_RejectsMismatchedClosedUnionArmCarrier()
        {
            var value = TsValue.from(TsUnion.From(2, 2, 42));

            Assert.Throws<TypeError>(() => TsUnion.CastDynamic<bool, string>(value));
        }

        [Fact]
        public void AsArm_RejectsInactiveArmProjection()
        {
            var value = TsUnion.From(1, 2, 42);

            Assert.Throws<TypeError>(() => value.asArm(2));
        }

        [Fact]
        public void DynamicOperators_RejectOpenClrObjectsBeforeFallback()
        {
            var value = new OpenClrObject();

            Assert.Throws<NotSupportedException>(() => TsValue.ApplyDynamicTypeof(value));
            Assert.Throws<NotSupportedException>(() => TsValue.ApplyDynamicBinary(value, "+", 1));
            Assert.Throws<NotSupportedException>(() => TsValue.ApplyDynamicBinaryBoolean(value, "===", value));
            Assert.Throws<NotSupportedException>(() => TsValue.ApplyDynamicUnaryBoolean(value, "!"));
        }

        private sealed class OpenClrObject
        {
        }
    }
}
