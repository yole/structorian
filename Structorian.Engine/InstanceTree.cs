using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Structorian.Engine
{
    public class InstanceTree: InstanceTreeNode
    {
        private List<InstanceTreeNode> _instances = new List<InstanceTreeNode>();
        private Dictionary<string, int> _globals = new Dictionary<string, int>();

        public override void AddChild(InstanceTreeNode node)
        {
            _instances.Add(node);
            NotifyInstanceAdded(this, node);
        }
        
        public override ReadOnlyCollection<InstanceTreeNode> Children
        {
            get { return _instances.AsReadOnly();  }
        }

        public override InstanceTreeNode Parent
        {
            get { return null; }
        }

        public override string NodeName
        {
            get { return null; }
        }
        
        public override bool HasChildren
        {
            get { return true;  }
        }

        public override void NeedData()
        {
        }

        public override void NeedChildren()
        {
        }

        public override ReadOnlyCollection<StructCell> Cells
        {
            get { return StructCell.EmptyCollection(); }
        }

        internal void RegisterGlobal(string id, int result)
        {
            _globals [id] = result;
        }
        
        internal int? GetGlobal(string id)
        {
            int result;
            if (_globals.TryGetValue(id, out result))
                return result;
            return null;
        }
        

        public void NotifyInstanceAdded(InstanceTreeNode parent, InstanceTreeNode child)
        {
            if (InstanceAdded != null)
            {
                InstanceAdded(this, new InstanceAddedEventArgs(parent, child));
            }
        }

        public void NotifyNodeNameChanged(InstanceTreeNode node)
        {
            if (NodeNameChanged != null)
            {
                NodeNameChanged(this, new NodeNameChangedEventArgs(node));
            }
        }

        public event InstanceAddedEventHandler InstanceAdded;
        public event NodeNameChangedEventHandler NodeNameChanged;
    }
    
    public class InstanceAddedEventArgs: EventArgs
    {
        private InstanceTreeNode _parent;
        private InstanceTreeNode _child;

        public InstanceAddedEventArgs(InstanceTreeNode parent, InstanceTreeNode child)
        {
            _parent = parent;
            _child = child;
        }

        public InstanceTreeNode Parent
        {
            get { return _parent; }
        }

        public InstanceTreeNode Child
        {
            get { return _child; }
        }
    }
    
    public delegate void InstanceAddedEventHandler(object sender, InstanceAddedEventArgs e);
    
    public class NodeNameChangedEventArgs: EventArgs
    {
        private InstanceTreeNode _node;

        public NodeNameChangedEventArgs(InstanceTreeNode node)
        {
            _node = node;
        }

        public InstanceTreeNode Node
        {
            get { return _node; }
        }
    }

    public delegate void NodeNameChangedEventHandler(object sender, NodeNameChangedEventArgs e);
}
