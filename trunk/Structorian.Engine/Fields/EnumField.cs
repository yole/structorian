using System;
using System.Globalization;
using System.IO;

namespace Structorian.Engine.Fields
{
    class EnumField: IntBasedField
    {
        public EnumField(StructDef structDef, int size) : base(structDef, size, true)
        {
        }
        
        public override void LoadData(BinaryReader reader, StructInstance instance)
        {
            int offset = (int)reader.BaseStream.Position;
            uint value = ReadIntValue(reader).ToUInt32(CultureInfo.CurrentCulture);
            EnumDef enumDef = GetEnumAttribute("enum");
            string displayValue = enumDef.ValueToString(value);
            AddCell(instance, new EnumValue(value, displayValue), displayValue, offset);
        }
    }

    internal class EnumValue: IConvertible
    {
        private IConvertible _intValue;
        private string _strValue;

        public EnumValue(uint intValue, string strValue)
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

        public override string ToString()
        {
            return _strValue;
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
