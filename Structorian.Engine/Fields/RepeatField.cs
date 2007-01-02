using System;
using System.IO;

namespace Structorian.Engine.Fields
{
    class RepeatField: StructField
    {
        public RepeatField(StructDef structDef) : base(structDef, "count", true)
        {
        }

        public override void LoadData(BinaryReader reader, StructInstance instance)
        {
            int count = GetExpressionAttribute("count").EvaluateInt(instance);
            for(int i=0; i<count; i++)
            {
                try
                {
                    LoadChildFields(reader, instance);
                }
                catch(BreakRepeatException)
                {
                    break;
                }
            }
        }

        public override int GetDataSize()
        {
            Expression expression = GetExpressionAttribute("count");
            if (expression.IsConstant)
            {
                int repeatCount = expression.EvaluateInt(null);
                int repeatSize = 0;
                foreach (StructField field in ChildFields)
                {
                    if (!field.IsLinked)
                        repeatSize += field.GetDataSize();
                }
                return repeatCount*repeatSize;
            }
            return base.GetDataSize();
        }
    }
}
