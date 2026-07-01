using Tsonic.CSharp.Runtime;

namespace Tsonic.CSharp.Js
{
    public sealed class TsUnion
    {
        private readonly TsValue _value;

        private TsUnion(int armIndex, int armCount, object? value)
        {
            if (armCount < 2 || armCount > 8)
            {
                throw new TypeError("TsUnion supports closed union carriers with 2 through 8 arms.");
            }
            if (armIndex < 1 || armIndex > armCount)
            {
                throw new TypeError("TsUnion active arm is outside the closed union carrier range.");
            }
            ArmIndex = armIndex;
            ArmCount = armCount;
            _value = TsValue.from(value);
        }

        public int ArmIndex { get; }

        public int ArmCount { get; }

        public TsValue value()
        {
            return _value;
        }

        public object? unwrap()
        {
            return _value.unwrap();
        }

        public bool isArm(int armIndex)
        {
            return ArmIndex == armIndex;
        }

        public TsValue asArm(int armIndex)
        {
            if (!isArm(armIndex))
            {
                throw new TypeError("TsUnion active arm does not match the requested closed union arm.");
            }
            return _value;
        }

        public TsValue ReadCompatSlot(string key)
        {
            return _value.ReadCompatSlot(key);
        }

        public TsValue WriteCompatSlot(string key, object? value)
        {
            return _value.WriteCompatSlot(key, value);
        }

        public TsValue ReadCompatElement(object? key)
        {
            return _value.ReadCompatElement(key);
        }

        public TsValue WriteCompatElement(object? key, object? value)
        {
            return _value.WriteCompatElement(key, value);
        }

        public TsValue InvokeCompat(params object?[] arguments)
        {
            return _value.InvokeCompat(arguments);
        }

        public TsValue ConstructCompat(params object?[] arguments)
        {
            return _value.ConstructCompat(arguments);
        }

        public static TsUnion From(int armIndex, int armCount, object? value)
        {
            return new TsUnion(armIndex, armCount, value);
        }

        public static TsUnion From<T1, T2>(Union<T1, T2>? value)
        {
            if (value is null)
            {
                return From(1, 2, null);
            }
            if (value.TryAs1(out var arm1))
            {
                return From(1, 2, arm1);
            }
            if (value.TryAs2(out var arm2))
            {
                return From(2, 2, arm2);
            }
            throw invalidRuntimeUnionState();
        }

        public static TsUnion From<T1, T2, T3>(Union<T1, T2, T3>? value)
        {
            if (value is null)
            {
                return From(1, 3, null);
            }
            if (value.TryAs1(out var arm1))
            {
                return From(1, 3, arm1);
            }
            if (value.TryAs2(out var arm2))
            {
                return From(2, 3, arm2);
            }
            if (value.TryAs3(out var arm3))
            {
                return From(3, 3, arm3);
            }
            throw invalidRuntimeUnionState();
        }

        public static TsUnion From<T1, T2, T3, T4>(Union<T1, T2, T3, T4>? value)
        {
            if (value is null)
            {
                return From(1, 4, null);
            }
            if (value.TryAs1(out var arm1))
            {
                return From(1, 4, arm1);
            }
            if (value.TryAs2(out var arm2))
            {
                return From(2, 4, arm2);
            }
            if (value.TryAs3(out var arm3))
            {
                return From(3, 4, arm3);
            }
            if (value.TryAs4(out var arm4))
            {
                return From(4, 4, arm4);
            }
            throw invalidRuntimeUnionState();
        }

        public static TsUnion From<T1, T2, T3, T4, T5>(Union<T1, T2, T3, T4, T5>? value)
        {
            if (value is null)
            {
                return From(1, 5, null);
            }
            if (value.TryAs1(out var arm1))
            {
                return From(1, 5, arm1);
            }
            if (value.TryAs2(out var arm2))
            {
                return From(2, 5, arm2);
            }
            if (value.TryAs3(out var arm3))
            {
                return From(3, 5, arm3);
            }
            if (value.TryAs4(out var arm4))
            {
                return From(4, 5, arm4);
            }
            if (value.TryAs5(out var arm5))
            {
                return From(5, 5, arm5);
            }
            throw invalidRuntimeUnionState();
        }

