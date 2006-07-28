using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Structorian.Engine.Fields
{
    class ElseField: StructField
    {
        public ElseField(StructDef structDef) : base(structDef, null, true)
        {
        }

        public override void LoadData(BinaryReader reader, StructInstance instance)
        {
            LoadChildFields(reader, instance);
        }
    }
}
