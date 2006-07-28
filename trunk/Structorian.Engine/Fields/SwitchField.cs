using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Structorian.Engine.Fields
{
    class SwitchField: StructField
    {
        private Expression _switchExpr;
        
        public SwitchField(StructDef structDef) 
            : base(structDef, "expr", true)
        {
        }

        public override void SetAttribute(string key, string value)
        {
            if (key == "expr")
                _switchExpr = ExpressionParser.Parse(value);
            else
                base.SetAttribute(key, value);
        }

        public override void LoadData(BinaryReader reader, StructInstance instance)
        {
            IComparable switchValue = _switchExpr.EvaluateComparable(instance);
            CaseField defaultField = null;
            foreach(CaseField field in ChildFields)
            {
                if (field.IsDefault)
                    defaultField = field;
                else
                {
                    IComparable caseValue = field.CaseExpression.EvaluateComparable(instance);
                    if (switchValue.CompareTo(caseValue) == 0)
                    {
                        field.LoadData(reader, instance);
                        defaultField = null;
                        break;
                    }
                }
            }
            if (defaultField != null)
                defaultField.LoadData(reader, instance);
        }
    }
}
