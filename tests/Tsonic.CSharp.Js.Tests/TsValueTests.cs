using System;
using System.Collections.Generic;
using Xunit;

namespace Tsonic.CSharp.Js.Tests
{
    public class TsValueTests
    {
        [Fact]
        public void CreateDynamicObject_BuildsOneClosedMutableCarrier()
        {
            var value = TsValue.CreateDynamicObject(
                "title", "first",
                "completed", false,
                "title", "last");

            Assert.Equal("last", value.ReadDynamicSlot("title").unwrap());
            Assert.Equal(false, value.ReadDynamicSlot("completed").unwrap());

            value.WriteDynamicSlot("title", "updated");
            Assert.Equal("updated", value.ReadDynamicSlot("title").unwrap());
        }

        [Fact]
        public void CreateDynamicObject_RejectsMalformedCompilerInput()
        {
            Assert.Throws<ArgumentException>(() =>
                TsValue.CreateDynamicObject("title"));
            Assert.Throws<ArgumentException>(() =>
                TsValue.CreateDynamicObject(1, "value"));
        }

        [Fact]
        public void ReadDynamicSlot_ReturnsUndefinedForMissingClosedObjectProperty()
        {
            var value = TsValue.from(new TsObject());

            var missing = value.ReadDynamicSlot("missing");

            Assert.Same(JSUndefined.value, missing.unwrap());
        }

        [Fact]
        public void ReadDynamicSlot_AndWriteDynamicSlot_UseClosedObjectProperties()
        {
            var value = TsValue.from(new TsObject());

            var written = value.WriteDynamicSlot("name", "tsonic");
            var read = value.ReadDynamicSlot("name");

            Assert.Equal("tsonic", written.unwrap());
            Assert.Equal("tsonic", read.unwrap());
        }

        [Fact]
        public void ReadDynamicSlot_PreservesPresentNullUndefinedAndMissingUndefined()
        {
            var value = TsValue.from(new TsObject());

            value.WriteDynamicSlot("nullValue", null);
            value.WriteDynamicSlot("undefinedValue", TsValue.undefined());

            Assert.Null(value.ReadDynamicSlot("nullValue").unwrap());
            Assert.Same(JSUndefined.value, value.ReadDynamicSlot("undefinedValue").unwrap());
            Assert.Same(JSUndefined.value, value.ReadDynamicSlot("missing").unwrap());
        }

        [Fact]
        public void OptionalPropertyRead_SkipsNullishReceivers()
        {
            Assert.Same(
                JSUndefined.value,
                TsValue.undefined().ReadDynamicSlotOptional("name").unwrap());
            Assert.Equal(
                "Ada",
                TsValue.from(new TsObject(new Dictionary<string, object?>
                {
                    ["name"] = "Ada"
                })).ReadDynamicSlotOptional("name").unwrap());
        }

        [Fact]
        public void ElementAccess_UsesClosedPropertyKeyConversion()
        {
            var value = TsValue.from(new TsObject());

            value.WriteDynamicElement(42, "answer");

            Assert.Equal("answer", value.ReadDynamicElement("42").unwrap());
        }

        [Fact]
        public void ElementAccess_UsesDistinctNullAndUndefinedPropertyKeys()
        {
            var value = TsValue.from(new TsObject());

            value.WriteDynamicElement(null, "null-key");
            value.WriteDynamicElement(JSUndefined.value, "undefined-key");

            Assert.Equal("null-key", value.ReadDynamicSlot("null").unwrap());
            Assert.Equal("undefined-key", value.ReadDynamicSlot("undefined").unwrap());
        }

        [Fact]
        public void OptionalElementRead_DoesNotEvaluateTheKeyForANullishReceiver()
        {
            var evaluated = false;

            var result = TsValue.undefined().ReadDynamicElementOptional(() =>
            {
                evaluated = true;
                return "name";
            });

            Assert.False(evaluated);
            Assert.Same(JSUndefined.value, result.unwrap());
        }

        [Fact]
        public void ArrayElementAccess_ExtendsWithUndefinedSlots()
        {
            var value = TsValue.from(new TsArray());

            value.WriteDynamicElement(2, "third");

            Assert.Equal(3, value.ReadDynamicSlot("length").unwrap());
            Assert.Same(JSUndefined.value, value.ReadDynamicElement(0).unwrap());
            Assert.Equal("third", value.ReadDynamicElement(2).unwrap());
        }

