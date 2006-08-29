using System;
using System.Collections.Generic;
using System.Text;

namespace Structorian.Engine
{
    class StructReference: ReferenceBase
    {
        public StructReference(StructField baseField, string attrName, string targetName,
            TextPosition pos): base(baseField, attrName, targetName, pos)
        {
        }
        
        public override void Resolve()
        {
            StructDef target = _baseField.StructDef.StructFile.GetStructByName(_targetName);
            if (target == null)
                throw new ParseException("Unknown struct " + _targetName, _position);
            _baseField.SetAttributeValue(_attrName, target);
        }
    }
}
