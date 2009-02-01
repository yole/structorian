using System;

namespace Structorian.Engine
{
    public enum StructTokenType
    {
        EOF, String, OpenCurly, CloseCurly, Semicolon, OpenSquare, CloseSquare, Equals, Comma
    }
    
    public class StructLexer: BaseLexer<StructTokenType, string>
    {
        public StructLexer(string text)
            : base(null, text, StructTokenType.EOF)
        {
            InitTokens();
        }

        public StructLexer(string fileName, string text)
            : base(fileName, text, StructTokenType.EOF)
        {
            InitTokens();
        }

        private void InitTokens()
        {
            RegisterCharToken('{', StructTokenType.OpenCurly);
            RegisterCharToken('}', StructTokenType.CloseCurly);
            RegisterCharToken(';', StructTokenType.Semicolon);
            RegisterCharToken('[', StructTokenType.OpenSquare);
            RegisterCharToken(']', StructTokenType.CloseSquare);
            RegisterCharToken('=', StructTokenType.Equals);
            RegisterCharToken(',', StructTokenType.Comma);
        }

        protected override Token FetchNextToken()
        {
            if (Char.IsLetterOrDigit(_text, _position) || _text[_position] == '_' || _text[_position] == '-')
                return GrabPlainString();
            else if (_text[_position] == '\"')
                return FetchDelimitedString('\"', '\0', StructTokenType.String);
            else if (_text[_position] == '(')
                return FetchDelimitedString(')', '(', StructTokenType.String);
            else if (_text[_position] == '/')
            {
                GrabComment();
                return null;
            }
            throw new ParseException("Unexpected character " + _text[_position], BuildTextPosition(_position));
        }

        protected override string EvaluateStringValue(string stringValue)
        {
            return stringValue;
        }

        private Token GrabPlainString()
        {
            int startPos = _position;
            while(_position < _text.Length && 
                  (Char.IsLetterOrDigit(_text, _position) || IsStringPunctuation(_text [_position])))
            {
                _position++;
            }
            return new Token(StructTokenType.String, _text.Substring(startPos, _position - startPos));
        }

        private bool IsStringPunctuation(char c)
        {
            switch(c)
            {
                case '_': case '+': case '-': case '*': case '/': case '!': case '<': case '>':
                case '&': case '|': case '.':
                    return true;
                default:
                    return false;
            }
        }
        
        private void GrabComment()
        {
            if (_position == _text.Length - 1 || (_text[_position + 1] != '/' && _text[_position + 1] != '*'))
                throw new ParseException("Unexpected character '/'", BuildTextPosition(_position));
            if (_text [_position+1] == '*')
            {
                _position += 2;
                SkipWhile(() => _position < _text.Length-1 && (_text [_position] != '*' || _text [_position+1] != '/'));
                _position += 2;
                if (_position >= _text.Length-1)
                    throw new ParseException("Unclosed comment", BuildTextPosition(_position - 2));
            }
            else
            {
                _position += 2;
                while (_position < _text.Length && _text[_position] != '\r' && _text[_position] != '\n')
                    _position++;
            }
        }

        public bool EndOfStream()
        {
            return NextToken.TokenType == StructTokenType.EOF;
        }
        
        public string GetAttributeValue(out TextPosition pos)
        {
            pos = BuildTextPosition(_position);
            int startPosition = _position;
            int parenCount = 0;
            while(_position < _text.Length)
            {
                if (_text[_position] == '(')
                {
                    parenCount++;
                }
                else if (_text[_position] == ')')
                {
                    parenCount--;
                }
                else if (parenCount == 0 && IsAttributeEnd(_text [_position]))
                {
                    break;
                }
                _position++;
            }

            if (_position >= _text.Length || (_text[_position] != ',' && _text[_position] != ']'))
                throw new ParseException("Unclosed attribute value", BuildTextPosition(startPosition));
            
            string result = _text.Substring(startPosition, _position - startPosition).Trim();
            if (result.StartsWith("\"") && result.EndsWith("\"") && result.Length >= 2)
                result = result.Substring(1, result.Length - 2);
            ConsumeNextToken();
            return result;
        }

        private bool IsAttributeEnd(char c)
        {
            switch(c)
            {
                case '\n': case '\r': case ',': case ']': return true;
                default: return false;                    
            }
        }
    }
}
