using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Structorian.Engine.Fields
{
    class IfField: StructField
    {
        private Expression _expr;
        
        public IfField(StructDef structDef)
            : base(structDef, "expr", true)
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
            if (_expr.EvaluateBool(instance))
            {
                LoadChildFields(reader, instance);
            }
        }
    }
}
