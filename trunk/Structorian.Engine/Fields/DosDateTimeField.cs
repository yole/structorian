using System;
using System.IO;

namespace Structorian.Engine.Fields
{
    class DosDateTimeField: StructField
    {
        public DosDateTimeField(StructDef structDef) : base(structDef)
        {
        }

        public override void LoadData(BinaryReader reader, StructInstance instance)
        {
            ushort dosDate, dosTime;
            if (GetBoolAttribute("timefirst"))
            {
                dosTime = reader.ReadUInt16();
                dosDate = reader.ReadUInt16();
            }
            else
            {
                dosDate = reader.ReadUInt16();
                dosTime = reader.ReadUInt16();
            }

            int day = dosDate & 0x1F;
            int month = (dosDate >> 5) & 0x0F;
            int year = 1980 + ((dosDate >> 9) & 0x7F);
            int second = (dosTime & 0x1F) * 2;
            int minute = (dosTime >> 5) & 0x3F;
            int hour = (dosTime >> 11) & 0x1F;
            AddCell(instance, new DateTime(year, month, day, hour, minute, second));
        }
    }
}
