using System;
using System.IO;

namespace Structorian.Engine.Fields
{
    class CalcField: StructField
    {
        public CalcField(StructDef structDef, bool hidden) : base(structDef)
        {
            _hidden = hidden;
        }

        public override void LoadData(BinaryReader reader, StructInstance instance)
        {
            var expression = GetExpressionAttribute("value");
            if (IsEagerEval(expression))
            {
                var value = expression.Evaluate(instance);
                AddCell(instance, value, -1);
            }
            else
            {
                AddCell(instance, expression);
            }
        }

        private static bool IsEagerEval(Expression expression)
        {
            return expression.Source.ToLower().Contains("curoffset");
        }
    }
}
