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

        public InstanceTree GetInstanceTree()
        {
            InstanceTreeNode parent = this;
            while (!(parent is InstanceTree))
                parent = parent.Parent;
            return (InstanceTree)parent;
        }

        public void EachNode(Action<InstanceTreeNode> action)
        {
            action(this);
            int i = 0;
            try
            {
                while (i < Children.Count)
                {
                    Children [i].EachNode(action);
                    i++;
                }
            }
            catch (LoadDataException)
            {
                // ignore
            }
            catch(OverflowException)
            {
                // ignore
            }
        }
    }
}