        public static TsUnion From<T1, T2, T3, T4, T5, T6>(Union<T1, T2, T3, T4, T5, T6>? value)
        {
            if (value is null)
            {
                return From(1, 6, null);
            }
            if (value.TryAs1(out var arm1))
            {
                return From(1, 6, arm1);
            }
            if (value.TryAs2(out var arm2))
            {
                return From(2, 6, arm2);
            }
            if (value.TryAs3(out var arm3))
            {
                return From(3, 6, arm3);
            }
            if (value.TryAs4(out var arm4))
            {
                return From(4, 6, arm4);
            }
            if (value.TryAs5(out var arm5))
            {
                return From(5, 6, arm5);
            }
            if (value.TryAs6(out var arm6))
            {
                return From(6, 6, arm6);
            }
            throw invalidRuntimeUnionState();
        }

        public static TsUnion From<T1, T2, T3, T4, T5, T6, T7>(Union<T1, T2, T3, T4, T5, T6, T7>? value)
        {
            if (value is null)
            {
                return From(1, 7, null);
            }
            if (value.TryAs1(out var arm1))
            {
                return From(1, 7, arm1);
            }
            if (value.TryAs2(out var arm2))
            {
                return From(2, 7, arm2);
            }
            if (value.TryAs3(out var arm3))
            {
                return From(3, 7, arm3);
            }
            if (value.TryAs4(out var arm4))
            {
                return From(4, 7, arm4);
            }
            if (value.TryAs5(out var arm5))
            {
                return From(5, 7, arm5);
            }
            if (value.TryAs6(out var arm6))
            {
                return From(6, 7, arm6);
            }
            if (value.TryAs7(out var arm7))
            {
                return From(7, 7, arm7);
            }
            throw invalidRuntimeUnionState();
        }

        public static TsUnion From<T1, T2, T3, T4, T5, T6, T7, T8>(Union<T1, T2, T3, T4, T5, T6, T7, T8>? value)
        {
            if (value is null)
            {
                return From(1, 8, null);
            }
            if (value.TryAs1(out var arm1))
            {
                return From(1, 8, arm1);
            }
            if (value.TryAs2(out var arm2))
            {
                return From(2, 8, arm2);
            }
            if (value.TryAs3(out var arm3))
            {
                return From(3, 8, arm3);
            }
            if (value.TryAs4(out var arm4))
            {
                return From(4, 8, arm4);
            }
            if (value.TryAs5(out var arm5))
            {
                return From(5, 8, arm5);
            }
            if (value.TryAs6(out var arm6))
            {
                return From(6, 8, arm6);
            }
            if (value.TryAs7(out var arm7))
            {
                return From(7, 8, arm7);
            }
            if (value.TryAs8(out var arm8))
            {
                return From(8, 8, arm8);
            }
            throw invalidRuntimeUnionState();
        }

        public static Union<T1, T2> CastCompat<T1, T2>(object? value)
        {
            var union = tryGetUnion(value, 2);
            if (union is not null)
            {
                if (union.isArm(1))
                {
                    return Union<T1, T2>.From1(TsValue.CastCompat<T1>(union.asArm(1)));
                }
                return Union<T1, T2>.From2(TsValue.CastCompat<T2>(union.asArm(2)));
            }
            if (TsValue.TryCastCompat<T1>(value, out var rawArm1))
            {
                return Union<T1, T2>.From1(rawArm1);
            }
            if (TsValue.TryCastCompat<T2>(value, out var rawArm2))
            {
                return Union<T1, T2>.From2(rawArm2);
            }
            throw unionCastError();
        }

        public static Union<T1, T2, T3> CastCompat<T1, T2, T3>(object? value)
        {
            var union = tryGetUnion(value, 3);
            if (union is not null)
            {
                if (union.isArm(1))
                {
                    return Union<T1, T2, T3>.From1(TsValue.CastCompat<T1>(union.asArm(1)));
                }
                if (union.isArm(2))
                {
                    return Union<T1, T2, T3>.From2(TsValue.CastCompat<T2>(union.asArm(2)));
                }
                return Union<T1, T2, T3>.From3(TsValue.CastCompat<T3>(union.asArm(3)));
            }
            if (TsValue.TryCastCompat<T1>(value, out var rawArm1))
            {
                return Union<T1, T2, T3>.From1(rawArm1);
            }
            if (TsValue.TryCastCompat<T2>(value, out var rawArm2))
            {
                return Union<T1, T2, T3>.From2(rawArm2);
            }
            if (TsValue.TryCastCompat<T3>(value, out var rawArm3))
            {
                return Union<T1, T2, T3>.From3(rawArm3);
            }
            throw unionCastError();
        }

