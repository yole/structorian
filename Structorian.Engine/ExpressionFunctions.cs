using System;

namespace Structorian.Engine
{
    class ExpressionFunctions: FunctionRegistry<StructInstance>
    {
        private static ExpressionFunctions _instance;

        public static ExpressionFunctions Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ExpressionFunctions();
                return _instance;
            }
        }

        private ExpressionFunctions()
        {
            Register("StructOffset", new FunctionDelegate(StructOffset));
            Register("ParentCount", new FunctionDelegate(ParentCount));
            Register("CurOffset", new FunctionDelegate(CurOffset));
            Register("StructName", new FunctionDelegate(StructName));
            Register("FileSize", new FunctionDelegate(FileSize));
            Register("SizeOf", new FunctionDelegate(SizeOf));
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
    }
}
