using System;
using System.IO;

namespace Structorian.Engine.Fields
{
    class IncludeField: StructField
    {
        private string _includeStructName;
        
        public IncludeField(StructDef structDef) : base(structDef, "struct", false)
        {
        }

        public override void SetAttribute(string key, string value)
        {
            if (key == "struct")
                _includeStructName = value;
            else
                base.SetAttribute(key, value);
        }

        public override void LoadData(BinaryReader reader, StructInstance instance)
        {
            StructDef structDef = GetIncludedStruct();
            structDef.LoadInstanceData(instance, reader.BaseStream);
            if (GetBoolAttribute("replace"))
                instance.SetNodeName(structDef.Name);
        }

        private StructDef GetIncludedStruct()
        {
            StructDef structDef = _structDef.StructFile.GetStructByName(_includeStructName);
            if (structDef == null)
                throw new LoadDataException("Unknown struct " + _includeStructName + " in include");
            return structDef;
        }

        public override bool ProvidesChildren()
        {
            StructDef structDef = GetIncludedStruct();
            return structDef.HasChildProvidingFields();
        }
    }
}
