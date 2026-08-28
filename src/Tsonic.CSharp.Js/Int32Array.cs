using System.Collections.Generic;

namespace Tsonic.CSharp.Js
{
    public sealed class Int32Array : TypedArray<Int32Array, int>
    {
        public Int32Array(double length) : base(length) { }
        public Int32Array(IEnumerable<double> values) : base(values) { }
        public Int32Array(ArrayBuffer buffer, double byteOffset = 0, double? length = null) : base(buffer, byteOffset, length) { }
        protected override int ToElement(double value) => checked((int)TypedArrayNumbers.ToSigned(value, 32));
        protected override double FromElement(int value) => value;
        protected override Int32Array CreateView(ArrayBuffer buffer, double byteOffset, double length) => new(buffer, byteOffset, length);
        protected override Int32Array CreateCopy(IEnumerable<double> values) => new(values);
    }
}
