using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Structorian.Engine.Fields
{
    class GlobalField: StructField
    {
        private Expression _valueExpr;
        
        public GlobalField(StructDef structDef) : base(structDef)
        {
        }

        public override void SetAttribute(string key, string value)
        {
            if (key == "value")
                _valueExpr = ExpressionParser.Parse(value);
            else
                base.SetAttribute(key, value);
        }

        public override void LoadData(BinaryReader reader, StructInstance instance)
        {
            int result = _valueExpr.EvaluateInt(instance);
            instance.RegisterGlobal(Id, result);
        }
    }
}
