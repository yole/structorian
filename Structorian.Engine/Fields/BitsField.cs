using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Structorian.Engine.Fields
{
    class BitsField: IntBasedField
    {
        public BitsField(StructDef structDef, int size) : base(structDef, size, true)
        {
        }

        public override void LoadData(BinaryReader reader, StructInstance instance)
        {
            int offset = (int) reader.BaseStream.Position;
            IConvertible value = ReadIntValue(reader, instance);
            ulong u = value.ToUInt64(CultureInfo.CurrentCulture);
            StringBuilder displayValue = new StringBuilder(_size * 8);
            for(int i=_size * 8 - 1; i >= 0; i--)
            {
                if ((u & (1ul << i)) != 0)
                    displayValue.Append("1");
                else
                    displayValue.Append("0");
            }
            AddCell(instance, value, displayValue.ToString(), offset);
        }
    }
}
