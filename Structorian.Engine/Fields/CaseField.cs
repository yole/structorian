using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Structorian.Engine.Fields
{
    class CaseField: StructField
    {
        private bool _default;

        public CaseField(StructDef structDef, bool isDefault) 
            : base(structDef, "expr", true)
        {
            _default = isDefault;
        }

        public bool IsDefault
        {
            get { return _default; }
        }

        public override void LoadData(BinaryReader reader, StructInstance instance)
        {
            LoadChildFields(reader, instance);
        }
    }
}
