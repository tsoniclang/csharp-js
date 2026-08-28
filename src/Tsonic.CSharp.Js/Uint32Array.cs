using System.Collections.Generic;

namespace Tsonic.CSharp.Js
{
    public sealed class Uint32Array : TypedArray<Uint32Array, uint>
    {
        public Uint32Array(double length) : base(length) { }
        public Uint32Array(IEnumerable<double> values) : base(values) { }
        public Uint32Array(ArrayBuffer buffer, double byteOffset = 0, double? length = null) : base(buffer, byteOffset, length) { }
        protected override uint ToElement(double value) => checked((uint)TypedArrayNumbers.ToUnsigned(value, 32));
        protected override double FromElement(uint value) => value;
        protected override Uint32Array CreateView(ArrayBuffer buffer, double byteOffset, double length) => new(buffer, byteOffset, length);
        protected override Uint32Array CreateCopy(IEnumerable<double> values) => new(values);
    }
}
