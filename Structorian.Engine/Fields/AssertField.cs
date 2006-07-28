using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Structorian.Engine.Fields
{
    class AssertField: StructField
    {
        private Expression _expr;
        
        public AssertField(StructDef structDef) : base(structDef, "expr", false)
        {
        }

        public override void SetAttribute(string key, string value)
        {
            if (key == "expr")
                _expr = ExpressionParser.Parse(value);
            else
                base.SetAttribute(key, value);
        }

        public override void LoadData(BinaryReader reader, StructInstance instance)
        {
            if (!_expr.EvaluateBool(instance))
                throw new LoadDataException("Assertion failed: " + _expr.ToString());
        }
    }
}
