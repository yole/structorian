using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Structorian.Engine.Fields
{
    class WhileField: StructField
    {
        public WhileField(StructDef structDef) : base(structDef, "expr", true)
        {
        }

        public override void LoadData(BinaryReader reader, StructInstance instance)
        {
            while (GetExpressionAttribute("expr").EvaluateBool(instance))
            {
                try
                {
                    LoadChildFields(reader, instance);
                }
                catch (BreakRepeatException)
                {
                    break;
                }
            }
        }
    }
}
