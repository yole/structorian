using System;
using System.Collections.Generic;

namespace Structorian.Engine
{
    public class EnumDef
    {
        private StructFile _structFile;
        private string _name;
        private Dictionary<uint, string> _values = new Dictionary<uint, string>();
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

        public bool GlobalMask
        {
            get { return _globalMask; }
        }

        public void AddValue(string name, uint value)
        {
            _values.Add(value, name);
            if (_global)
                _structFile.RegisterGlobalEnumConstant(name, value);
            if (_globalMask)
                _structFile.RegisterGlobalEnumConstant(name, (uint) (1 << (int) value));
        }

        public string ValueToString(uint value)
        {
            if (_values.ContainsKey(value))
                return _values[value];
            
            if (_inherit != null)
            {
                InitBaseEnum();
                return _baseEnum.ValueToString(value);
            }
            return "";
        }

        private void InitBaseEnum()
        {
            if (_baseEnum == null)
            {
                _baseEnum = _structFile.GetEnumByName(_inherit);
                if (_baseEnum == null)
                    throw new LoadDataException("Base enum " + _inherit + " not found");
            }
        }

        public uint? StringToValue(string s)
        {
            foreach(uint k in _values.Keys)
            {
                if (_values[k] == s)
                    return k;
            }
            if (_inherit != null)
            {
                InitBaseEnum();
                return _baseEnum.StringToValue(s);
            }
            return null;
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
