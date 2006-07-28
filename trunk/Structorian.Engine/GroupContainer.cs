using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Structorian.Engine
{
    class GroupContainer: InstanceTreeNode
    {
        private string _groupName;
        private readonly InstanceTreeNode _parent;
        private List<InstanceTreeNode> _children = null;

        public GroupContainer(InstanceTreeNode parent, string groupName)
        {
            _parent = parent;
            _groupName = groupName;
        }

        public override InstanceTreeNode Parent
        {
            get { return _parent; }
        }

        public override string NodeName
        {
            get { return _groupName; }
        }
        
        public override bool HasChildren
        {
            get { return true; }
        }

        public override void NeedChildren()
        {
            if (_children != null)
            {
                InstanceTree tree = GetInstanceTree();
                foreach(InstanceTreeNode child in _children)
                {
                    tree.NotifyInstanceAdded(this, child);
                }
            }
        }

        public override ReadOnlyCollection<StructCell> Cells
        {
            get { return StructCell.EmptyCollection(); }
        }

        public override ReadOnlyCollection<InstanceTreeNode> Children
        {
            get { return _children.AsReadOnly(); }
        }

        public override void AddChild(InstanceTreeNode instance)
        {
            if (_children == null)
                _children = new List<InstanceTreeNode>();
            _children.Add(instance);
        }
    }
}
