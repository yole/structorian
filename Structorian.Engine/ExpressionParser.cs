using System;

namespace Structorian.Engine
{
    public class ExpressionParser
    {
        private static ExprTokenType[] _condComboTokens = new ExprTokenType[]
            {
                ExprTokenType.AND, ExprTokenType.OR
            };
        private static ExprTokenType[] _condTokens = new ExprTokenType[]
            {
                ExprTokenType.GT, ExprTokenType.GE, ExprTokenType.EQ, ExprTokenType.NE,
                ExprTokenType.LT, ExprTokenType.LE
            };
        private static ExprTokenType[] _exprTokens = new ExprTokenType[]
            {
                ExprTokenType.Plus, ExprTokenType.Minus
            };
        private static ExprTokenType[] _termTokens = new ExprTokenType[]
            {
                ExprTokenType.Mult, ExprTokenType.Div
            };
        
        private delegate Expression StepDownDelegate(ExpressionLexer lexer);
        private delegate Expression CreateBinaryDelegate(ExprTokenType operation, Expression lhs, Expression rhs);
        
        public static Expression Parse(string source)
        {
            ExpressionLexer lexer = new ExpressionLexer(source);

            Expression result = ParseCondCombo(lexer);
            lexer.GetNextToken(ExprTokenType.EOF);
            result.Source = source;
            return result;
        }

        private static Expression RecursiveDescentStep(ExpressionLexer lexer, ExprTokenType[] tokens,
            StepDownDelegate stepDown, CreateBinaryDelegate createBinary)
        {
            Expression expr = stepDown(lexer);
            while (true)
            {
                ExprTokenType? token = lexer.CheckNextToken(tokens);
                if (token.HasValue)
                    expr = createBinary(token.Value, expr, stepDown(lexer));
                else
                    break;
            }
            return expr;
        }

        private static Expression ParseCondCombo(ExpressionLexer lexer)
        {
            return RecursiveDescentStep(lexer, _condComboTokens,
                delegate(ExpressionLexer l) { return ParseCond(l); },
                delegate(ExprTokenType o, Expression lhs, Expression rhs) { return new LogicalExpression(o, lhs, rhs); });
        }

        private static Expression ParseCond(ExpressionLexer lexer)
        {
            return RecursiveDescentStep(lexer, _condTokens,
                delegate(ExpressionLexer l) { return ParseExpr(l); },
                delegate(ExprTokenType o, Expression lhs, Expression rhs) { return new CompareExpression(o, lhs, rhs);} );
        }

        private static Expression ParseExpr(ExpressionLexer lexer)
        {
            return RecursiveDescentStep(lexer, _exprTokens,
                delegate(ExpressionLexer l) { return ParseTerm(l); },
                delegate(ExprTokenType o, Expression lhs, Expression rhs) { return new BinaryExpression(o, lhs, rhs); });
        }
        
        private static Expression ParseTerm(ExpressionLexer lexer)
        {
            return RecursiveDescentStep(lexer, _termTokens,
                delegate(ExpressionLexer l) { return ParseFactor(l); },
                delegate(ExprTokenType o, Expression lhs, Expression rhs) { return new BinaryExpression(o, lhs, rhs); });
        }
        
        private static Expression ParseFactor(ExpressionLexer lexer)
        {
            ExprTokenType tokenType = lexer.PeekNextToken();
            if (tokenType == ExprTokenType.Number)
                return new PrimitiveExpression((int)lexer.GetNextToken(ExprTokenType.Number));
            if (tokenType == ExprTokenType.String)
                return new PrimitiveExpression((string)lexer.GetNextToken(ExprTokenType.String));
            if (tokenType == ExprTokenType.Symbol)
            {
                string symbol = (string) lexer.GetNextToken(ExprTokenType.Symbol);
                if (lexer.PeekNextToken() == ExprTokenType.Dot)
                {
                    lexer.GetNextToken(ExprTokenType.Dot);
                    Expression exprInContext = ParseFactor(lexer);
                    return new ContextExpression(symbol, exprInContext);
                }
                else
                    return new SymbolExpression(symbol);
            }
            if (tokenType == ExprTokenType.Open)
            {
                lexer.GetNextToken(ExprTokenType.Open);
                Expression result = ParseCondCombo(lexer);
                lexer.GetNextToken(ExprTokenType.Close);
                return result;
            }
            if (tokenType == ExprTokenType.Minus)
            {
                lexer.GetNextToken(tokenType);
                return new UnaryExpression(ParseFactor(lexer), tokenType);
            }
            throw new ParseException("Unexpected token " + tokenType, lexer.CurrentPosition);
        }
    }
}
