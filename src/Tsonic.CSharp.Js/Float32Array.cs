using System.Collections.Generic;

namespace Tsonic.CSharp.Js
{
    public sealed class Float32Array : TypedArray<Float32Array, float>
    {
        public Float32Array(double length) : base(length) { }
        public Float32Array(IEnumerable<double> values) : base(values) { }
        public Float32Array(ArrayBuffer buffer, double byteOffset = 0, double? length = null) : base(buffer, byteOffset, length) { }
        protected override float ToElement(double value) => (float)value;
        protected override double FromElement(float value) => value;
        protected override Float32Array CreateView(ArrayBuffer buffer, double byteOffset, double length) => new(buffer, byteOffset, length);
        protected override Float32Array CreateCopy(IEnumerable<double> values) => new(values);
    }
}
