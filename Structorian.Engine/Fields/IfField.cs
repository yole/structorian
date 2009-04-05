using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Structorian.Engine.Fields
{
    class IfField: StructField
    {
        public IfField(StructDef structDef)
            : base(structDef, "expr", true)
        {
        }

        public override void LoadData(BinaryReader reader, StructInstance instance)
        {
            if (GetExpressionAttribute("expr").EvaluateBool(instance))
            {
                LoadChildFields(reader, instance);
            }
            else if (_linkedFields != null)
            {
                foreach(StructField field in _linkedFields)
                {
                    if (field is ElseIfField)
                    {
                        Expression expr = field.GetExpressionAttribute("expr");
                        bool result;
                        try
                        {
                             result = expr.EvaluateBool(instance);
                        }
                        catch (Exception e)
                        {
                            throw new LoadDataException("Error evaluating if condition: " + e.Message);
                        }
                        if (result)
                        {
                            field.LoadData(reader, instance);
                            break;
                        }
                    }
                    else if (field is ElseField)
                    {
                        field.LoadData(reader, instance);
                        break;
                    }
                }
            }
        }

        protected internal override bool CanLinkField(StructField nextField)
        {
            return nextField is ElseField || nextField is ElseIfField;
        }
    }
}
