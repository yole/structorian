using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Structorian.Engine
{
    public class StructFile
    {
        private List<StructDef> _structDefs = new List<StructDef>();
        private List<EnumDef> _enumDefs = new List<EnumDef>();
        private Dictionary<string, uint> _globalEnumConstants = new Dictionary<string, uint>();
        private List<ReferenceBase> _references = new List<ReferenceBase>();
        
        public ReadOnlyCollection<StructDef> Structs
        {
            get { return _structDefs.AsReadOnly();  }
        }
        
        public ReadOnlyCollection<ReferenceBase> References
        {
            get { return _references.AsReadOnly();  }
        }
        
        public void Add(StructDef def)
        {
            _structDefs.Add(def);
        }
        
        public void Add(EnumDef def)
        {
            _enumDefs.Add(def);
        }

        public StructDef GetStructByName(string structName)
        {
            return _structDefs.Find(delegate(StructDef def) { return def.Name == structName; });
        }

        public EnumDef GetEnumByName(string name)
        {
            return _enumDefs.Find(delegate(EnumDef def) { return def.Name == name; });
        }
        
        internal void RegisterGlobalEnumConstant(string name, uint value)
        {
            _globalEnumConstants.Add(name, value);
        }

        public uint? EvaluateGlobalEnumConstant(string name)
        {
            uint result;
            if (_globalEnumConstants.TryGetValue(name, out result))
                return result;
            return null;
        }
        
        internal void AddReference(ReferenceBase reference)
        {
            _references.Add(reference);
        }
    }
}
