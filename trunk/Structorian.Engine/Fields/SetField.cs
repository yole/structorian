using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Structorian.Engine.Fields
{
    class SetField: IntBasedField
    {
        public SetField(StructDef structDef, int size) : base(structDef, size, true)
        {
        }

        public override void LoadData(BinaryReader reader, StructInstance instance)
        {
            int offset = (int)reader.BaseStream.Position;
            IConvertible iValue = ReadIntValue(reader, instance);
            ulong value = iValue.ToUInt64(CultureInfo.CurrentCulture);
            EnumDef enumDef = GetEnumAttribute("enum");
            AddCell(instance, new SetValue((uint) value, enumDef, _size), offset);
        }
    }

    internal class SetValue: EnumValue
    {
        private readonly int _size;

        public SetValue(uint intValue, EnumDef enumDef, int size) : base(intValue, enumDef)
        {
            _size = size;
        }

        public override string ToString()
        {
            var result = new StringBuilder();
            for (int i = 0; i < _size * 8; i++)
            {
                if ((_value & (1ul << i)) != 0)
                {
                    if (result.Length > 0)
                        result.Append(", ");
                    result.Append(EnumDef.ValueToString((uint)i));
                }
            }
            return result.ToString();
        }
    }
}
