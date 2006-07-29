using System;
using System.Collections.Generic;
using System.Text;

namespace Structorian.Engine
{
    public class TextPosition
    {
        string _file;
        private int _line;
        private int _col;

        public TextPosition(string file, int line, int col)
        {
            _file = file;
            _line = line;
            _col = col;
        }

        public string File
        {
            get { return _file; }
        }

        public int Line
        {
            get { return _line; }
        }

        public int Col
        {
            get { return _col; }
        }

        public override string ToString()
        {
            return _file + " (" + _line + "," + _col + ")";
        }

        public TextPosition OffsetBy(TextPosition position)
        {
            if (position.Line == 1)
                return new TextPosition(_file, _line, _col + position.Col);
            else
                return new TextPosition(_file, _line + position.Line - 1, position.Col);
        }
    }
}
