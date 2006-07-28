using System;
using System.IO;

namespace Structorian.Engine.Fields
{
    class IntField: IntBasedField
    {
        private bool _hex;

        public IntField(StructDef structDef, int size, bool unsigned, bool hex)
            : base(structDef, size, unsigned)
        {
            _hex = hex;
        }

        public override void LoadData(BinaryReader reader, StructInstance instance)
        {
            string format = _hex ? "x" + (_size*2).ToString() : "d";
            string displayValue;
            IConvertible value;
            value = ReadIntValue(reader);
            switch(_size)
            {
                case 1:
                    displayValue = _unsigned ? ((Byte)value).ToString(format) : ((SByte)value).ToString(format);
                    break;
                    
                case 2:
                    displayValue = _unsigned ? ((UInt16)value).ToString(format) : ((Int16)value).ToString(format);
                    break;
                    
                case 4:
                    displayValue = _unsigned ? ((UInt32)value).ToString(format) : ((Int32)value).ToString(format);
                    break;

                default:
                    throw new Exception("Unsupported integer size " + _size);
            }
            if (_hex) displayValue = "0x" + displayValue;
            AddCell(instance, value, displayValue);
        }
    }
}
