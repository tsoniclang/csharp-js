using System.Collections.Generic;

namespace Tsonic.CSharp.Js
{
    public sealed class Int16Array : TypedArray<Int16Array, short>
    {
        public Int16Array(double length) : base(length) { }
        public Int16Array(IEnumerable<double> values) : base(values) { }
        public Int16Array(ArrayBuffer buffer, double byteOffset = 0, double? length = null) : base(buffer, byteOffset, length) { }
        protected override short ToElement(double value) => checked((short)TypedArrayNumbers.ToSigned(value, 16));
        protected override double FromElement(short value) => value;
        protected override Int16Array CreateView(ArrayBuffer buffer, double byteOffset, double length) => new(buffer, byteOffset, length);
        protected override Int16Array CreateCopy(IEnumerable<double> values) => new(values);
    }
}
