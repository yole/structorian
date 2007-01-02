using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Structorian.Engine.Fields
{
    class ElseIfField: StructField
    {
        public ElseIfField(StructDef structDef)
            : base(structDef, "expr", true)
        {
        }

        public override void LoadData(BinaryReader reader, StructInstance instance)
        {
            LoadChildFields(reader, instance);
        }

        public override void Validate()
        {
            if (!IsLinked)
                throw new ParseException("'elif' without 'if'", Position);
        }
    }
}
