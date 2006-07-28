using System;
using System.Collections.Generic;
using System.IO;

namespace Structorian.Engine
{
    public abstract class StructField
    {
        protected StructDef _structDef;
        private List<StructField> _childFields = null;
        private StructField _linkedToField = null;
        protected List<StructField> _linkedFields = null;
        private string _tag;
        private string _id;
        private string _defaultAttribute = "tag";
        private bool _acceptsChildren = false;
        protected bool _hidden;

        protected StructField(StructDef structDef)
        {
            _structDef = structDef;
        }

        protected StructField(StructDef structDef, string defaultAttribute, bool acceptsChildren)
        {
            _structDef = structDef;
            _defaultAttribute = defaultAttribute;
            _acceptsChildren = acceptsChildren;
        }

        public string Id
        {
            get
            {
                if (_id != null) return _id;
                return _tag;
            }
        }

        public string Tag
        {
            get { return _tag; }
        }
        
        public virtual void SetAttribute(string key, string value)
        {
            if (key == "tag")
                _tag = value;
            else if (key == "id")
                _id = value;
            else if (key == "hidden")
                _hidden = (Int32.Parse(value) > 0);
            else
                throw new Exception("Unknown attribute " + key);
        }
        
        public virtual string DefaultAttribute
        {
            get { return _defaultAttribute;  }
        }

        public List<StructField> ChildFields
        {
            get
            {   
                if (_childFields == null)
                    _childFields = new List<StructField>();
                return _childFields;
            }
        }

        public abstract void LoadData(BinaryReader reader, StructInstance instance);

        public void AddChildField(StructField field)
        {
            if (!_acceptsChildren)
                throw new Exception("Field does not accept children");
            
            if (_childFields == null)
                _childFields = new List<StructField>();
            _childFields.Add(field);
        }

        protected void LoadChildFields(BinaryReader reader, StructInstance instance)
        {
            foreach (StructField field in ChildFields)
            {
                field.LoadData(reader, instance);
            }
        }
        
        public virtual bool ProvidesChildren()
        {
            foreach(StructField field in ChildFields)
            {
                if (field.ProvidesChildren()) return true;
            }
            return false;
        }

        protected void AddCell(StructInstance instance, IConvertible value)
        {
            instance.AddCell(new StructCell(this, value), _hidden);
        }

        protected void AddCell(StructInstance instance, IConvertible value, string displayValue)
        {
            instance.AddCell(new StructCell(this, value, displayValue), _hidden);
        }
        
        protected internal virtual bool CanLinkField(StructField nextField)
        {
            return false;
        }

        internal void LinkField(StructField field)
        {
            if (_linkedFields == null)
                _linkedFields = new List<StructField>();
            _linkedFields.Add(field);
            field._linkedToField = this;
        }
        
        internal bool IsLinked
        {
            get { return _linkedToField != null; }
        }
    }
}

