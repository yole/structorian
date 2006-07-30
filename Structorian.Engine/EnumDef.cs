using System;
using System.Collections.Generic;

namespace Structorian.Engine
{
    public class EnumDef
    {
        private StructFile _structFile;
        private string _name;
        private Dictionary<int, string> _values = new Dictionary<int, string>();
        private string _inherit;
        private bool _global;
        private bool _globalMask;
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
            if (_global)
                _structFile.RegisterGlobalEnumConstant(name, value);
            if (_globalMask)
                _structFile.RegisterGlobalEnumConstant(name, 1 << value);
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
            else if (key == "global")
                _global = (Int32.Parse(value) > 0);
            else if (key == "globalmask")
                _globalMask = (Int32.Parse(value) > 0);
            else
                throw new Exception("Unknown attribute " + key);
        }
    }
}
