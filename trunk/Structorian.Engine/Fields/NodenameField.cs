using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Structorian.Engine.Fields
{
    class NodenameField: StructField
    {
        public NodenameField(StructDef structDef) : base(structDef, "name", false)
        {
        }

        public override void LoadData(BinaryReader reader, StructInstance instance)
        {
            instance.SetNodeName(GetExpressionAttribute("name").EvaluateString(instance));
        }
    }
}