        [Fact]
        public void JsArrayElementAccess_UsesClosedSparseArrayCarrier()
        {
            var array = new JSArray<object?>();
            array.setLength(3);
            array[1] = "middle";
            var value = TsValue.from(array);

            Assert.Equal(3, value.ReadDynamicSlot("length").unwrap());
            Assert.Same(JSUndefined.value, value.ReadDynamicElement(0).unwrap());
            Assert.Equal("middle", value.ReadDynamicElement(1).unwrap());

            value.WriteDynamicElement(2, "last");
            Assert.Equal("last", array[2]);

            value.WriteDynamicSlot("length", 1);
            Assert.Equal(1, array.length);
            Assert.Same(JSUndefined.value, value.ReadDynamicElement(2).unwrap());
        }

        [Fact]
        public void JsArrayElementWrite_RejectsIncompatibleClosedElementCarrier()
        {
            var value = TsValue.from(new JSArray<int>());

            Assert.Throws<TypeError>(() => value.WriteDynamicElement(0, "not-an-int"));
        }

        [Fact]
        public void InvokeDynamic_UsesClosedFunctionCarrier()
        {
            var function = TsValue.from(new TsFunction(args => TsValue.from((int)args[0].unwrap()! + 1)));

            var result = function.InvokeDynamic(41);

            Assert.Equal(42, result.unwrap());
        }

        [Fact]
        public void ReadDynamicSlot_ThenInvokeDynamic_UsesClosedObjectFunctionProperty()
        {
            var target = TsValue.from(new TsObject(new Dictionary<string, object?>
            {
                ["create"] = new TsFunction(args => TsValue.from("created:" + (string)args[0].unwrap()!))
            }));

            var result = target.ReadDynamicSlot("create").InvokeDynamic("Ada");

            Assert.Equal("created:Ada", result.unwrap());
        }

        [Fact]
        public void MemberInvocation_PreservesReceiverAndEvaluationOrder()
        {
            var events = new List<string>();
            var target = TsValue.from(new TsObject());
            target.WriteDynamicSlot("name", "Ada");
            target.WriteDynamicSlot("greet", new TsFunction((receiver, arguments) =>
            {
                events.Add("call");
                return TsValue.from(
                    (string)receiver.ReadDynamicSlot("name").unwrap()! +
                    (string)arguments[0].unwrap()!);
            }));

            var result = target.InvokeDynamicSlot(
                "greet",
                optionalReceiver: false,
                optionalCall: false,
                () =>
                {
                    events.Add("argument");
                    return new object?[] { "!" };
                });

            Assert.Equal("Ada!", result.unwrap());
            Assert.Equal(new[] { "argument", "call" }, events);
        }

        [Fact]
        public void OptionalMemberInvocation_SkipsKeysAndArgumentsAtTheExactBoundary()
        {
            var keyEvaluated = false;
            var argumentsEvaluated = false;

            var receiverResult = TsValue.undefined().InvokeDynamicElement(
                () =>
                {
                    keyEvaluated = true;
                    return "run";
                },
                optionalReceiver: true,
                optionalCall: false,
                () =>
                {
                    argumentsEvaluated = true;
                    return System.Array.Empty<object?>();
                });

            Assert.Same(JSUndefined.value, receiverResult.unwrap());
            Assert.False(keyEvaluated);
            Assert.False(argumentsEvaluated);

            var target = TsValue.from(new TsObject());
            var callResult = target.InvokeDynamicSlot(
                "missing",
                optionalReceiver: false,
                optionalCall: true,
                () =>
                {
                    argumentsEvaluated = true;
                    return System.Array.Empty<object?>();
                });

            Assert.Same(JSUndefined.value, callResult.unwrap());
            Assert.False(argumentsEvaluated);
        }

        [Fact]
        public void OptionalDirectInvocation_SkipsArgumentsForANullishCallee()
        {
            var evaluated = false;

            var result = TsValue.undefined().InvokeDynamicOptional(() =>
            {
                evaluated = true;
                return System.Array.Empty<object?>();
            });

            Assert.Same(JSUndefined.value, result.unwrap());
            Assert.False(evaluated);
        }

