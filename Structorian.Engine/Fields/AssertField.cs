using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Structorian.Engine.Fields
{
    class AssertField: StructField
    {
        public AssertField(StructDef structDef) : base(structDef, "expr", false)
        {
        }

        public override void LoadData(BinaryReader reader, StructInstance instance)
        {
            Expression expr = GetExpressionAttribute("expr");
            if (!expr.EvaluateBool(instance))
                throw new LoadDataException("Assertion failed: " + expr.ToString());
        }
    }
}