        public static Union<T1, T2, T3, T4> CastCompat<T1, T2, T3, T4>(object? value)
        {
            var union = tryGetUnion(value, 4);
            if (union is not null)
            {
                if (union.isArm(1))
                {
                    return Union<T1, T2, T3, T4>.From1(TsValue.CastCompat<T1>(union.asArm(1)));
                }
                if (union.isArm(2))
                {
                    return Union<T1, T2, T3, T4>.From2(TsValue.CastCompat<T2>(union.asArm(2)));
                }
                if (union.isArm(3))
                {
                    return Union<T1, T2, T3, T4>.From3(TsValue.CastCompat<T3>(union.asArm(3)));
                }
                return Union<T1, T2, T3, T4>.From4(TsValue.CastCompat<T4>(union.asArm(4)));
            }
            if (TsValue.TryCastCompat<T1>(value, out var rawArm1))
            {
                return Union<T1, T2, T3, T4>.From1(rawArm1);
            }
            if (TsValue.TryCastCompat<T2>(value, out var rawArm2))
            {
                return Union<T1, T2, T3, T4>.From2(rawArm2);
            }
            if (TsValue.TryCastCompat<T3>(value, out var rawArm3))
            {
                return Union<T1, T2, T3, T4>.From3(rawArm3);
            }
            if (TsValue.TryCastCompat<T4>(value, out var rawArm4))
            {
                return Union<T1, T2, T3, T4>.From4(rawArm4);
            }
            throw unionCastError();
        }

        public static Union<T1, T2, T3, T4, T5> CastCompat<T1, T2, T3, T4, T5>(object? value)
        {
            var union = tryGetUnion(value, 5);
            if (union is not null)
            {
                if (union.isArm(1))
                {
                    return Union<T1, T2, T3, T4, T5>.From1(TsValue.CastCompat<T1>(union.asArm(1)));
                }
                if (union.isArm(2))
                {
                    return Union<T1, T2, T3, T4, T5>.From2(TsValue.CastCompat<T2>(union.asArm(2)));
                }
                if (union.isArm(3))
                {
                    return Union<T1, T2, T3, T4, T5>.From3(TsValue.CastCompat<T3>(union.asArm(3)));
                }
                if (union.isArm(4))
                {
                    return Union<T1, T2, T3, T4, T5>.From4(TsValue.CastCompat<T4>(union.asArm(4)));
                }
                return Union<T1, T2, T3, T4, T5>.From5(TsValue.CastCompat<T5>(union.asArm(5)));
            }
            if (TsValue.TryCastCompat<T1>(value, out var rawArm1))
            {
                return Union<T1, T2, T3, T4, T5>.From1(rawArm1);
            }
            if (TsValue.TryCastCompat<T2>(value, out var rawArm2))
            {
                return Union<T1, T2, T3, T4, T5>.From2(rawArm2);
            }
            if (TsValue.TryCastCompat<T3>(value, out var rawArm3))
            {
                return Union<T1, T2, T3, T4, T5>.From3(rawArm3);
            }
            if (TsValue.TryCastCompat<T4>(value, out var rawArm4))
            {
                return Union<T1, T2, T3, T4, T5>.From4(rawArm4);
            }
            if (TsValue.TryCastCompat<T5>(value, out var rawArm5))
            {
                return Union<T1, T2, T3, T4, T5>.From5(rawArm5);
            }
            throw unionCastError();
        }

