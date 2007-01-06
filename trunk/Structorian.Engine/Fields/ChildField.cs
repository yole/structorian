using System;
using System.IO;

namespace Structorian.Engine.Fields
{
    public class ChildField: StructField
    {
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

        public override bool ProvidesChildren()
        {
            return !_isSibling;
        }

        public override void LoadData(BinaryReader reader, StructInstance instance)
        {
            long? offset = EvaluateOffset(instance);
            int count = EvaluateCount(instance);
            if (_isSibling)
                DoLoadChildren(instance, instance.Parent, reader.BaseStream, offset, count);
            else
            {
                instance.AddChildSeed(new ChildSeed(this, offset, count, _isSibling));
            }
        }

        private void DoLoadChildren(StructInstance instance, InstanceTreeNode parent, Stream stream,
            long? offset, int count)
        {
            StructInstance lastChild = instance.LastChild;
            string groupName = GetStringAttribute("group");
            if (groupName != null)
            {
                GroupContainer container = new GroupContainer(parent, groupName);
                parent.AddChild(container);
                parent = container;
            }

            if (count == 0) return;

            StructDef childDef = GetStructAttribute("struct");
            if (childDef == null)
                childDef = _structDef;

            StructInstance childInstance;
            bool followChildren = GetBoolAttribute("followchildren");
            if (offset.HasValue)
            {
                childInstance = new StructInstance(childDef, parent, stream, offset.Value);
            }
            else
            {
                bool firstFollowChildren = followChildren && lastChild != parent;
                childInstance = new StructInstance(childDef, parent, stream, lastChild, firstFollowChildren);
            }
            parent.AddChild(childInstance);

            for (int i = 1; i < count; i++)
            {
                StructInstance nextInstance = new StructInstance(childDef, parent, stream, childInstance, followChildren);
                parent.AddChild(nextInstance);
                childInstance = nextInstance;
            }
        }

        private long? EvaluateOffset(StructInstance instance)
        {
            Expression offsetExpr = GetExpressionAttribute("offset");
            if (offsetExpr != null)
                return offsetExpr.EvaluateLong(instance);
            else
                return null;
        }

        private int EvaluateCount(StructInstance instance)
        {
            Expression countExpr = GetExpressionAttribute("count");
            if (countExpr != null)
                return countExpr.EvaluateInt(instance);
            else
                return 1;
        }

        class ChildSeed : IChildSeed
        {
            private readonly ChildField _field;
            private long? _offset;
            private int _count;
            private bool _isSibling;

            public ChildSeed(ChildField field, long? offset, int count, bool isSibling)
            {
                _field = field;
                _offset = offset;
                _count = count;
                _isSibling = isSibling;
            }

            public void LoadChildren(StructInstance instance, Stream stream)
            {
                if (!_isSibling)
                    _field.DoLoadChildren(instance, instance, stream, _offset, _count);
            }

        }
    }
}
