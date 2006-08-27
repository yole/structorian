using System;

namespace Structorian.Engine
{
    public class ParseException: Exception
    {
        private readonly TextPosition _position;
        private readonly int _length;

        public ParseException(string message, TextPosition position) : base(message)
        {
            _position = position;
            _length = 0;
        }

        public ParseException(string message, TextPosition position, int length)
            : base(message)
        {
            _position = position;
            _length = length;
        }

        public TextPosition Position
        {
            get { return _position; }
        }

        public int Length
        {
            get { return _length; }
        } 
    }
}
