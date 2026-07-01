using System.Collections.Generic;
using Xunit;

namespace Tsonic.CSharp.Js.Tests
{
    public class TsValueTests
    {
        [Fact]
        public void ReadCompatSlot_ReturnsUndefinedForMissingClosedObjectProperty()
        {
            var value = TsValue.from(new TsObject());

            var missing = value.ReadCompatSlot("missing");

            Assert.Same(JSUndefined.value, missing.unwrap());
        }

        [Fact]
        public void ReadCompatSlot_AndWriteCompatSlot_UseClosedObjectProperties()
        {
            var value = TsValue.from(new TsObject());

            var written = value.WriteCompatSlot("name", "tsonic");
            var read = value.ReadCompatSlot("name");

            Assert.Equal("tsonic", written.unwrap());
            Assert.Equal("tsonic", read.unwrap());
        }

        [Fact]
        public void ElementAccess_UsesClosedPropertyKeyConversion()
        {
            var value = TsValue.from(new TsObject());

            value.WriteCompatElement(42, "answer");

            Assert.Equal("answer", value.ReadCompatElement("42").unwrap());
        }

        [Fact]
        public void ArrayElementAccess_ExtendsWithUndefinedSlots()
        {
            var value = TsValue.from(new TsArray());

            value.WriteCompatElement(2, "third");

            Assert.Equal(3, value.ReadCompatSlot("length").unwrap());
            Assert.Same(JSUndefined.value, value.ReadCompatElement(0).unwrap());
            Assert.Equal("third", value.ReadCompatElement(2).unwrap());
        }

        [Fact]
        public void InvokeCompat_UsesClosedFunctionCarrier()
        {
            var function = TsValue.from(new TsFunction(args => TsValue.from((int)args[0].unwrap()! + 1)));

            var result = function.InvokeCompat(41);

            Assert.Equal(42, result.unwrap());
        }

        [Fact]
        public void ReadCompatSlot_ThenInvokeCompat_UsesClosedObjectFunctionProperty()
        {
            var target = TsValue.from(new TsObject(new Dictionary<string, object?>
            {
                ["create"] = new TsFunction(args => TsValue.from("created:" + (string)args[0].unwrap()!))
            }));

            var result = target.ReadCompatSlot("create").InvokeCompat("Ada");

            Assert.Equal("created:Ada", result.unwrap());
        }

        [Fact]
        public void ConstructCompat_UsesClosedConstructorCarrier()
        {
            var function = TsValue.from(new TsFunction(
                args => TsValue.from(args.Count),
                args => TsValue.from(new TsObject(new Dictionary<string, object?>
                {
                    ["count"] = args.Count
                }))));

            var result = Assert.IsType<TsObject>(function.ConstructCompat(1, 2).unwrap());

            Assert.Equal(2, result.ReadCompatSlot("count").unwrap());
        }

        [Fact]
        public void MissingMethodCall_ThrowsTypeErrorWithoutReflection()
        {
            var target = TsValue.from(new TsObject());

            var missing = target.ReadCompatSlot("missing");

            Assert.Throws<TypeError>(() => missing.InvokeCompat());
        }

        [Fact]
        public void UnsupportedClrObject_IsRejectedBeforeCarrierCreation()
        {
            Assert.Throws<System.NotSupportedException>(() => TsValue.from(new { name = "not-closed" }));
        }

        [Fact]
        public void ApplyCompatBinary_UsesClosedPrimitiveOperatorSemantics()
        {
            Assert.Equal(3d, TsValue.ApplyCompatBinary(1, "+", 2).unwrap());
            Assert.Equal("hello2", TsValue.ApplyCompatBinary("hello", "+", 2).unwrap());
            Assert.Equal(3d, TsValue.ApplyCompatBinary("5", "-", 2).unwrap());
            Assert.Equal("fallback", TsValue.ApplyCompatBinary(JSUndefined.value, "??", "fallback").unwrap());
            Assert.Equal("right", TsValue.ApplyCompatBinary(true, "&&", "right").unwrap());
            Assert.Equal("left", TsValue.ApplyCompatBinary("left", "||", "right").unwrap());
        }

        [Fact]
        public void ApplyCompatBinaryBoolean_UsesClosedPrimitiveComparisonSemantics()
        {
            Assert.True(TsValue.ApplyCompatBinaryBoolean("2", "==", 2));
            Assert.False(TsValue.ApplyCompatBinaryBoolean("2", "===", 2));
            Assert.True(TsValue.ApplyCompatBinaryBoolean(2, "===", 2d));
            Assert.True(TsValue.ApplyCompatBinaryBoolean("a", "<", "b"));
            Assert.True(TsValue.ApplyCompatBinaryBoolean(4, ">=", "4"));
        }

        [Fact]
        public void ApplyCompatUnary_UsesClosedPrimitiveOperatorSemantics()
        {
            Assert.Equal(7d, TsValue.ApplyCompatUnary("7", "+").unwrap());
            Assert.Equal(-7d, TsValue.ApplyCompatUnary("7", "-").unwrap());
            Assert.Equal(~7, TsValue.ApplyCompatUnary(7, "~").unwrap());
            Assert.True(TsValue.ApplyCompatUnaryBoolean(0, "!"));
            Assert.False(TsValue.ApplyCompatUnaryBoolean("value", "!"));
        }

        [Fact]
        public void ApplyCompatOperator_RejectsUnsupportedOperatorsDeterministically()
        {
            Assert.Throws<System.NotSupportedException>(() => TsValue.ApplyCompatBinary(1, "<<", 2));
            Assert.Throws<System.NotSupportedException>(() => TsValue.ApplyCompatUnary(1, "delete"));
        }

        [Fact]
        public void CastCompat_ReturnsClosedTypedCarrierValue()
        {
            Assert.Equal(42, TsValue.CastCompat<int>(TsValue.from(42)));
            Assert.Equal("Ada", TsValue.CastCompat<string>(TsValue.from("Ada")));
        }

        [Fact]
        public void CastCompat_AllowsNullishOnlyForNullableOrReferenceTargets()
        {
            Assert.Null(TsValue.CastCompat<string>(TsValue.undefined()));
            Assert.Throws<TypeError>(() => TsValue.CastCompat<int>(TsValue.undefined()));
        }

        [Fact]
        public void CastCompat_RejectsMismatchedClosedCarrierDeterministically()
        {
            Assert.Throws<TypeError>(() => TsValue.CastCompat<int>(TsValue.from("42")));
        }
    }
}
