using System;
using System.Collections.Generic;
using System.Text;

namespace Structorian.Engine
{
    public class EnumDef
    {
        private StructFile _structFile;
        private string _name;
        private Dictionary<int, string> _values = new Dictionary<int, string>();
        private string _inherit;
        private EnumDef _baseEnum;

        public EnumDef(StructFile file, string name)
        {
            _structFile = file;
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }
        
        public void AddValue(string name, int value)
        {
            _values.Add(value, name);
        }

        public string ValueToString(int value)
        {
            if (_values.ContainsKey(value))
                return _values[value];
            
            if (_inherit != null)
            {
                if (_baseEnum == null)
                {
                    _baseEnum = _structFile.GetEnumByName(_inherit);
                    if (_baseEnum == null)
                        throw new LoadDataException("Base enum " + _inherit + " not found");
                }
                return _baseEnum.ValueToString(value);
            }
            return "";
        }

        public void SetAttribute(string key, string value)
        {
            if (key == "inherit")
                _inherit = value;
            else
                throw new Exception("Unknown attribute " + key);
        }
    }
}
