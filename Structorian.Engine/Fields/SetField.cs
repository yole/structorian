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
            uint value = iValue.ToUInt32(CultureInfo.CurrentCulture);
            EnumDef enumDef = GetEnumAttribute("enum");

            StringBuilder result = new StringBuilder();
            for(int i=0; i<_size*8; i++)
            {
                if ((value & (1 << i)) != 0)
                {
                    if (result.Length > 0)
                        result.Append(", ");
                    result.Append(enumDef.ValueToString((uint) i));
                }
            }
            AddCell(instance, new EnumValue(value, result.ToString()), offset);
        }
    }
}
