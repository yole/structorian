using System;
using System.Globalization;

namespace Structorian.Engine
{
    public interface IEvaluateContext
    {
        IConvertible EvaluateSymbol(string symbol);
        IConvertible EvaluateFunction(string symbol, Expression[] parameters);
        IEvaluateContext EvaluateContext(string symbol, Expression[] parameters);
    }
    
    public delegate IConvertible EvaluateDelegate(IEvaluateContext context, Expression[] parameters);
    
    public abstract class Expression
    {
        private string _source;
        public abstract IConvertible Evaluate(IEvaluateContext context);
        
        public int EvaluateInt(IEvaluateContext context)
        {
            return Evaluate(context).ToInt32(CultureInfo.CurrentCulture);
        }

        public long EvaluateLong(IEvaluateContext context)
        {
            return Evaluate(context).ToInt64(CultureInfo.CurrentCulture);
        }

        public bool EvaluateBool(IEvaluateContext context)
        {
            return Evaluate(context).ToBoolean(CultureInfo.CurrentCulture);
        }

        public string EvaluateString(IEvaluateContext context)
        {
            return Evaluate(context).ToString(CultureInfo.CurrentCulture);
        }
        
        public IComparable EvaluateComparable(IEvaluateContext context)
        {
            IConvertible result = Evaluate(context);
            if (result.GetTypeCode() == TypeCode.String)
                return result.ToString(CultureInfo.CurrentCulture);
            else if (result.GetTypeCode() == TypeCode.UInt32)
            {
                uint value = result.ToUInt32(CultureInfo.CurrentCulture);
                return (int) value;
            }
            else
                return result.ToInt32(CultureInfo.CurrentCulture);
        }

        public string Source
        {
            get { return _source; }
            set { _source = value; }
        }
        
        public virtual bool IsConstant
        {
            get { return false; }
        }

        public override string ToString()
        {
            if (_source != null) return _source;
            return base.ToString();
        }
    }
    
    public class PrimitiveExpression: Expression
    {
        private IConvertible _value;

        internal PrimitiveExpression(IConvertible value)
        {
            _value = value;
        }

        public override IConvertible Evaluate(IEvaluateContext context)
        {
            return _value;
        }

        public override bool IsConstant
        {
            get { return true; }
        }
    }
    
    public class SymbolExpression: Expression
    {
        private readonly string _symbol;

        public SymbolExpression(string symbol)
        {
            _symbol = symbol;
        }

        public override IConvertible Evaluate(IEvaluateContext context)
        {
            return context.EvaluateSymbol(_symbol);
        }

        public string Symbol
        {
            get { return _symbol; }
        }
    }
    
    class BinaryExpression: Expression
    {
        protected ExprTokenType _operation;
        protected Expression _lhs;
        protected Expression _rhs;

        public BinaryExpression(ExprTokenType operation, Expression lhs, Expression rhs)
        {
            _operation = operation;
            _lhs = lhs;
            _rhs = rhs;
        }

        public override IConvertible Evaluate(IEvaluateContext context)
        {
            IConvertible lhsValue = _lhs.Evaluate(context);
            IConvertible rhsValue = _rhs.Evaluate(context);
            CultureInfo culture = CultureInfo.CurrentCulture;
            if (_operation == ExprTokenType.Plus)
            {
                if (lhsValue.GetTypeCode() == TypeCode.String || rhsValue.GetTypeCode() == TypeCode.String)
                    return lhsValue.ToString(culture) + rhsValue.ToString(culture);
            }

            int lhs = lhsValue.ToInt32(culture);
            int rhs = rhsValue.ToInt32(culture);
            switch(_operation)
            {
                case ExprTokenType.Plus: return lhs + rhs;
                case ExprTokenType.Minus: return lhs - rhs;
                case ExprTokenType.Mult: return lhs * rhs;
                case ExprTokenType.Div: return lhs / rhs;
                case ExprTokenType.Mod: return lhs % rhs;
                case ExprTokenType.BitAND: return lhs & rhs;
                case ExprTokenType.BitOR: return lhs | rhs;
                case ExprTokenType.SHL: return lhs << rhs;
                case ExprTokenType.SHR: return lhs >> rhs;
            }
            throw new Exception("Unknown binary operation");
        }
    }
    
    class CompareExpression: BinaryExpression
    {
        public CompareExpression(ExprTokenType operation, Expression lhs, Expression rhs)
            : base(operation, lhs, rhs)
        {
        }

        public override IConvertible Evaluate(IEvaluateContext context)
        {
            IComparable lhs = _lhs.EvaluateComparable(context);
            IComparable rhs = _rhs.EvaluateComparable(context);
            int result = lhs.CompareTo(rhs);
            switch(_operation)
            {
                case ExprTokenType.EQ: return result == 0;
                case ExprTokenType.NE: return result != 0;
                case ExprTokenType.GT: return result > 0;
                case ExprTokenType.GE: return result >= 0;
                case ExprTokenType.LT: return result < 0;
                case ExprTokenType.LE: return result <= 0;
            }
            throw new Exception("Unknown compare operation");
        }
    }
    
    class LogicalExpression: BinaryExpression
    {
        public LogicalExpression(ExprTokenType operation, Expression lhs, Expression rhs)
            : base(operation, lhs, rhs)
        {
        }

        public override IConvertible Evaluate(IEvaluateContext context)
        {
            bool lhs = _lhs.EvaluateBool(context);
            if (_operation == ExprTokenType.AND && !lhs)
                return false;
            if (_operation == ExprTokenType.OR && lhs)
                return true;

            bool rhs = _rhs.EvaluateBool(context);
            switch(_operation)
            {
                case ExprTokenType.AND: return lhs && rhs;
                case ExprTokenType.OR: return lhs || rhs;
            }
            throw new Exception("Unknown logical operation");
        }
    }

    class ContextExpression: Expression
    {
        private readonly string _contextExpr;
        private readonly Expression _expr;
        private readonly Expression[] _parameters;

        public ContextExpression(string contextExpr, Expression expr, Expression[] parameters)
        {
            _contextExpr = contextExpr;
            _expr = expr;
            _parameters = parameters;
        }

        public override IConvertible Evaluate(IEvaluateContext context)
        {
            IEvaluateContext ctx = context.EvaluateContext(_contextExpr, _parameters);
            return _expr.Evaluate(ctx);
        }
    }
    
    class UnaryExpression: Expression
    {
        private readonly Expression _operand;
        private readonly ExprTokenType _operation;

        public UnaryExpression(Expression operand, ExprTokenType operation)
        {
            _operand = operand;
            _operation = operation;
        }

        public override IConvertible Evaluate(IEvaluateContext context)
        {
            switch(_operation)
            {
                case ExprTokenType.Minus:
                    return -_operand.EvaluateInt(context);
                case ExprTokenType.NOT:
                    return !_operand.EvaluateBool(context);
            }
            throw new Exception("Unexpected unary operation " + _operation);
        }
    }
    
    class FunctionExpression: Expression
    {
        private readonly string _function;
        private readonly Expression[] _parameters;

        public FunctionExpression(string function, Expression[] parameters)
        {
            _function = function;
            _parameters = parameters;
        }

        public override IConvertible Evaluate(IEvaluateContext context)
        {
            IConvertible baseResult = BaseFunctions.Instance.Evaluate(_function, _parameters, context);
            if (baseResult != null)
                return baseResult;
            if (context != null)
                return context.EvaluateFunction(_function, _parameters);
            throw new Exception("Function '" + _function + "' not found in current context");
        }
    }
}
