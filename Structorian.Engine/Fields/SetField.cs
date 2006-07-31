using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Structorian.Engine.Fields
{
    class SetField: IntBasedField
    {
        private string _enumName;
        
        public SetField(StructDef structDef, int size) : base(structDef, size, true)
        {
        }

        public override void SetAttribute(string key, string value)
        {
            if (key == "enum")
                _enumName = value;
            else
                base.SetAttribute(key, value);
        }

        public override void LoadData(BinaryReader reader, StructInstance instance)
        {
            IConvertible iValue = ReadIntValue(reader);
            uint value = iValue.ToUInt32(CultureInfo.CurrentCulture);
            EnumDef enumDef = _structDef.StructFile.GetEnumByName(_enumName);
            if (enumDef == null)
                throw new LoadDataException("Enum '" + _enumName + "' not found");
            
            StringBuilder result = new StringBuilder();
            for(int i=0; i<_size*8; i++)
            {
                if ((value & (1 << i)) != 0)
                {
                    if (result.Length > 0)
                        result.Append(", ");
                    result.Append(enumDef.ValueToString(i));
                }
            }
            AddCell(instance, new EnumValue((int) value, result.ToString()));
        }
    }
}
