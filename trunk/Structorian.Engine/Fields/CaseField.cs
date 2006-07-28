using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Structorian.Engine.Fields
{
    class CaseField: StructField
    {
        private bool _default;
        private Expression _caseExpression;

        public CaseField(StructDef structDef, bool isDefault) 
            : base(structDef, "expr", true)
        {
            _default = isDefault;
        }

        public override void SetAttribute(string key, string value)
        {
            if (key == "expr" && !_default)
                _caseExpression = ExpressionParser.Parse(value);
            else
                base.SetAttribute(key, value);
        }

        public bool IsDefault
        {
            get { return _default; }
        }

        public Expression CaseExpression
        {
            get { return _caseExpression; }
        }

        public override void LoadData(BinaryReader reader, StructInstance instance)
        {
            LoadChildFields(reader, instance);
        }
    }
}
