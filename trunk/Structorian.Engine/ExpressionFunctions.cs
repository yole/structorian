using System;
using System.Collections.Generic;

namespace Structorian.Engine
{
    class ExpressionFunctions
    {
        private static Dictionary<String, EvaluateDelegate> _functionRegistry = InitializeFunctions();
        
        private static Dictionary<String, EvaluateDelegate> InitializeFunctions()
        {
            Dictionary<string, EvaluateDelegate> result = new Dictionary<string, EvaluateDelegate>();
            result.Add("StructOffset", new EvaluateDelegate(StructOffset));
            result.Add("ParentCount", new EvaluateDelegate(ParentCount));
            result.Add("CurOffset", new EvaluateDelegate(CurOffset));
            return result;
        }
        
        public static IConvertible Evaluate(string function, IEvaluateContext context)
        {
            EvaluateDelegate evalDelegate;
            if (!_functionRegistry.TryGetValue(function, out evalDelegate))
                return null;
            return evalDelegate(context, null);
        }
        
        private static IConvertible StructOffset(IEvaluateContext context, string param)
        {
            if (context is StructInstance)
            {
                return ((StructInstance) context).Offset;
            }
            throw new LoadDataException("Invalid StructOffset context");
        }

        private static IConvertible CurOffset(IEvaluateContext context, string param)
        {
            if (context is StructInstance)
            {
                return ((StructInstance)context).CurOffset;
            }
            throw new LoadDataException("Invalid StructOffset context");
        }

        private static IConvertible ParentCount(IEvaluateContext context, string param)
        {
            if (context is StructInstance)
            {
                return ((StructInstance)context).ParentCount;
            }
            throw new LoadDataException("Invalid StructOffset context");
        }
    }
}