        [Fact]
        public void ConstructDynamic_UsesClosedConstructorCarrier()
        {
            var function = TsValue.from(new TsFunction(
                args => TsValue.from(args.Count),
                args => TsValue.from(new TsObject(new Dictionary<string, object?>
                {
                    ["count"] = args.Count
                }))));

            var result = Assert.IsType<TsObject>(function.ConstructDynamic(1, 2).unwrap());

            Assert.Equal(2, result.ReadDynamicSlot("count").unwrap());
        }

        [Fact]
        public void MissingMethodCall_ThrowsTypeErrorWithoutReflection()
        {
            var target = TsValue.from(new TsObject());

            var missing = target.ReadDynamicSlot("missing");

            Assert.Throws<TypeError>(() => missing.InvokeDynamic());
        }

        [Fact]
        public void UnsupportedClrObject_IsRejectedBeforeCarrierCreation()
        {
            Assert.Throws<System.NotSupportedException>(() => TsValue.from(new { name = "not-closed" }));
        }

        [Fact]
        public void ApplyDynamicBinary_UsesClosedPrimitiveOperatorSemantics()
        {
            Assert.Equal(3d, TsValue.ApplyDynamicBinary(1, "+", 2).unwrap());
            Assert.Equal("hello2", TsValue.ApplyDynamicBinary("hello", "+", 2).unwrap());
            Assert.Equal(3d, TsValue.ApplyDynamicBinary("5", "-", 2).unwrap());
        }

        [Fact]
        public void ApplyDynamicLogical_PreservesShortCircuitEvaluation()
        {
            var evaluations = 0;
            object? right()
            {
                evaluations++;
                return "right";
            }

            Assert.Equal(
                "fallback",
                TsValue.ApplyDynamicLogical(JSUndefined.value, "??", () => "fallback").unwrap());
            Assert.Equal("right", TsValue.ApplyDynamicLogical(true, "&&", right).unwrap());
            Assert.Equal("left", TsValue.ApplyDynamicLogical("left", "||", right).unwrap());
            Assert.Equal(1, evaluations);
        }

        [Fact]
        public void ApplyDynamicBinaryBoolean_UsesClosedPrimitiveComparisonSemantics()
        {
            Assert.True(TsValue.ApplyDynamicBinaryBoolean("2", "==", 2));
            Assert.False(TsValue.ApplyDynamicBinaryBoolean("2", "===", 2));
            Assert.True(TsValue.ApplyDynamicBinaryBoolean(2, "===", 2d));
            Assert.True(TsValue.ApplyDynamicBinaryBoolean("a", "<", "b"));
            Assert.True(TsValue.ApplyDynamicBinaryBoolean(4, ">=", "4"));
        }

        [Fact]
        public void ApplyDynamicBinaryBoolean_DistinguishesNullAndUndefinedEquality()
        {
            Assert.True(TsValue.ApplyDynamicBinaryBoolean(null, "==", JSUndefined.value));
            Assert.False(TsValue.ApplyDynamicBinaryBoolean(null, "===", JSUndefined.value));
            Assert.False(TsValue.ApplyDynamicBinaryBoolean(null, "==", false));
            Assert.False(TsValue.ApplyDynamicBinaryBoolean(JSUndefined.value, "==", false));
            Assert.False(TsValue.ApplyDynamicBinaryBoolean(null, "==", 0));
            Assert.True(TsValue.ApplyDynamicBinaryBoolean(false, "==", 0));
        }

        [Fact]
        public void ApplyDynamicBinaryBoolean_UndefinedRelationalComparisonsAreFalse()
        {
            Assert.False(TsValue.ApplyDynamicBinaryBoolean(JSUndefined.value, "<", 0));
            Assert.False(TsValue.ApplyDynamicBinaryBoolean(JSUndefined.value, "<=", 0));
            Assert.False(TsValue.ApplyDynamicBinaryBoolean(JSUndefined.value, ">", 0));
            Assert.False(TsValue.ApplyDynamicBinaryBoolean(JSUndefined.value, ">=", 0));
            Assert.True(TsValue.ApplyDynamicBinaryBoolean(null, "<=", 0));
        }

