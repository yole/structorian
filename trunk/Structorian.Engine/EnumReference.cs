using System;
using System.Collections.Generic;
using System.Text;

namespace Structorian.Engine
{
    class EnumReference: ReferenceBase
    {
        public EnumReference(StructField baseField, string attrName, string targetName, TextPosition position) 
            : base(baseField, attrName, targetName, position)
        {
        }

        public override void Resolve()
        {
            EnumDef target = _baseField.StructDef.StructFile.GetEnumByName(_targetName);
            if (target == null)
                throw new ParseException("Unknown enum " + _targetName, _position);
            _baseField.SetAttributeValue(_attrName, target);
        }
    }
}
