using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Structorian.Engine.Fields
{
    class NodenameField: StructField
    {
        private Expression _nameExpr;
        
        public NodenameField(StructDef structDef) : base(structDef, "name", false)
        {
        }

        public override void SetAttribute(string key, string value)
        {
            if (key == "name")
                _nameExpr = ExpressionParser.Parse(value);
            else
                base.SetAttribute(key, value);
        }

        public override void LoadData(BinaryReader reader, StructInstance instance)
        {
            instance.SetNodeName(_nameExpr.EvaluateString(instance));
        }
    }
}
