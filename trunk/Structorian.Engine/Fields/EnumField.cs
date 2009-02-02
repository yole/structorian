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

    internal class EnumValue: ConvertibleWrapper<uint>, IComparable
    {
        private readonly EnumDef _enumDef;

        public EnumValue(uint intValue, EnumDef enumDef) : base(intValue)
        {
            _enumDef = enumDef;
        }

        public override TypeCode GetTypeCode()
        {
            return TypeCode.Int32;
        }

        public override string ToString()
        {
            return _enumDef.ValueToString(_value);
        }

        public int CompareTo(object obj)
        {
            if (obj is IConvertible)
            {
                return ToInt32(CultureInfo.CurrentCulture).CompareTo(((IConvertible)obj).ToInt32(CultureInfo.CurrentCulture));
            }
            return 0;
        }

        public override string ToString(IFormatProvider provider)
        {
            return ToString();
        }

        public EnumDef EnumDef
        {
            get { return _enumDef; }
        }
    }
}
