using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Structorian.Engine.Fields
{
    class CalcField: StructField
    {
        private Expression _expr;
        
        public CalcField(StructDef structDef, bool hidden) : base(structDef)
        {
            _hidden = hidden;
        }

        public override void SetAttribute(string key, string value)
        {
            if (key == "value")
                _expr = ExpressionParser.Parse(value);
            else
                base.SetAttribute(key, value);
        }

        public override void LoadData(BinaryReader reader, StructInstance instance)
        {
            AddCell(instance, _expr.Evaluate(instance));
        }
    }
}
