using System;
using System.Collections.Generic;
using System.Text;

namespace Structorian.Engine
{
    public abstract class ReferenceBase
    {
        protected StructField _baseField;
        protected string _attrName;
        protected string _targetName;
        protected TextPosition _position;

        protected ReferenceBase(StructField baseField, string attrName, string targetName, TextPosition position)
        {
            _baseField = baseField;
            _attrName = attrName;
            _targetName = targetName;
            _position = position;
        }

        public abstract void Resolve();
    }
}
