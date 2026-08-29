using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Tsonic.CSharp.Runtime;
using Xunit;

namespace Tsonic.CSharp.Js.Tests
{
    public class CapabilityClosureTests
    {
        [Fact]
        public void Symbols_PreserveFreshAndRegistryIdentity()
        {
            var first = Symbol.create("state");
            var second = Symbol.create("state");
            var registered = Symbol.@for("state");

            Assert.NotSame(first, second);
            Assert.Same(registered, Symbol.@for("state"));
            Assert.Null(Symbol.keyFor(first));
            Assert.Equal("state", Symbol.keyFor(registered));
        }

        [Fact]
        public async Task PromiseCombinators_PreserveSettlementSemantics()
        {
            var pending = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            var fast = Task.FromResult(7);
            Assert.Equal(7, await PromiseRuntime<int>.Race(new[] { pending.Task, fast }));

            var rejected = Task.FromException<int>(new InvalidOperationException("no"));
            Assert.Equal(7, await PromiseRuntime<int>.Any(new[] { rejected, fast }));
            await Assert.ThrowsAsync<AggregateException>(() =>
                PromiseRuntime<int>.Any(new[] { rejected }));

            var settled = await PromiseRuntime<int>.AllSettled(new[] { fast, rejected });
            Assert.True(settled[0].Is1());
            Assert.Equal(7, settled[0].As1().value);
            Assert.True(settled[1].Is2());
            Assert.IsType<InvalidOperationException>(settled[1].As2().reason);

            var finalizerRuns = 0;
            Assert.Equal(7, await PromiseRuntime<int>.Finally(fast, () => finalizerRuns++));
            Assert.Equal(1, finalizerRuns);
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                PromiseRuntime<int>.Finally(rejected, () => finalizerRuns++));
            Assert.Equal(2, finalizerRuns);
        }

        [Fact]
        public void Intl_UsesOnlyTheDeterministicApprovedLocaleAndTimeZone()
        {
            var dateOptions = JSON.createObject(
                "timeZone", "UTC",
                "year", "numeric",
                "month", "2-digit",
                "day", "2-digit");
            var formatter = new IntlDateTimeFormat(
                TsValue.from("en-US"),
                TsValue.from(dateOptions));

            Assert.Equal("06/15/2023", formatter.format(Date.UTC(2023, 5, 15)));
            Assert.Equal("UTC", formatter.resolvedOptions().timeZone);
            Assert.Throws<RangeError>(() => new IntlDateTimeFormat(TsValue.from("fr-FR")));
            Assert.Throws<RangeError>(() => new IntlDateTimeFormat(
                TsValue.from("en-US"),
                TsValue.from(JSON.createObject("timeZone", "Europe/Paris"))));

            var number = new IntlNumberFormat(
                TsValue.from("en-US"),
                TsValue.from(JSON.createObject("style", "percent", "maximumFractionDigits", 1.0)));
            Assert.Equal("12.5%", number.format(0.125));

            var collator = new IntlCollator(
                TsValue.from("en-US"),
                TsValue.from(JSON.createObject("numeric", true)));
            Assert.True(collator.compare("item2", "item10") < 0);
        }

        [Fact]
        public void JsonStringify_AppliesClosedReplacerAndPropertyListWithoutReflection()
        {
            var source = JSON.createObject("keep", 1.0, "drop", 2.0);
            JsonReplacer replacer = (key, value) =>
                key == "drop" ? TsValue.undefined() : value;

            Assert.Equal("{\"keep\":1}", JSON.stringify(source, replacer));
            Assert.Equal(
                "{\n  \"drop\": 2\n}",
                JSON.stringify(source, new[] { "drop" }, TsValue.from(2.0)));
            Assert.Throws<TypeError>(() => JSON.stringify(source, new object()));
        }

        [Fact]
        public void BinaryViews_ShareBackingStorageAndHonorEndianSelection()
        {
            var buffer = new ArrayBuffer(8);
            var bigEndian = new DataView(buffer);
            var littleEndian = new DataView(buffer);

            bigEndian.setUint32(0, 0x01020304, false);
            Assert.Equal(0x04030201, littleEndian.getUint32(0, true));

            var bytes = new Uint8Array(buffer);
            Assert.Equal(new[] { 1.0, 2.0, 3.0, 4.0 },
                Enumerable.Range(0, 4).Select(index => bytes[index]).ToArray());
            bytes[1] = 9;
            Assert.Equal(0x01090304, bigEndian.getUint32(0, false));
        }

        [Fact]
        public void WeakCollections_DoNotRetainTheirKeys()
        {
            var map = new WeakMap<object, int>();
            var set = new WeakSet<object>();
            var weak = AddEphemeralKey(map, set);

            for (var attempt = 0; attempt < 10 && weak.IsAlive; attempt++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }

            GC.KeepAlive(map);
            GC.KeepAlive(set);
            Assert.False(weak.IsAlive);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference AddEphemeralKey(
            WeakMap<object, int> map,
            WeakSet<object> set)
        {
            var key = new object();
            map.set(key, 1);
            set.add(key);
            return new WeakReference(key);
        }
    }
}