        [Fact]
        public void ApplyDynamicUnary_UsesClosedPrimitiveOperatorSemantics()
        {
            Assert.Equal(7d, TsValue.ApplyDynamicUnary("7", "+").unwrap());
            Assert.Equal(-7d, TsValue.ApplyDynamicUnary("7", "-").unwrap());
            Assert.Equal(~7, TsValue.ApplyDynamicUnary(7, "~").unwrap());
            Assert.True(TsValue.ApplyDynamicUnaryBoolean(0, "!"));
            Assert.False(TsValue.ApplyDynamicUnaryBoolean("value", "!"));
        }

        [Fact]
        public void ApplyDynamicVoid_EvaluatesOperandAndReturnsUndefined()
        {
            var value = TsValue.ApplyDynamicVoid(TsValue.from(42));

            Assert.Same(JSUndefined.value, value.unwrap());
        }

        [Fact]
        public void ApplyDynamicTypeof_UsesClosedCarrierRuntimeKinds()
        {
            Assert.Equal("undefined", TsValue.ApplyDynamicTypeof(TsValue.undefined()));
            Assert.Equal("object", TsValue.ApplyDynamicTypeof(null));
            Assert.Equal("boolean", TsValue.ApplyDynamicTypeof(true));
            Assert.Equal("number", TsValue.ApplyDynamicTypeof(1));
            Assert.Equal("string", TsValue.ApplyDynamicTypeof("value"));
            Assert.Equal("function", TsValue.ApplyDynamicTypeof(new TsFunction(_ => TsValue.undefined())));
            Assert.Equal("object", TsValue.ApplyDynamicTypeof(new TsObject()));
            Assert.Equal("object", TsValue.ApplyDynamicTypeof(new TsArray()));
        }

        [Fact]
        public void ToDynamicBoolean_UsesClosedJavaScriptTruthiness()
        {
            Assert.False(TsValue.ToDynamicBoolean(TsValue.undefined()));
            Assert.False(TsValue.ToDynamicBoolean(null));
            Assert.False(TsValue.ToDynamicBoolean(""));
            Assert.False(TsValue.ToDynamicBoolean(0));
            Assert.True(TsValue.ToDynamicBoolean("value"));
            Assert.True(TsValue.ToDynamicBoolean(new TsObject()));
        }

        [Fact]
        public void IsDynamicInstanceOf_UsesTheClosedWrappedCarrier()
        {
            var exception = new FormatException("invalid");
            var value = TsValue.from(exception);

            Assert.True(TsValue.IsDynamicInstanceOf<FormatException>(value));
            Assert.True(TsValue.IsDynamicInstanceOf<Exception>(value));
            Assert.False(TsValue.IsDynamicInstanceOf<ArgumentException>(value));
        }

        [Fact]
        public void ThrownValueConversion_PreservesNativeExceptionIdentity()
        {
            var exception = new FormatException("invalid");
            var value = TsThrownValueException.toValue(exception);

            Assert.Same(exception, value.unwrap());
            Assert.Same(exception, TsThrownValueException.from(value));
            Assert.Equal("invalid", value.ReadDynamicSlot("message").unwrap());
        }

        [Fact]
        public void ApplyDynamicOperator_RejectsUnsupportedOperatorsDeterministically()
        {
            Assert.Throws<System.NotSupportedException>(() => TsValue.ApplyDynamicBinary(1, "<<", 2));
            Assert.Throws<System.NotSupportedException>(() => TsValue.ApplyDynamicUnary(1, "delete"));
        }

        [Fact]
        public void CastDynamic_ReturnsClosedTypedCarrierValue()
        {
            Assert.Equal(42, TsValue.CastDynamic<int>(TsValue.from(42)));
            Assert.Equal("Ada", TsValue.CastDynamic<string>(TsValue.from("Ada")));
        }

        [Fact]
        public void CastDynamic_AllowsNullishOnlyForNullableOrReferenceTargets()
        {
            Assert.Null(TsValue.CastDynamic<string>(TsValue.undefined()));
            Assert.Throws<TypeError>(() => TsValue.CastDynamic<int>(TsValue.undefined()));
        }

        [Fact]
        public void CastDynamic_RejectsMismatchedClosedCarrierDeterministically()
        {
            Assert.Throws<TypeError>(() => TsValue.CastDynamic<int>(TsValue.from("42")));
        }
    }
}
