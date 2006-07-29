using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Structorian.Engine.Fields
{
    class RepeatField: StructField
    {
        public RepeatField(StructDef structDef) : base(structDef, "count", true)
        {
        }

        public override void LoadData(BinaryReader reader, StructInstance instance)
        {
            int count = GetExpressionAttribute("count").EvaluateInt(instance);
            for(int i=0; i<count; i++)
            {
                LoadChildFields(reader, instance);
            }
        }
    }
}
