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
            uint value = ReadIntValue(reader, instance).ToUInt32(CultureInfo.CurrentCulture);
            EnumDef enumDef = GetEnumAttribute("enum");
            AddCell(instance, new EnumValue(value, enumDef), enumDef.ValueToString(value), offset);
        }
    }

    internal class EnumValue: IConvertible, IComparable
    {
        protected readonly uint _intValue;
        private readonly EnumDef _enumDef;

        public EnumValue(uint intValue, EnumDef enumDef)
        {
            _intValue = intValue;
            _enumDef = enumDef;
        }

        public TypeCode GetTypeCode()
        {
            return TypeCode.Int32;
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            return ((IConvertible) _intValue).ToBoolean(provider);
        }

        public char ToChar(IFormatProvider provider)
        {
            return ((IConvertible)_intValue).ToChar(provider);
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            return ((IConvertible)_intValue).ToSByte(provider);
        }

        public byte ToByte(IFormatProvider provider)
        {
            return ((IConvertible)_intValue).ToByte(provider);
        }

        public short ToInt16(IFormatProvider provider)
        {
            return ((IConvertible)_intValue).ToInt16(provider);
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            return ((IConvertible)_intValue).ToUInt16(provider);
        }

        public int ToInt32(IFormatProvider provider)
        {
            return ((IConvertible)_intValue).ToInt32(provider);
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            return ((IConvertible)_intValue).ToUInt32(provider);
        }

        public long ToInt64(IFormatProvider provider)
        {
            return ((IConvertible)_intValue).ToInt64(provider);
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            return ((IConvertible)_intValue).ToUInt64(provider);
        }

        public float ToSingle(IFormatProvider provider)
        {
            return ((IConvertible)_intValue).ToSingle(provider);
        }

        public double ToDouble(IFormatProvider provider)
        {
            return ((IConvertible)_intValue).ToDouble(provider);
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            return ((IConvertible)_intValue).ToDecimal(provider);
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            return ((IConvertible)_intValue).ToDateTime(provider);
        }

        public override string ToString()
        {
            return _enumDef.ValueToString(_intValue);
        }

        public int CompareTo(object obj)
        {
            if (obj is IConvertible)
            {
                return ToInt32(CultureInfo.CurrentCulture).CompareTo(((IConvertible)obj).ToInt32(CultureInfo.CurrentCulture));
            }
            return 0;
        }

        public string ToString(IFormatProvider provider)
        {
            return ToString();
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            return ((IConvertible)_intValue).ToType(conversionType, provider);
        }

        public EnumDef EnumDef
        {
            get { return _enumDef; }
        }
    }
}
