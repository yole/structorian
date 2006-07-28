using System;
using System.Globalization;
using System.IO;

namespace Structorian.Engine.Fields
{
    class EnumField: IntBasedField
    {
        private string _enumName;
        
        public EnumField(StructDef structDef, int size) : base(structDef, size, false)
        {
        }
        
        public override void SetAttribute(string key, string value)
        {
            if (key == "enum")
                _enumName = value;
            else
                base.SetAttribute(key, value);
        }

        public override void LoadData(BinaryReader reader, StructInstance instance)
        {
            int value = ReadIntValue(reader).ToInt32(CultureInfo.CurrentCulture);
            EnumDef enumDef = _structDef.StructFile.GetEnumByName(_enumName);
            if (enumDef == null)
                throw new LoadDataException("Enum '" + _enumName + "' not found");
            string displayValue = enumDef.ValueToString(value);
            AddCell(instance, new EnumValue(value, displayValue), displayValue);
        }
        
        private class EnumValue: IConvertible
        {
            private IConvertible _intValue;
            private string _strValue;

            public EnumValue(int intValue, string strValue)
            {
                _intValue = intValue;
                _strValue = strValue;
            }

            public TypeCode GetTypeCode()
            {
                return TypeCode.Int32;
            }

            public bool ToBoolean(IFormatProvider provider)
            {
                return _intValue.ToBoolean(provider);
            }

            public char ToChar(IFormatProvider provider)
            {
                return _intValue.ToChar(provider);
            }

            public sbyte ToSByte(IFormatProvider provider)
            {
                return _intValue.ToSByte(provider);
            }

            public byte ToByte(IFormatProvider provider)
            {
                return _intValue.ToByte(provider);
            }

            public short ToInt16(IFormatProvider provider)
            {
                return _intValue.ToInt16(provider);
            }

            public ushort ToUInt16(IFormatProvider provider)
            {
                return _intValue.ToUInt16(provider);
            }

            public int ToInt32(IFormatProvider provider)
            {
                return _intValue.ToInt32(provider);
            }

            public uint ToUInt32(IFormatProvider provider)
            {
                return _intValue.ToUInt32(provider);
            }

            public long ToInt64(IFormatProvider provider)
            {
                return _intValue.ToInt64(provider);
            }

            public ulong ToUInt64(IFormatProvider provider)
            {
                return _intValue.ToUInt64(provider);
            }

            public float ToSingle(IFormatProvider provider)
            {
                return _intValue.ToSingle(provider);
            }

            public double ToDouble(IFormatProvider provider)
            {
                return _intValue.ToDouble(provider);
            }

            public decimal ToDecimal(IFormatProvider provider)
            {
                return _intValue.ToDecimal(provider);
            }

            public DateTime ToDateTime(IFormatProvider provider)
            {
                return _intValue.ToDateTime(provider);
            }

            public string ToString(IFormatProvider provider)
            {
                return _strValue;
            }

            public object ToType(Type conversionType, IFormatProvider provider)
            {
                return _intValue.ToType(conversionType, provider);
            }
        }
    }
}
