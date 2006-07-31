using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Structorian.Engine.Fields
{
    class AlignField: StructField
    {
        public AlignField(StructDef structDef) : base(structDef, "bytes", false)
        {
        }

        public override void LoadData(BinaryReader reader, StructInstance instance)
        {
            int bytes = GetIntAttribute("bytes").Value;
            if (bytes > 1 && reader.BaseStream.Position % bytes != 0)
                reader.BaseStream.Position = ((reader.BaseStream.Position/bytes) + 1)*bytes;
        }
    }
}
