using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Structorian.Engine
{
    public enum ByteOrder { Default, LittleEndian, BigEndian };
    
    public class StructDef
    {
        private string _name;
        private StructFile _structFile;
        private List<StructField> _fields = new List<StructField>();
        private string _fileMask;
        private ByteOrder _byteOrder = ByteOrder.Default;
        private bool _hidden;
        private bool _fieldLike;
        private bool _preload;

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

        public bool Preload
        {
            get { return _preload; }
        }

        public bool FieldLike
        {
            get { return _fieldLike; }
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
                if (field.IsLinked) continue;
                
                try
                {
                    field.LoadData(reader, instance);
                }
                catch(LoadDataException ex)
                {
                    instance.AddCell(StructCell.CreateErrorCell(field, ex.Message), false);
                    break;
                }
                catch(IOException ex)
                {
                    instance.AddCell(StructCell.CreateErrorCell(field, ex.Message), false);
                    break;
                }
            }
            if (instance.RewindOffset != -1)
                stream.Position = instance.RewindOffset;
        }

        public void SetAttribute(string name, string value, TextPosition position)
        {
            if (name == "filemask")
                _fileMask = value;
            else if (name == "byteorder")
            {
                if (value == "intel" || value == "littleendian")
                    _byteOrder = ByteOrder.LittleEndian;
                else if (value == "motorola" || value == "bigendian")
                    _byteOrder = ByteOrder.BigEndian;
                else
                    throw new ParseException("Unknown byteorder value " + value, position);
            }
            else if (name == "hidden")
                _hidden = true;
            else if (name == "preload")
                _preload = true;
            else if (name == "fieldlike")
                _fieldLike = (Int32.Parse(value) > 0);
            else
                throw new ParseException("Unknown attribute " + name, position);
        }

        public bool HasChildProvidingFields()
        {
            return _fields.Find(delegate(StructField f) { return f.ProvidesChildren(); }) != null;
        }
        
        public bool IsReverseByteOrder()
        {
            return _byteOrder == ByteOrder.BigEndian;
        }

        public int GetDataSize()
        {
            int result = 0;
            foreach(StructField field in _fields)
            {
                if (!field.IsLinked)
                    result += field.GetDataSize();
            }
            return result;
        }
    }
}
