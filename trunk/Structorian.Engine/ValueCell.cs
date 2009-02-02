using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Structorian.Engine
{
    public class ValueCell: StructCell
    {
        private IConvertible _value;
        protected string _displayValue;
        private int _offset;

        public static StructCell CreateErrorCell(StructField def, string errorMessage)
        {
            return new ValueCell(def, new ErrValue(errorMessage), -1);
        }

        public ValueCell(StructField def, IConvertible value, int offset)
            : base(def)
        {
            _value = value;
            _offset = offset;
        }

        public ValueCell(StructField def, IConvertible value, string displayValue, int offset): base(def)
        {
            _value = value;
            _displayValue = displayValue;
            _offset = offset;
        }

        public override int Offset
        {
            get { return _offset; }
        }

        public override string Value
        {
            get
            {
                if (_displayValue != null)
                    return _displayValue;
                return _value.ToString();
            }
        }

        public override IConvertible GetValue()
        {
            return _value;
        }
    }
}