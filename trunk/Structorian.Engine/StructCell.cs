using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Structorian.Engine
{
    public class StructCell
    {
        private static ReadOnlyCollection<StructCell> _emptyCollection = new List<StructCell>().AsReadOnly();
        
        private StructField _def;
        private IConvertible _value;
        private string _displayValue;
        private bool _isError;
        
        public static StructCell CreateErrorCell(StructField def, string errorMessage)
        {
            StructCell result = new StructCell(def, null, errorMessage);
            result._isError = true;
            return result;
        }

        public StructCell(StructField def, IConvertible value)
        {
            _def = def;
            _value = value;
            _displayValue = _value.ToString();
        }

        public StructCell(StructField def, IConvertible value, string displayValue)
        {
            _def = def;
            _value = value;
            _displayValue = displayValue;
        }

        public StructField GetStructDef()
        {
            return _def;
        }

        public string Tag
        {
            get { return _def.Tag; }
        }
        
        public string Value
        {
            get { return _displayValue;  }
        }

        public override string ToString()
        {
            return _displayValue;
        }
        
        public IConvertible GetValue()
        {
            return _value;
        }

        public bool IsError()
        {
            return _isError;
        }

        public static ReadOnlyCollection<StructCell> EmptyCollection()
        {
            return _emptyCollection;
        }
    }
}
