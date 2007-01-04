using System;
using System.Collections.ObjectModel;

namespace Structorian.Engine
{
    public abstract class InstanceTreeNode
    {
        public abstract InstanceTreeNode Parent { get; }
        public abstract void AddChild(InstanceTreeNode instance);
        public abstract string NodeName { get; }
        public abstract bool HasChildren { get; }
        public abstract void NeedData();
        public abstract void NeedChildren();
        public abstract ReadOnlyCollection<InstanceTreeNode> Children { get; }
        public abstract ReadOnlyCollection<StructCell> Cells { get; }

        public abstract StructInstance LastChild { get; }
        public abstract long EndChildrenOffset { get; }

        protected InstanceTree GetInstanceTree()
        {
            InstanceTreeNode parent = Parent;
            while (!(parent is InstanceTree))
                parent = parent.Parent;
            return (InstanceTree)parent;
        }
    }
}
