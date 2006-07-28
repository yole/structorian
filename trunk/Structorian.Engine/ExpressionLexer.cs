using System;
using System.Globalization;

namespace Structorian.Engine
{
    enum ExprTokenType { Symbol, Number, String, Plus,  Minus, Mult, Div, Dot, 
        Open, Close, EQ, NE, GT, GE, LT, LE, 
        AND, OR, NOT, BitAND, BitOR, EOF };
    
    class ExpressionLexer: BaseLexer<ExprTokenType, object>
    {
        public ExpressionLexer(string text): base(null, text, ExprTokenType.EOF)
        {
            InitTokens();
        }

        private void InitTokens()
        {
            RegisterCharToken('+', ExprTokenType.Plus);
            RegisterCharToken('-', ExprTokenType.Minus);
            RegisterCharToken('*', ExprTokenType.Mult);
            RegisterCharToken('/', ExprTokenType.Div);
            RegisterCharToken('.', ExprTokenType.Dot);
            RegisterCharToken('(', ExprTokenType.Open);
            RegisterCharToken(')', ExprTokenType.Close);
        }

        protected override Token FetchNextToken()
        {
            char c = _text[_position];
            if (c == '>')
                return FetchTwoCharToken('=', ExprTokenType.GT, ExprTokenType.GE);
            if (c == '<')
                return FetchTwoCharToken('=', ExprTokenType.LT, ExprTokenType.LE);
            if (c == '!')
                return FetchTwoCharToken('=', ExprTokenType.NOT, ExprTokenType.NE);
            if (c == '&')
                return FetchTwoCharToken('&', ExprTokenType.BitAND, ExprTokenType.AND);
            if (c == '|')
                return FetchTwoCharToken('|', ExprTokenType.BitOR, ExprTokenType.OR);
            if (c == '=')
            {
                _position++;
                if (_position >= _text.Length || _text[_position] != '=')
                    throw new ParseException("Unexpected character =", BuildTextPosition(_position - 1));
                _position++;
                return new Token(ExprTokenType.EQ);
            }
            if (Char.IsLetter(_text, _position) || _text [_position] == '_')
                return new Token(ExprTokenType.Symbol, ReadSymbol());
            if (Char.IsDigit(_text, _position))
                return new Token(ExprTokenType.Number, ReadNumber());
            if (c == '\"')
                return FetchDelimitedString('\"', '\0', ExprTokenType.String);

            throw new ParseException("Unknown character " + _text[_position], BuildTextPosition(_position));
        }

        private Token FetchTwoCharToken(char c, ExprTokenType token1, ExprTokenType token2)
        {
            _position++;
            if (_position < _text.Length && _text [_position] == c)
            {
                _position++;
                return new Token(token2);
            }
            return new Token(token1);
        }

        private int ReadNumber()
        {
            NumberStyles ns = NumberStyles.Integer;
            
            int startPos = _position;
            if (_position < _text.Length - 1 && _text[_position] == '0' &&
                (_text[_position + 1] == 'x' || _text[_position + 1] == 'X'))
            {
                _position += 2;
                startPos += 2;
                while (_position < _text.Length && IsHexChar(_text [_position]))
                    _position++;
                ns = NumberStyles.HexNumber;
            }
            else
            {
                while (_position < _text.Length && Char.IsDigit(_text, _position))
                    _position++;
            }
                
            return Int32.Parse(_text.Substring(startPos, _position - startPos), ns);
        }

        private static bool IsHexChar(char c)
        {
            return (c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' || c <= 'f');
        }

        private string ReadSymbol()
        {
            int startPos = _position;
            _position++;
            while(_position < _text.Length && 
                  (Char.IsLetterOrDigit(_text, _position) || _text [_position] == '_'))
            {
                _position++;
            }
            return _text.Substring(startPos, _position - startPos);
        }

        protected override object EvaluateStringValue(string stringValue)
        {
            return stringValue;
        }
    }
}
