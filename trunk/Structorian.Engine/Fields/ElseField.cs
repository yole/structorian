using System;
using System.IO;

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

        public override void Validate()
        {
            if (!IsLinked)
                throw new ParseException("'else' without 'if'", Position);
        }
    }
}
