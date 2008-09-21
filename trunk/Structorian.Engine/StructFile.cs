using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;

namespace Structorian.Engine
{
    public class StructFile
    {
        private readonly string _baseDir;
        private readonly List<StructDef> _structDefs = new List<StructDef>();
        private readonly List<EnumDef> _enumDefs = new List<EnumDef>();
        private readonly Dictionary<string, uint> _globalEnumConstants = new Dictionary<string, uint>();
        private readonly List<ReferenceBase> _references = new List<ReferenceBase>();
        private readonly List<string> _pluginFileNames = new List<string>();
        private readonly List<Type> _pluginExportedTypes = new List<Type>();

        public StructFile(string baseDir)
        {
            _baseDir = baseDir;
        }

        public ReadOnlyCollection<StructDef> Structs
        {
            get { return _structDefs.AsReadOnly();  }
        }
        
        public ReadOnlyCollection<ReferenceBase> References
        {
            get { return _references.AsReadOnly();  }
        }

        public ReadOnlyCollection<string> PluginFileNames
        {
            get { return _pluginFileNames.AsReadOnly(); }
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

        internal void AddPlugin(string pluginFile)
        {
            _pluginFileNames.Add(pluginFile);

            string path = Path.Combine(_baseDir, pluginFile + ".dll");
            Assembly plugin = Assembly.LoadFrom(path);
            Type[] exportedTypes = plugin.GetExportedTypes();
            _pluginExportedTypes.AddRange(exportedTypes);
        }

        public List<T> GetPluginExtensions<T>()
        {
            return _pluginExportedTypes
                .FindAll(t => typeof(T).IsAssignableFrom(t))
                .ConvertAll(t => (T) Activator.CreateInstance(t));
        }
    }
}
