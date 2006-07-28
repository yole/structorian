using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Structorian.Engine
{
    public class StructDef
    {
        private string _name;
        private StructFile _structFile;
        private List<StructField> _fields = new List<StructField>();
        private string _fileMask;

        public StructDef(StructFile structFile, string name)
        {
            _structFile = structFile;
            _name = name;
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public StructFile StructFile
        {
            get { return _structFile; }
        }

        public ReadOnlyCollection<StructField> Fields
        {
            get { return _fields.AsReadOnly();  }
        }

        public string FileMask
        {
            get { return _fileMask; }
        }

        public void AddField(StructField field)
        {
            _fields.Add(field);
        }

        public InstanceTree LoadData(Stream stream)
        {
            InstanceTree tree = new InstanceTree();
            StructInstance instance = new StructInstance(this, tree, stream, stream.Position);
            tree.AddChild(instance);
            return tree;
        }

        public void LoadInstanceData(StructInstance instance, Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            foreach (StructField field in _fields)
            {
                try
                {
                    field.LoadData(reader, instance);
                }
                catch(LoadDataException ex)
                {
                    instance.AddCell(StructCell.CreateErrorCell(field, ex.Message), false);
                    break;
                }
            }
            if (instance.RewindOffset != -1)
                stream.Position = instance.RewindOffset;
        }

        public void SetAttribute(string name, string value)
        {
            if (name == "filemask")
                _fileMask = value;
            else
                throw new Exception("Unknown attribute " + name);
        }

        public bool HasChildProvidingFields()
        {
            return _fields.Find(delegate(StructField f) { return f.ProvidesChildren(); }) != null;
        }
    }
}