        public static Union<T1, T2, T3, T4, T5, T6> CastCompat<T1, T2, T3, T4, T5, T6>(object? value)
        {
            var union = tryGetUnion(value, 6);
            if (union is not null)
            {
                if (union.isArm(1))
                {
                    return Union<T1, T2, T3, T4, T5, T6>.From1(TsValue.CastCompat<T1>(union.asArm(1)));
                }
                if (union.isArm(2))
                {
                    return Union<T1, T2, T3, T4, T5, T6>.From2(TsValue.CastCompat<T2>(union.asArm(2)));
                }
                if (union.isArm(3))
                {
                    return Union<T1, T2, T3, T4, T5, T6>.From3(TsValue.CastCompat<T3>(union.asArm(3)));
                }
                if (union.isArm(4))
                {
                    return Union<T1, T2, T3, T4, T5, T6>.From4(TsValue.CastCompat<T4>(union.asArm(4)));
                }
                if (union.isArm(5))
                {
                    return Union<T1, T2, T3, T4, T5, T6>.From5(TsValue.CastCompat<T5>(union.asArm(5)));
                }
                return Union<T1, T2, T3, T4, T5, T6>.From6(TsValue.CastCompat<T6>(union.asArm(6)));
            }
            if (TsValue.TryCastCompat<T1>(value, out var rawArm1))
            {
                return Union<T1, T2, T3, T4, T5, T6>.From1(rawArm1);
            }
            if (TsValue.TryCastCompat<T2>(value, out var rawArm2))
            {
                return Union<T1, T2, T3, T4, T5, T6>.From2(rawArm2);
            }
            if (TsValue.TryCastCompat<T3>(value, out var rawArm3))
            {
                return Union<T1, T2, T3, T4, T5, T6>.From3(rawArm3);
            }
            if (TsValue.TryCastCompat<T4>(value, out var rawArm4))
            {
                return Union<T1, T2, T3, T4, T5, T6>.From4(rawArm4);
            }
            if (TsValue.TryCastCompat<T5>(value, out var rawArm5))
            {
                return Union<T1, T2, T3, T4, T5, T6>.From5(rawArm5);
            }
            if (TsValue.TryCastCompat<T6>(value, out var rawArm6))
            {
                return Union<T1, T2, T3, T4, T5, T6>.From6(rawArm6);
            }
            throw unionCastError();
        }

        public static Union<T1, T2, T3, T4, T5, T6, T7> CastCompat<T1, T2, T3, T4, T5, T6, T7>(object? value)
        {
            var union = tryGetUnion(value, 7);
            if (union is not null)
            {
                if (union.isArm(1))
                {
                    return Union<T1, T2, T3, T4, T5, T6, T7>.From1(TsValue.CastCompat<T1>(union.asArm(1)));
                }
                if (union.isArm(2))
                {
                    return Union<T1, T2, T3, T4, T5, T6, T7>.From2(TsValue.CastCompat<T2>(union.asArm(2)));
                }
                if (union.isArm(3))
                {
                    return Union<T1, T2, T3, T4, T5, T6, T7>.From3(TsValue.CastCompat<T3>(union.asArm(3)));
                }
                if (union.isArm(4))
                {
                    return Union<T1, T2, T3, T4, T5, T6, T7>.From4(TsValue.CastCompat<T4>(union.asArm(4)));
                }
                if (union.isArm(5))
                {
                    return Union<T1, T2, T3, T4, T5, T6, T7>.From5(TsValue.CastCompat<T5>(union.asArm(5)));
                }
                if (union.isArm(6))
                {
                    return Union<T1, T2, T3, T4, T5, T6, T7>.From6(TsValue.CastCompat<T6>(union.asArm(6)));
                }
                return Union<T1, T2, T3, T4, T5, T6, T7>.From7(TsValue.CastCompat<T7>(union.asArm(7)));
            }
            if (TsValue.TryCastCompat<T1>(value, out var rawArm1))
            {
                return Union<T1, T2, T3, T4, T5, T6, T7>.From1(rawArm1);
            }
            if (TsValue.TryCastCompat<T2>(value, out var rawArm2))
            {
                return Union<T1, T2, T3, T4, T5, T6, T7>.From2(rawArm2);
            }
            if (TsValue.TryCastCompat<T3>(value, out var rawArm3))
            {
                return Union<T1, T2, T3, T4, T5, T6, T7>.From3(rawArm3);
            }
            if (TsValue.TryCastCompat<T4>(value, out var rawArm4))
            {
                return Union<T1, T2, T3, T4, T5, T6, T7>.From4(rawArm4);
            }
            if (TsValue.TryCastCompat<T5>(value, out var rawArm5))
            {
                return Union<T1, T2, T3, T4, T5, T6, T7>.From5(rawArm5);
            }
            if (TsValue.TryCastCompat<T6>(value, out var rawArm6))
            {
                return Union<T1, T2, T3, T4, T5, T6, T7>.From6(rawArm6);
            }
            if (TsValue.TryCastCompat<T7>(value, out var rawArm7))
            {
                return Union<T1, T2, T3, T4, T5, T6, T7>.From7(rawArm7);
            }
            throw unionCastError();
        }

