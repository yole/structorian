using System;
using System.IO;

namespace Structorian.Engine.Fields
{
    abstract class IntBasedField: StructField
    {
        protected int _size;
        protected bool _unsigned;
        
        protected IntBasedField(StructDef structDef, int size, bool unsigned) : base(structDef)
        {
            _size = size;
            _unsigned = unsigned;
        }

        protected IConvertible ReadIntValue(BinaryReader reader, StructInstance instance)
        {
            Expression expr = GetExpressionAttribute("value");
            if (expr != null)
            {
                return expr.EvaluateInt(instance);
            }
            
            if (_structDef.IsReverseByteOrder())
                reader = new ReverseByteOrderReader(reader.BaseStream);
                    
            IConvertible value;
            switch (_size)
            {
                case 1:
                    value = _unsigned ? (IConvertible)reader.ReadByte() : reader.ReadSByte();
                    break;

                case 2:
                    value = _unsigned ? (IConvertible)reader.ReadUInt16() : reader.ReadInt16();
                    break;

                case 4:
                    value = _unsigned ? (IConvertible)reader.ReadUInt32() : reader.ReadInt32();
                    break;

                case 8:
                    value = _unsigned ? (IConvertible) reader.ReadUInt64() : reader.ReadInt64();
                    break;

                default:
                    throw new Exception("Unsupported integer size " + _size);
            }
            return value;
        }

        public override int GetDataSize()
        {
            return _size;
        }
    }

    internal class ReverseByteOrderReader : BinaryReader
    {
        public ReverseByteOrderReader(Stream stream): base(stream)
        {
        }

        public override short ReadInt16()
        {
            byte[] data = ReadBytes(2);
            return (short)(data[1] | data[0] << 8);
        }

        public override ushort ReadUInt16()
        {
            byte[] data = ReadBytes(2);
            return (ushort)(data[1] | data[0] << 8);
        }

        public override int ReadInt32()
        {
            byte[] data = ReadBytes(4);
            return data[3] | data[2] << 8 | data[1] << 16 | data[0] << 24;
        }

        public override uint ReadUInt32()
        {
            byte[] data = ReadBytes(4);
            return (uint)(data[3] | data[2] << 8 | data[1] << 16 | data[0] << 24);
        }

        public override long ReadInt64()
        {
            byte[] data = ReadBytes(8);
            return data[7] | data[6] << 8 | data[5] << 16 | data[4] << 24 | data[3] << 32 | data[2] << 40 | data[1] << 48 | data[0] << 56;
        }

        public override ulong ReadUInt64()
        {
            byte[] data = ReadBytes(8);
            return (ulong) (data[7] | data[6] << 8 | data[5] << 16 | data[4] << 24 | data[3] << 32 | data[2] << 40 | data[1] << 48 | data[0] << 56);
        }
    }
}
