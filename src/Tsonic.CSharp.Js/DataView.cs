using System;
using System.Buffers.Binary;

namespace Tsonic.CSharp.Js
{
    public sealed class DataView
    {
        private readonly int _byteOffset;

        public DataView(ArrayBuffer buffer, double byteOffset = 0, double? byteLength = null)
        {
            ArgumentNullException.ThrowIfNull(buffer);
            var offset = CheckedIndex(byteOffset);
            if (offset > buffer.ByteLength)
            {
                throw new RangeError("DataView byteOffset is outside the ArrayBuffer.");
            }
            var selectedLength = byteLength is null
                ? buffer.ByteLength - offset
                : CheckedIndex(byteLength.Value);
            if (selectedLength > buffer.ByteLength - offset)
            {
                throw new RangeError("DataView byteLength exceeds the ArrayBuffer.");
            }
            this.buffer = buffer;
            _byteOffset = offset;
            ByteLength = selectedLength;
        }

        public ArrayBuffer buffer { get; }

        public int byteOffset => _byteOffset;

        public int byteLength => ByteLength;

        private int ByteLength { get; }

        public sbyte getInt8(double offset) => unchecked((sbyte)Read(offset, 1)[0]);

        public byte getUint8(double offset) => Read(offset, 1)[0];

        public short getInt16(double offset, bool littleEndian = false) =>
            littleEndian ? BinaryPrimitives.ReadInt16LittleEndian(Read(offset, 2)) : BinaryPrimitives.ReadInt16BigEndian(Read(offset, 2));

        public ushort getUint16(double offset, bool littleEndian = false) =>
            littleEndian ? BinaryPrimitives.ReadUInt16LittleEndian(Read(offset, 2)) : BinaryPrimitives.ReadUInt16BigEndian(Read(offset, 2));

        public int getInt32(double offset, bool littleEndian = false) =>
            littleEndian ? BinaryPrimitives.ReadInt32LittleEndian(Read(offset, 4)) : BinaryPrimitives.ReadInt32BigEndian(Read(offset, 4));

        public uint getUint32(double offset, bool littleEndian = false) =>
            littleEndian ? BinaryPrimitives.ReadUInt32LittleEndian(Read(offset, 4)) : BinaryPrimitives.ReadUInt32BigEndian(Read(offset, 4));

        public float getFloat32(double offset, bool littleEndian = false) =>
            BitConverter.Int32BitsToSingle(getInt32(offset, littleEndian));

        public double getFloat64(double offset, bool littleEndian = false) =>
            BitConverter.Int64BitsToDouble(littleEndian
                ? BinaryPrimitives.ReadInt64LittleEndian(Read(offset, 8))
                : BinaryPrimitives.ReadInt64BigEndian(Read(offset, 8)));

        public void setInt8(double offset, double value) => Writable(offset, 1)[0] = unchecked((byte)NativeInteger.Bits32(value));

        public void setUint8(double offset, double value) => Writable(offset, 1)[0] = unchecked((byte)NativeInteger.Bits32(value));

        public void setInt16(double offset, double value, bool littleEndian = false)
        {
            var span = Writable(offset, 2);
            var converted = unchecked((short)NativeInteger.Bits32(value));
            if (littleEndian) BinaryPrimitives.WriteInt16LittleEndian(span, converted);
            else BinaryPrimitives.WriteInt16BigEndian(span, converted);
        }

        public void setUint16(double offset, double value, bool littleEndian = false)
        {
            var span = Writable(offset, 2);
            var converted = unchecked((ushort)NativeInteger.Bits32(value));
            if (littleEndian) BinaryPrimitives.WriteUInt16LittleEndian(span, converted);
            else BinaryPrimitives.WriteUInt16BigEndian(span, converted);
        }

        public void setInt32(double offset, double value, bool littleEndian = false)
        {
            var span = Writable(offset, 4);
            var converted = unchecked((int)NativeInteger.Bits32(value));
            if (littleEndian) BinaryPrimitives.WriteInt32LittleEndian(span, converted);
            else BinaryPrimitives.WriteInt32BigEndian(span, converted);
        }

        public void setUint32(double offset, double value, bool littleEndian = false)
        {
            var span = Writable(offset, 4);
            var converted = unchecked((uint)NativeInteger.Bits32(value));
            if (littleEndian) BinaryPrimitives.WriteUInt32LittleEndian(span, converted);
            else BinaryPrimitives.WriteUInt32BigEndian(span, converted);
        }

        public void setFloat32(double offset, double value, bool littleEndian = false)
        {
            var span = Writable(offset, 4);
            var bits = BitConverter.SingleToInt32Bits((float)value);
            if (littleEndian) BinaryPrimitives.WriteInt32LittleEndian(span, bits);
            else BinaryPrimitives.WriteInt32BigEndian(span, bits);
        }

        public void setFloat64(double offset, double value, bool littleEndian = false)
        {
            var span = Writable(offset, 8);
            var bits = BitConverter.DoubleToInt64Bits(value);
            if (littleEndian) BinaryPrimitives.WriteInt64LittleEndian(span, bits);
            else BinaryPrimitives.WriteInt64BigEndian(span, bits);
        }

        private ReadOnlySpan<byte> Read(double offset, int width)
        {
            var selected = ValidateRange(offset, width);
            return buffer.Bytes.AsSpan(_byteOffset + selected, width);
        }

        private Span<byte> Writable(double offset, int width)
        {
            var selected = ValidateRange(offset, width);
            return buffer.Bytes.AsSpan(_byteOffset + selected, width);
        }

        private int ValidateRange(double offset, int width)
        {
            var selected = CheckedIndex(offset);
            if (selected > ByteLength - width)
            {
                throw new RangeError("DataView offset is outside the view.");
            }
            return selected;
        }

        private static int CheckedIndex(double value)
        {
            if (!double.IsFinite(value))
            {
                throw new RangeError("DataView offsets and lengths must be finite.");
            }
            var integer = System.Math.Truncate(value);
            if (integer < 0 || integer > int.MaxValue)
            {
                throw new RangeError("DataView offset or length is outside the supported range.");
            }
            return checked((int)integer);
        }
    }
}
