using System.Collections.Generic;

namespace Tsonic.CSharp.Js
{
    public sealed class Float64Array : TypedArray<Float64Array, double>
    {
        public Float64Array(double length) : base(length) { }
        public Float64Array(IEnumerable<double> values) : base(values) { }
        public Float64Array(ArrayBuffer buffer, double byteOffset = 0, double? length = null) : base(buffer, byteOffset, length) { }
        protected override double ToElement(double value) => value;
        protected override double FromElement(double value) => value;
        protected override Float64Array CreateView(ArrayBuffer buffer, double byteOffset, double length) => new(buffer, byteOffset, length);
        protected override Float64Array CreateCopy(IEnumerable<double> values) => new(values);
    }
}
