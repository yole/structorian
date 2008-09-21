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
        }

        private static IConvertible EndsWith(IEvaluateContext context, Expression[] parameters)
        {
            string param1 = parameters[0].EvaluateString(context);
            string param2 = parameters[1].EvaluateString(context);
            return param1.EndsWith(param2);
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
            Register("StructOffset", StructOffset);
            Register("ParentCount", ParentCount);
            Register("CurOffset", CurOffset);
            Register("StructName", StructName);
            Register("FileSize", FileSize);
            Register("SizeOf", SizeOf);
            Register("ChildIndex", ChildIndex);
        }

        private static IConvertible StructOffset(StructInstance context, Expression[] parameters)
        {
            return context.Offset;
        }

        private static IConvertible CurOffset(StructInstance context, Expression[] parameters)
        {
            return context.CurOffset;
        }

        private static IConvertible ParentCount(StructInstance context, Expression[] parameters)
        {
            return context.ParentCount;
        }

        private static IConvertible StructName(StructInstance context, Expression[] parameters)
        {
            return context.Def.Name;
        }

        private static IConvertible FileSize(StructInstance context, Expression[] parameters)
        {
            return context.Stream.Length;
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

        private static IConvertible ChildIndex(StructInstance context, Expression[] parameters)
        {
            return context.ChildIndex;
        }
    }
}
