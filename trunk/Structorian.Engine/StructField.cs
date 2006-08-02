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
        private TextPosition _position;
        private bool _acceptsChildren = false;
        protected bool _hidden;
        private Dictionary<string, object> _attributeValues = new Dictionary<string, object>();

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

        public TextPosition Position
        {
            get { return _position; }
            set { _position = value; }
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
        
        internal void SetAttributeValue(string key, object value)
        {
            _attributeValues[key] = value;
        }
        
        public object GetAttribute(string key)
        {
            object result;
            if (!_attributeValues.TryGetValue(key, out result))
                return null;
            return result;
        }
        
        public Expression GetExpressionAttribute(string key)
        {
            return (Expression) GetAttribute(key);
        }

        public string GetStringAttribute(string key)
        {
            return (string) GetAttribute(key);
        }

        public int? GetIntAttribute(string key)
        {
            return (int?)GetAttribute(key);
        }

        public bool GetBoolAttribute(string key)
        {
            object attr = GetAttribute(key);
            if (attr == null) return false;
            return (bool) attr;
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
        
        public virtual void Validate()
        {
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
                if (!field.IsLinked)
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

        protected void AddCell(StructInstance instance, IConvertible value, int offset)
        {
            instance.AddCell(new StructCell(this, value, offset), _hidden);
        }

        protected void AddCell(StructInstance instance, IConvertible value, string displayValue, int offset)
        {
            instance.AddCell(new StructCell(this, value, displayValue, offset), _hidden);
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

