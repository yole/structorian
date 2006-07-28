using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Structorian.Engine
{
    public abstract class InstanceTreeNode
    {
        public abstract InstanceTreeNode Parent { get; }
        public abstract void AddChild(InstanceTreeNode instance);
        public abstract string NodeName { get; }
        public abstract bool HasChildren { get; }
        public abstract void NeedChildren();
        public abstract ReadOnlyCollection<InstanceTreeNode> Children { get; }
        public abstract ReadOnlyCollection<StructCell> Cells { get; }

        protected InstanceTree GetInstanceTree()
        {
            InstanceTreeNode parent = Parent;
            while (!(parent is InstanceTree))
                parent = parent.Parent;
            return (InstanceTree)parent;
        }
    }
}
