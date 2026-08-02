using System;
using System.Collections.Generic;
using Xunit;

namespace Tsonic.CSharp.Js.Tests
{
    public class TsValueTests
    {
        [Fact]
        public void CreateCompatObject_BuildsOneClosedMutableCarrier()
        {
            var value = TsValue.CreateCompatObject(
                "title", "first",
                "completed", false,
                "title", "last");

            Assert.Equal("last", value.ReadCompatSlot("title").unwrap());
            Assert.Equal(false, value.ReadCompatSlot("completed").unwrap());

            value.WriteCompatSlot("title", "updated");
            Assert.Equal("updated", value.ReadCompatSlot("title").unwrap());
        }

        [Fact]
        public void CreateCompatObject_RejectsMalformedCompilerInput()
        {
            Assert.Throws<ArgumentException>(() =>
                TsValue.CreateCompatObject("title"));
            Assert.Throws<ArgumentException>(() =>
                TsValue.CreateCompatObject(1, "value"));
        }

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
        public void ReadCompatSlot_PreservesPresentNullUndefinedAndMissingUndefined()
        {
            var value = TsValue.from(new TsObject());

            value.WriteCompatSlot("nullValue", null);
            value.WriteCompatSlot("undefinedValue", TsValue.undefined());

            Assert.Null(value.ReadCompatSlot("nullValue").unwrap());
            Assert.Same(JSUndefined.value, value.ReadCompatSlot("undefinedValue").unwrap());
            Assert.Same(JSUndefined.value, value.ReadCompatSlot("missing").unwrap());
        }

        [Fact]
        public void OptionalPropertyRead_SkipsNullishReceivers()
        {
            Assert.Same(
                JSUndefined.value,
                TsValue.undefined().ReadCompatSlotOptional("name").unwrap());
            Assert.Equal(
                "Ada",
                TsValue.from(new TsObject(new Dictionary<string, object?>
                {
                    ["name"] = "Ada"
                })).ReadCompatSlotOptional("name").unwrap());
        }

        [Fact]
        public void ElementAccess_UsesClosedPropertyKeyConversion()
        {
            var value = TsValue.from(new TsObject());

            value.WriteCompatElement(42, "answer");

            Assert.Equal("answer", value.ReadCompatElement("42").unwrap());
        }

        [Fact]
        public void ElementAccess_UsesDistinctNullAndUndefinedPropertyKeys()
        {
            var value = TsValue.from(new TsObject());

            value.WriteCompatElement(null, "null-key");
            value.WriteCompatElement(JSUndefined.value, "undefined-key");

            Assert.Equal("null-key", value.ReadCompatSlot("null").unwrap());
            Assert.Equal("undefined-key", value.ReadCompatSlot("undefined").unwrap());
        }

