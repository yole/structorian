using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Structorian.Engine.Fields
{
    class GlobalField: StructField
    {
        public GlobalField(StructDef structDef) : base(structDef)
        {
        }

        public override void LoadData(BinaryReader reader, StructInstance instance)
        {
            int result = GetExpressionAttribute("value").EvaluateInt(instance);
            instance.RegisterGlobal(Id, result);
        }
    }
}
