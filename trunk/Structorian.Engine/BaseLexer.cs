using System;

namespace Structorian.Engine
{
    public abstract class BaseLexer<T, V> where T: struct
    {
        public class Token
        {
            T _tokenType;
            V _tokenValue;

            public Token(T tokenType)
            {
                _tokenType = tokenType;
            }

            public Token(T tokenType, V tokenValue)
            {
                _tokenType = tokenType;
                _tokenValue = tokenValue;
            }

            public T TokenType
            {
                get { return _tokenType; }
            }

            public V TokenValue
            {
                get { return _tokenValue; }
            }
        }

        protected string _fileName;
        protected string _text;
        protected int _position;
        private Token _nextToken;
        private int _nextTokenStartPosition;
        private int _nextTokenEndPosition;
        private int _curLine = 1;
        private int _curLineStart;
        private T _eofTokenType;
        private Token[] _charTokens = new Token[128];
        
        protected BaseLexer(string fileName, string text, T eofToken)
        {
            _fileName = fileName;
            _text = text;
            _position = 0;
            _nextToken = null;
            _eofTokenType = eofToken;
        }
        
        protected void RegisterCharToken(char c, T tokenType)
        {
            _charTokens[c] = new Token(tokenType);
        }
        
        protected Token NextToken
        {
            get 
            {
                if (_nextToken == null) _nextToken = ParseNextToken();
                return _nextToken;
            }
        }
        
        protected void ConsumeNextToken()
        {
            _nextToken = null;
        }

        protected virtual Token ParseNextToken()
        {
            Token result;
            do
            {
                SkipWhitespace();
                if (_position == _text.Length)
                    return new Token(_eofTokenType);

                _nextTokenStartPosition = _position;
                char c = _text[_position];
                if (c < 128 && _charTokens[c] != null)
                {
                    result = _charTokens[c];
                    _position++;
                }
                else
                    result = FetchNextToken();
            } while (result == null);
            _nextTokenEndPosition = _position;
            return result;
        }

        private void SkipWhitespace()
        {
            while (_position < _text.Length && Char.IsWhiteSpace(_text, _position))
            {
                if (_text [_position] == '\r' || _text [_position] == '\n')
                {
                    _position++;
                    if (_position < _text.Length && 
                        (_text [_position] == '\r' || _text [_position] == '\n') &&
                        _text [_position] != _text [_position-1])
                    {
                        _position++;
                    }
                    _curLine++;
                    _curLineStart = _position;
                }
                else
                    _position++;
            }
        }

        protected abstract Token FetchNextToken();

        public T PeekNextToken()
        {
            return NextToken.TokenType;
        }

        public V GetNextToken(T expectedType)
        {
            if (NextToken.TokenType.Equals(expectedType))
            {
                V result = NextToken.TokenValue;
                ConsumeNextToken();
                return result;
            }
            throw new ParseException("Unexpected token " + NextToken.TokenType, BuildTextPosition(_nextTokenStartPosition));
        }

        public bool CheckNextToken(T expectedType)
        {
            if (NextToken.TokenType.Equals(expectedType))
            {
                ConsumeNextToken();
                return true;
            }
            return false;
        }
        
        public T? CheckNextToken(T[] tokenTypes)
        {
            if (Array.IndexOf(tokenTypes, NextToken.TokenType) >= 0)
            {
                T nextToken = NextToken.TokenType;
                ConsumeNextToken();
                return nextToken;
            }
            return null;
        }

        public TextPosition CurrentPosition
        {
            get
            {
                if (_nextToken == null) _nextToken = ParseNextToken();
                return BuildTextPosition(_nextTokenStartPosition);
            }
        }
        
        public TextPosition LastTokenEndPosition
        {
            get 
            {
                return BuildTextPosition(_nextTokenEndPosition); 
            }
        }

        protected TextPosition BuildTextPosition(int position)
        {
            return new TextPosition(_fileName, _curLine, position - _curLineStart);
        }

        protected Token FetchDelimitedString(char delimiterEnd, char delimiterStart, T tokenType)
        {
            _position++;
            int startPos = _position;
            int nestingLevel = 1;
            while (_position < _text.Length)
            {
                if (_text[_position] == delimiterEnd)
                {
                    nestingLevel--;
                    if (nestingLevel == 0) break;
                }
                else if (delimiterStart != 0 && _text[_position] == delimiterStart)
                    nestingLevel++;
                _position++;
            }

            if (nestingLevel > 0)
                throw new ParseException("Unclosed " + delimiterEnd, BuildTextPosition(startPos));

            string stringValue = _text.Substring(startPos, _position - startPos);
            Token result = new Token(tokenType, EvaluateStringValue(stringValue));
            _position++;
            return result;
        }

        protected abstract V EvaluateStringValue(string stringValue);
    }
}
