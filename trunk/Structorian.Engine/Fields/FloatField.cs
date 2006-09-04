using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Structorian.Engine.Fields
{
    class FloatField: StructField
    {
        public FloatField(StructDef structDef) : base(structDef)
        {
        }

        public override void LoadData(BinaryReader reader, StructInstance instance)
        {
            int offset = (int)reader.BaseStream.Position;
            float value = reader.ReadSingle();
            AddCell(instance, value, offset);
        }

        public override int GetDataSize()
        {
            return 4;
        }
    }
}
