using System;
using System.IO;

namespace Structorian.Engine.Fields
{
    abstract class IntBasedField: StructField
    {
        protected int _size;
        protected bool _unsigned;
        protected int _frombit;
        protected int _tobit;
        
        protected IntBasedField(StructDef structDef, int size, bool unsigned) : base(structDef)
        {
            _size = size;
            _unsigned = unsigned;
        }

        public int FromBit
        {
            get { return _frombit; }
        }

        public int ToBit
        {
            get { return _tobit; }
        }

        public override void SetAttribute(string key, string value)
        {
            if (key == "frombit")
                _frombit = Int32.Parse(value);
            else if (key == "tobit")
                _tobit = Int32.Parse(value);
            else
                base.SetAttribute(key, value);
        }

        protected IConvertible ReadIntValue(BinaryReader reader)
        {
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

                default:
                    throw new Exception("Unsupported integer size " + _size);
            }
            return value;
        }
    }
}
