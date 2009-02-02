using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Structorian.Engine
{
    public abstract class StructCell
    {
        private static readonly ReadOnlyCollection<StructCell> _emptyCollection = new List<StructCell>().AsReadOnly();
        
        private readonly StructField _def;
        private string _tag;
        
        protected StructCell(StructField def)
        {
            _def = def;
        }

        public StructField GetStructDef()
        {
            return _def;
        }

        public abstract int Offset { get; }

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
        
        public abstract string Value { get; }

        public override string ToString()
        {
            return Value;
        }

        public abstract IConvertible GetValue();
        
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
