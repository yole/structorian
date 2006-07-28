using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Structorian.Engine.Fields
{
    class RepeatField: StructField
    {
        private Expression _countExpr;
        
        public RepeatField(StructDef structDef) : base(structDef, "count", true)
        {
        }

        public override void SetAttribute(string key, string value)
        {
            if (key == "count")
                _countExpr = ExpressionParser.Parse(value);
            else
                base.SetAttribute(key, value);
        }

        public override void LoadData(BinaryReader reader, StructInstance instance)
        {
            int count = _countExpr.EvaluateInt(instance);
            for(int i=0; i<count; i++)
            {
                LoadChildFields(reader, instance);
            }
        }
    }
}
