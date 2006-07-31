using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Structorian.Engine.Fields
{
    class CaseField: StructField
    {
        public CaseField(StructDef structDef, bool isDefault) 
            : base(structDef, "expr", true)
        {
            SetAttributeValue("default", isDefault);
        }

        public override void LoadData(BinaryReader reader, StructInstance instance)
        {
            LoadChildFields(reader, instance);
        }
    }
}
