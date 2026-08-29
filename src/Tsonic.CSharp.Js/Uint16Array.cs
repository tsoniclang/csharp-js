using System.Collections.Generic;

namespace Tsonic.CSharp.Js
{
    public sealed class Uint16Array : TypedArray<Uint16Array, ushort>
    {
        public Uint16Array(double length) : base(length) { }
        public Uint16Array(IEnumerable<double> values) : base(values) { }
        public Uint16Array(ArrayBuffer buffer, double byteOffset = 0, double? length = null) : base(buffer, byteOffset, length) { }
        protected override ushort ToElement(double value) => checked((ushort)TypedArrayNumbers.ToUnsigned(value, 16));
        protected override double FromElement(ushort value) => value;
        protected override Uint16Array CreateView(ArrayBuffer buffer, double byteOffset, double length) => new(buffer, byteOffset, length);
        protected override Uint16Array CreateCopy(IEnumerable<double> values) => new(values);
    }
}