        [Fact]
        public void OptionalElementRead_DoesNotEvaluateTheKeyForANullishReceiver()
        {
            var evaluated = false;

            var result = TsValue.undefined().ReadCompatElementOptional(() =>
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

            value.WriteCompatElement(2, "third");

            Assert.Equal(3, value.ReadCompatSlot("length").unwrap());
            Assert.Same(JSUndefined.value, value.ReadCompatElement(0).unwrap());
            Assert.Equal("third", value.ReadCompatElement(2).unwrap());
        }

        [Fact]
        public void JsArrayElementAccess_UsesClosedSparseArrayCarrier()
        {
            var array = new JSArray<object?>();
            array.setLength(3);
            array[1] = "middle";
            var value = TsValue.from(array);

            Assert.Equal(3, value.ReadCompatSlot("length").unwrap());
            Assert.Same(JSUndefined.value, value.ReadCompatElement(0).unwrap());
            Assert.Equal("middle", value.ReadCompatElement(1).unwrap());

            value.WriteCompatElement(2, "last");
            Assert.Equal("last", array[2]);

            value.WriteCompatSlot("length", 1);
            Assert.Equal(1, array.length);
            Assert.Same(JSUndefined.value, value.ReadCompatElement(2).unwrap());
        }

        [Fact]
        public void JsArrayElementWrite_RejectsIncompatibleClosedElementCarrier()
        {
            var value = TsValue.from(new JSArray<int>());

            Assert.Throws<TypeError>(() => value.WriteCompatElement(0, "not-an-int"));
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
        public void MemberInvocation_PreservesReceiverAndEvaluationOrder()
        {
            var events = new List<string>();
            var target = TsValue.from(new TsObject());
            target.WriteCompatSlot("name", "Ada");
            target.WriteCompatSlot("greet", new TsFunction((receiver, arguments) =>
            {
                events.Add("call");
                return TsValue.from(
                    (string)receiver.ReadCompatSlot("name").unwrap()! +
                    (string)arguments[0].unwrap()!);
            }));

            var result = target.InvokeCompatSlot(
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

            var receiverResult = TsValue.undefined().InvokeCompatElement(
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
            var callResult = target.InvokeCompatSlot(
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

            var result = TsValue.undefined().InvokeCompatOptional(() =>
            {
                evaluated = true;
                return System.Array.Empty<object?>();
            });

            Assert.Same(JSUndefined.value, result.unwrap());
            Assert.False(evaluated);
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
        }

        [Fact]
        public void ApplyCompatLogical_PreservesShortCircuitEvaluation()
        {
            var evaluations = 0;
            object? right()
            {
                evaluations++;
                return "right";
            }

            Assert.Equal(
                "fallback",
                TsValue.ApplyCompatLogical(JSUndefined.value, "??", () => "fallback").unwrap());
            Assert.Equal("right", TsValue.ApplyCompatLogical(true, "&&", right).unwrap());
            Assert.Equal("left", TsValue.ApplyCompatLogical("left", "||", right).unwrap());
            Assert.Equal(1, evaluations);
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
        public void ApplyCompatBinaryBoolean_DistinguishesNullAndUndefinedEquality()
        {
            Assert.True(TsValue.ApplyCompatBinaryBoolean(null, "==", JSUndefined.value));
            Assert.False(TsValue.ApplyCompatBinaryBoolean(null, "===", JSUndefined.value));
            Assert.False(TsValue.ApplyCompatBinaryBoolean(null, "==", false));
            Assert.False(TsValue.ApplyCompatBinaryBoolean(JSUndefined.value, "==", false));
            Assert.False(TsValue.ApplyCompatBinaryBoolean(null, "==", 0));
            Assert.True(TsValue.ApplyCompatBinaryBoolean(false, "==", 0));
        }

        [Fact]
        public void ApplyCompatBinaryBoolean_UndefinedRelationalComparisonsAreFalse()
        {
            Assert.False(TsValue.ApplyCompatBinaryBoolean(JSUndefined.value, "<", 0));
            Assert.False(TsValue.ApplyCompatBinaryBoolean(JSUndefined.value, "<=", 0));
            Assert.False(TsValue.ApplyCompatBinaryBoolean(JSUndefined.value, ">", 0));
            Assert.False(TsValue.ApplyCompatBinaryBoolean(JSUndefined.value, ">=", 0));
            Assert.True(TsValue.ApplyCompatBinaryBoolean(null, "<=", 0));
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
        public void ApplyCompatVoid_EvaluatesOperandAndReturnsUndefined()
        {
            var value = TsValue.ApplyCompatVoid(TsValue.from(42));

            Assert.Same(JSUndefined.value, value.unwrap());
        }

        [Fact]
        public void ApplyCompatTypeof_UsesClosedCarrierRuntimeKinds()
        {
            Assert.Equal("undefined", TsValue.ApplyCompatTypeof(TsValue.undefined()));
            Assert.Equal("object", TsValue.ApplyCompatTypeof(null));
            Assert.Equal("boolean", TsValue.ApplyCompatTypeof(true));
            Assert.Equal("number", TsValue.ApplyCompatTypeof(1));
            Assert.Equal("string", TsValue.ApplyCompatTypeof("value"));
            Assert.Equal("function", TsValue.ApplyCompatTypeof(new TsFunction(_ => TsValue.undefined())));
            Assert.Equal("object", TsValue.ApplyCompatTypeof(new TsObject()));
            Assert.Equal("object", TsValue.ApplyCompatTypeof(new TsArray()));
        }

        [Fact]
        public void ToCompatBoolean_UsesClosedJavaScriptTruthiness()
        {
            Assert.False(TsValue.ToCompatBoolean(TsValue.undefined()));
            Assert.False(TsValue.ToCompatBoolean(null));
            Assert.False(TsValue.ToCompatBoolean(""));
            Assert.False(TsValue.ToCompatBoolean(0));
            Assert.True(TsValue.ToCompatBoolean("value"));
            Assert.True(TsValue.ToCompatBoolean(new TsObject()));
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
