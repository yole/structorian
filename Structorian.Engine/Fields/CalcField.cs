using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Structorian.Engine.Fields
{
    class CalcField: StructField
    {
        public CalcField(StructDef structDef, bool hidden) : base(structDef)
        {
            _hidden = hidden;
        }

        public override void LoadData(BinaryReader reader, StructInstance instance)
        {
            AddCell(instance, GetExpressionAttribute("value").Evaluate(instance));
        }
    }
}
