using System;
using System.Globalization;

namespace Structorian.Engine
{
    enum ExprTokenType { Symbol, Number, String, Plus,  Minus, Mult, Div, Mod, Dot, 
        Open, Close, EQ, NE, GT, GE, LT, LE, 
        AND, OR, NOT, BitAND, BitOR, SHL, SHR, EOF };
    
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
            RegisterCharToken('%', ExprTokenType.Mod);
            RegisterCharToken('.', ExprTokenType.Dot);
            RegisterCharToken('(', ExprTokenType.Open);
            RegisterCharToken(')', ExprTokenType.Close);
        }

        protected override Token FetchNextToken()
        {
            char c = _text[_position];
            if (c == '>')
                return FetchTwoCharToken('=', ExprTokenType.GE, '>', ExprTokenType.SHR, ExprTokenType.GT);
            if (c == '<')
                return FetchTwoCharToken('=', ExprTokenType.LE, '<', ExprTokenType.SHL, ExprTokenType.LT);
            if (c == '!')
                return FetchTwoCharToken('=', ExprTokenType.NE, ExprTokenType.NOT);
            if (c == '&')
                return FetchTwoCharToken('&', ExprTokenType.AND, ExprTokenType.BitAND);
            if (c == '|')
                return FetchTwoCharToken('|', ExprTokenType.OR, ExprTokenType.BitOR);
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
                return new Token(token1);
            }
            return new Token(token2);
        }

        private Token FetchTwoCharToken(char c, ExprTokenType token1, char c2, ExprTokenType token2, ExprTokenType token3)
        {
            _position++;
            if (_position < _text.Length)
            {
                if (_text[_position] == c)
                {
                    _position++;
                    return new Token(token1);
                }
                if (_text[_position] == c2)
                {
                    _position++;
                    return new Token(token2);
                }
            }

            return new Token(token3);
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

            string valueString = _text.Substring(startPos, _position - startPos);
            try
            {
                return Int32.Parse(valueString, ns);
            }
            catch(FormatException)
            {
                throw new ParseException("Invalid number format in string " + valueString,
                                         BuildTextPosition(startPos), _position - startPos);
            }
        }

        private static bool IsHexChar(char c)
        {
            return (c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f');
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
