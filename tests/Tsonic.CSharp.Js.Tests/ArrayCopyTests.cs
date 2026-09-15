using System;
using Tsonic.CSharp.Runtime;
using Xunit;

namespace Tsonic.CSharp.Js.Tests
{
    public sealed class ArrayCopyTests
    {
        [Fact]
        public void DenseCopyKeepsElementIdentityAndIndependentStorage()
        {
            var element = new JSArray<int>(new[] { 4 });
            var source = new JSArray<JSArray<int>>(new[] { element });
            var copy = JSArrayStatics.fromDense(source);
            Assert.NotSame(source, copy);
            Assert.Same(element, copy[0]);
            copy[0] = new JSArray<int>(new[] { 8 });
            Assert.Same(element, source[0]);
            var error = Assert.Throws<InvalidOperationException>(() => JSArrayStatics.fromDense(new JSArray<int>(2)));
            Assert.Equal("checked array density invariant violated", error.Message);
        }

        [Fact]
        public void OptionalCopiesMaterializeHolesAsPresentUndefined()
        {
            var source = JSArray<int?>.fromSparse(3, (0, 4), (2, null));
            var copy = JSArrayStatics.fromOptionalValue(source);
            Assert.Equal(new int?[] { 4, null, null }, copy);
            Assert.False(source.hasIndex(1));
            Assert.True(copy.hasIndex(1));
            Assert.True(copy.hasIndex(2));
            var references = JSArray<string?>.fromSparse(3, (0, "value"), (2, null));
            var referenceCopy = JSArrayStatics.fromOptionalReference(references);
            Assert.Equal(new string?[] { "value", null, null }, referenceCopy);
            Assert.True(referenceCopy.hasIndex(1));
            Assert.False(references.hasIndex(1));
            var onlyUndefined = new JSArray<Undefined>(2);
            var undefinedCopy = JSArrayStatics.fromUndefined(onlyUndefined);
            Assert.Equal(2, undefinedCopy.length);
            Assert.Same(Undefined.value, undefinedCopy[0]);
            Assert.Same(Undefined.value, undefinedCopy[1]);
            Assert.True(undefinedCopy.hasIndex(0));
            Assert.False(onlyUndefined.hasIndex(0));
        }

        [Fact]
        public void MappingUsesTheLiveIteratorAndPreservesTheExactException()
        {
            var source = JSArray<int?>.fromSparse(3, (0, 4));
            var visited = new System.Collections.Generic.List<int?>();
            var error = new InvalidOperationException("stop");
            var thrown = Assert.Throws<InvalidOperationException>(() =>
                JSArrayStatics.fromOptionalValue<int, int>(source, (value, index) =>
                {
                    visited.Add(value);
                    if (index == 0) source[2] = 9;
                    if (index == 2) throw error;
                    return index;
                }));
            Assert.Same(error, thrown);
            Assert.Equal(new int?[] { 4, null, 9 }, visited);
            Assert.Equal(new[] { "value", "missing" }, JSArrayStatics.fromOptionalReference<string, string>(
                JSArray<string?>.fromSparse(2, (0, "value")), (value, _) => value ?? "missing"));
            Assert.Equal(new[] { 0, 1 }, JSArrayStatics.fromUndefined(new JSArray<Undefined>(2), (value, index) =>
            {
                Assert.Same(Undefined.value, value);
                return index;
            }));
        }
    }
}
