using System.Collections.Generic;

namespace Tsonic.CSharp.Js
{
    public sealed class Int8Array : TypedArray<Int8Array, sbyte>
    {
        public Int8Array(double length) : base(length) { }
        public Int8Array(IEnumerable<double> values) : base(values) { }
        public Int8Array(ArrayBuffer buffer, double byteOffset = 0, double? length = null) : base(buffer, byteOffset, length) { }
        protected override sbyte ToElement(double value) => checked((sbyte)TypedArrayNumbers.ToSigned(value, 8));
        protected override double FromElement(sbyte value) => value;
        protected override Int8Array CreateView(ArrayBuffer buffer, double byteOffset, double length) => new(buffer, byteOffset, length);
        protected override Int8Array CreateCopy(IEnumerable<double> values) => new(values);
    }
}
