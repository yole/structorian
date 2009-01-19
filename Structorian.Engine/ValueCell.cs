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
        private bool _isError;

        public static StructCell CreateErrorCell(StructField def, string errorMessage)
        {
            ValueCell result = new ValueCell(def, null, errorMessage, -1);
            result._isError = true;
            return result;
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

        public override bool IsError()
        {
            return _isError;
        }
    }
}