using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Structorian.Engine
{
    public class StructCell
    {
        private static ReadOnlyCollection<StructCell> _emptyCollection = new List<StructCell>().AsReadOnly();
        
        private StructField _def;
        private string _tag;
        private IConvertible _value;
        private string _displayValue;
        private int _offset;
        private bool _isError;
        
        public static StructCell CreateErrorCell(StructField def, string errorMessage)
        {
            StructCell result = new StructCell(def, null, errorMessage, -1);
            result._isError = true;
            return result;
        }

        public StructCell(StructField def, IConvertible value, int offset)
        {
            _def = def;
            _value = value;
            _offset = offset;
        }

        public StructCell(StructField def, IConvertible value, string displayValue, int offset)
        {
            _def = def;
            _value = value;
            _displayValue = displayValue;
            _offset = offset;
        }

        public StructField GetStructDef()
        {
            return _def;
        }

        public int Offset
        {
            get { return _offset; }
        }

        public string Tag
        {
            get
            {
                if (_tag != null)
                    return _tag;

                return _def.Tag;
            }
            set { _tag = value; }
        }
        
        public string Value
        {
            get
            {
                if (_displayValue != null)
                    return _displayValue;
                return _value.ToString();
            }
        }

        public override string ToString()
        {
            return Value;
        }
        
        public IConvertible GetValue()
        {
            return _value;
        }

        public bool IsError()
        {
            return _isError;
        }
        
        public int GetDataSize(StructInstance instance)
        {
            return _def.GetInstanceDataSize(this, instance);
        }

        public static ReadOnlyCollection<StructCell> EmptyCollection()
        {
            return _emptyCollection;
        }
    }
}