        public static Union<T1, T2, T3, T4, T5, T6, T7, T8> CastCompat<T1, T2, T3, T4, T5, T6, T7, T8>(object? value)
        {
            var union = tryGetUnion(value, 8);
            if (union is not null)
            {
                if (union.isArm(1))
                {
                    return Union<T1, T2, T3, T4, T5, T6, T7, T8>.From1(TsValue.CastCompat<T1>(union.asArm(1)));
                }
                if (union.isArm(2))
                {
                    return Union<T1, T2, T3, T4, T5, T6, T7, T8>.From2(TsValue.CastCompat<T2>(union.asArm(2)));
                }
                if (union.isArm(3))
                {
                    return Union<T1, T2, T3, T4, T5, T6, T7, T8>.From3(TsValue.CastCompat<T3>(union.asArm(3)));
                }
                if (union.isArm(4))
                {
                    return Union<T1, T2, T3, T4, T5, T6, T7, T8>.From4(TsValue.CastCompat<T4>(union.asArm(4)));
                }
                if (union.isArm(5))
                {
                    return Union<T1, T2, T3, T4, T5, T6, T7, T8>.From5(TsValue.CastCompat<T5>(union.asArm(5)));
                }
                if (union.isArm(6))
                {
                    return Union<T1, T2, T3, T4, T5, T6, T7, T8>.From6(TsValue.CastCompat<T6>(union.asArm(6)));
                }
                if (union.isArm(7))
                {
                    return Union<T1, T2, T3, T4, T5, T6, T7, T8>.From7(TsValue.CastCompat<T7>(union.asArm(7)));
                }
                return Union<T1, T2, T3, T4, T5, T6, T7, T8>.From8(TsValue.CastCompat<T8>(union.asArm(8)));
            }
            if (TsValue.TryCastCompat<T1>(value, out var rawArm1))
            {
                return Union<T1, T2, T3, T4, T5, T6, T7, T8>.From1(rawArm1);
            }
            if (TsValue.TryCastCompat<T2>(value, out var rawArm2))
            {
                return Union<T1, T2, T3, T4, T5, T6, T7, T8>.From2(rawArm2);
            }
            if (TsValue.TryCastCompat<T3>(value, out var rawArm3))
            {
                return Union<T1, T2, T3, T4, T5, T6, T7, T8>.From3(rawArm3);
            }
            if (TsValue.TryCastCompat<T4>(value, out var rawArm4))
            {
                return Union<T1, T2, T3, T4, T5, T6, T7, T8>.From4(rawArm4);
            }
            if (TsValue.TryCastCompat<T5>(value, out var rawArm5))
            {
                return Union<T1, T2, T3, T4, T5, T6, T7, T8>.From5(rawArm5);
            }
            if (TsValue.TryCastCompat<T6>(value, out var rawArm6))
            {
                return Union<T1, T2, T3, T4, T5, T6, T7, T8>.From6(rawArm6);
            }
            if (TsValue.TryCastCompat<T7>(value, out var rawArm7))
            {
                return Union<T1, T2, T3, T4, T5, T6, T7, T8>.From7(rawArm7);
            }
            if (TsValue.TryCastCompat<T8>(value, out var rawArm8))
            {
                return Union<T1, T2, T3, T4, T5, T6, T7, T8>.From8(rawArm8);
            }
            throw unionCastError();
        }

        private static TsUnion? tryGetUnion(object? value, int armCount)
        {
            value = TsValue.UnwrapCompatCarrier(value);
            if (value is TsUnion union)
            {
                return union.ArmCount == armCount ? union : throw unionCastError();
            }
            return null;
        }

        private static TypeError unionCastError()
        {
            return new TypeError("TsUnion cast requires a value assignable to a closed union arm or a closed TsUnion carrier with matching arm count.");
        }

        private static TypeError invalidRuntimeUnionState()
        {
            return new TypeError("Runtime union carrier did not expose an active arm.");
        }
    }
}
