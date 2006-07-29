using System;
using System.IO;

namespace Structorian.Engine.Fields
{
    class SwitchField: StructField
    {
        public SwitchField(StructDef structDef) 
            : base(structDef, "expr", true)
        {
        }

        public override void LoadData(BinaryReader reader, StructInstance instance)
        {
            IComparable switchValue = GetExpressionAttribute("expr").EvaluateComparable(instance);
            CaseField defaultField = null;
            foreach(CaseField field in ChildFields)
            {
                if (field.IsDefault)
                    defaultField = field;
                else
                {
                    IComparable caseValue = field.GetExpressionAttribute("expr").EvaluateComparable(instance);
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
