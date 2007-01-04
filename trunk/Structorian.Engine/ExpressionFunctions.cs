using System;
using System.Collections.Generic;

namespace Structorian.Engine
{
    class ExpressionFunctions
    {
        private static Dictionary<String, EvaluateDelegate> _functionRegistry = InitializeFunctions();
        
        private static Dictionary<String, EvaluateDelegate> InitializeFunctions()
        {
            Dictionary<string, EvaluateDelegate> result = new Dictionary<string, EvaluateDelegate>(StringComparer.InvariantCultureIgnoreCase);
            result.Add("StructOffset", new EvaluateDelegate(StructOffset));
            result.Add("ParentCount", new EvaluateDelegate(ParentCount));
            result.Add("CurOffset", new EvaluateDelegate(CurOffset));
            result.Add("StructName", new EvaluateDelegate(StructName));
            result.Add("FileSize", new EvaluateDelegate(FileSize));
            result.Add("SizeOf", new EvaluateDelegate(SizeOf));
            return result;
        }
        
        public static IConvertible Evaluate(string function, Expression[] parameters, IEvaluateContext context)
        {
            EvaluateDelegate evalDelegate;
            if (!_functionRegistry.TryGetValue(function, out evalDelegate))
                return null;
            return evalDelegate(context, parameters);
        }

        private static IConvertible StructOffset(IEvaluateContext context, Expression[] parameters)
        {
            if (context is StructInstance)
            {
                return ((StructInstance) context).Offset;
            }
            throw new LoadDataException("Invalid StructOffset context");
        }

        private static IConvertible CurOffset(IEvaluateContext context, Expression[] parameters)
        {
            if (context is StructInstance)
            {
                return ((StructInstance)context).CurOffset;
            }
            throw new LoadDataException("Invalid CurOffset context");
        }

        private static IConvertible ParentCount(IEvaluateContext context, Expression[] parameters)
        {
            if (context is StructInstance)
            {
                return ((StructInstance)context).ParentCount;
            }
            throw new LoadDataException("Invalid ParentCount context");
        }

        private static IConvertible StructName(IEvaluateContext context, Expression[] parameters)
        {
            if (context is StructInstance)
            {
                return ((StructInstance)context).Def.Name;
            }
            throw new LoadDataException("Invalid StructName context");
        }

        private static IConvertible FileSize(IEvaluateContext context, Expression[] parameters)
        {
            if (context is StructInstance)
            {
                return ((StructInstance) context).Stream.Length;
            }
            throw new LoadDataException("Invalid FileSize context");
        }

        private static IConvertible SizeOf(IEvaluateContext context, Expression[] parameters)
        {
            if (parameters.Length != 1) 
                throw new LoadDataException("SizeOf function requires an argument");
            if (!(parameters [0] is SymbolExpression))
                throw new LoadDataException("SizeOf argument must be a symbol");
            string symbol = ((SymbolExpression) parameters[0]).Symbol;

            if (context is StructInstance)
            {
                StructFile structFile = ((StructInstance) context).Def.StructFile;
                StructDef def = structFile.GetStructByName(symbol);
                if (def == null) throw new LoadDataException("Structure '" + symbol + "' not found");
                return def.GetDataSize();
            }
            throw new LoadDataException("Invalid SizeOf context");
        }
    }
}
