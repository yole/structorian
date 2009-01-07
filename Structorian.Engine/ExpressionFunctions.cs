using System;

namespace Structorian.Engine
{
    class BaseFunctions: FunctionRegistry<IEvaluateContext>
    {
        private static BaseFunctions _instance;

        public static BaseFunctions Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new BaseFunctions();
                return _instance;
            }
        }

        private BaseFunctions()
        {
            Register("EndsWith", EndsWith);
            Register("Length", Length);
        }

        private static IConvertible EndsWith(IEvaluateContext context, Expression[] parameters)
        {
            string param1 = parameters[0].EvaluateString(context);
            string param2 = parameters[1].EvaluateString(context);
            return param1.EndsWith(param2);
        }

        private static IConvertible Length(IEvaluateContext context, Expression[] parameters)
        {
            string param = parameters[0].EvaluateString(context);
            return param.Length;
        }
    }

    class StructFunctions: FunctionRegistry<StructInstance>
    {
        private static StructFunctions _instance;

        public static StructFunctions Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new StructFunctions();
                return _instance;
            }
        }

        private StructFunctions()
        {
            Register("StructOffset", (context, parameters) => context.Offset);
            Register("EndOffset", (context, parameters) => context.EndOffset);
            Register("ParentCount", (context, parameters) => context.ParentCount);
            Register("CurOffset", (context, parameters) => context.CurOffset);
            Register("StructName", (context, parameters) => context.Def.Name);
            Register("FileSize", (context, parameters) => context.Stream.Length);
            Register("SizeOf", SizeOf);
            Register("ChildIndex", (context, parameters) => context.ChildIndex);
        }

        private static IConvertible SizeOf(StructInstance context, Expression[] parameters)
        {
            if (parameters.Length != 1) 
                throw new LoadDataException("SizeOf function requires an argument");
            if (!(parameters [0] is SymbolExpression))
                throw new LoadDataException("SizeOf argument must be a symbol");
            string symbol = ((SymbolExpression) parameters[0]).Symbol;

            StructFile structFile = context.Def.StructFile;
            StructDef def = structFile.GetStructByName(symbol);
            if (def == null) throw new LoadDataException("Structure '" + symbol + "' not found");
            return def.GetDataSize();
        }
    }
}
