using System;
using System.Collections.Generic;
using System.Text;

namespace Structorian.Engine
{
    public class ParseException: Exception
    {
        private readonly TextPosition _position;

        public ParseException(string message, TextPosition position) : base(message)
        {
            _position = position;
        }

        public TextPosition Position
        {
            get { return _position; }
        }
    }
}
