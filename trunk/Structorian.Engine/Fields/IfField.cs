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
            else if (_linkedFields != null)
            {
                foreach(StructField field in _linkedFields)
                {
                    if (field is ElseField)
                    {
                        field.LoadData(reader, instance);
                        break;
                    }
                }
            }
        }

        protected internal override bool CanLinkField(StructField nextField)
        {
            return nextField is ElseField;
        }
    }
}
