using System;
using System.Collections.Generic;
using System.IO;

namespace Structorian.Engine.Fields
{
    public class ChildField: StructField, IChildSeed
    {
        private Expression _offsetExpr;
        private Expression _countExpr;
        private string _childStruct;
        private string _groupName;
        private bool _isSibling;
        
        public ChildField(StructDef structDef, bool isSibling)
            : base(structDef)
        {
            _isSibling = isSibling;
        }

        public override string DefaultAttribute
        {
            get { return "struct"; }
        }

        public override void SetAttribute(string key, string value)
        {
            if (key == "struct")
                _childStruct = value;
            else if (key == "offset")
                _offsetExpr = ExpressionParser.Parse(value);
            else if (key == "count")
                _countExpr = ExpressionParser.Parse(value);
            else if (key == "group")
                _groupName = value;
            else
                base.SetAttribute(key, value);
        }

        public override bool ProvidesChildren()
        {
            return !_isSibling;
        }

        public override void LoadData(BinaryReader reader, StructInstance instance)
        {
            if (_isSibling)
                DoLoadChildren(instance, instance.Parent, reader.BaseStream);
            else
                instance.AddChildSeed(this);
        }

        public void LoadChildren(StructInstance instance, Stream stream)
        {
            if (!_isSibling)
                DoLoadChildren(instance, instance, stream);
        }

        private void DoLoadChildren(StructInstance instance, InstanceTreeNode parent, Stream stream)
        {
            if (_groupName != null)
            {
                GroupContainer container = new GroupContainer(parent, _groupName);
                parent.AddChild(container);
                parent = container;
            }
            
            int count;
            if (_countExpr != null)
            {
                count = _countExpr.EvaluateInt(instance);
                if (count == 0) return;
            }
            else
                count = 1;
            
            StructDef childDef;
            if (_childStruct != null)
            {
                childDef = _structDef.StructFile.GetStructByName(_childStruct);
                if (childDef == null)
                    throw new LoadDataException("Structure " + _childStruct + " not found");
            }
            else
                childDef = _structDef;
            
            StructInstance childInstance;
            if (_offsetExpr != null)
            {
                long childOffset = _offsetExpr.EvaluateLong(instance);
                childInstance = new StructInstance(childDef, parent, stream, childOffset);
            }
            else
                childInstance = new StructInstance(childDef, parent, stream, instance);
            parent.AddChild(childInstance);
            
            for(int i=1; i<count; i++)
            {
                StructInstance nextInstance = new StructInstance(childDef, parent, stream, childInstance);
                parent.AddChild(nextInstance);
                childInstance = nextInstance;
            }
        }
    }
}
