using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Structorian.Engine
{
    interface IChildSeed
    {
        void LoadChildren(StructInstance instance, Stream stream);
    }
    
    public class StructInstance: InstanceTreeNode, IEvaluateContext
    {
        private StructDef _def;
        private InstanceTreeNode _parent;
        private Stream _stream;
        private long _offset;
        private long _endOffset;
        private long _rewindOffset = -1;
        private string _nodeName;
        private List<StructCell> _cells = null;
        private List<StructCell> _hiddenCells = null;
        private List<InstanceTreeNode> _children = null;
        private List<IChildSeed> _childSeeds = null;

        private StructInstance _followInstance = null;

        public StructInstance(StructDef def, InstanceTreeNode parent, Stream stream, long offset)
        {
            _def = def;
            _parent = parent;
            _stream = stream;
            _offset = offset;
        }

        public StructInstance(StructDef def, InstanceTreeNode parent, Stream stream, StructInstance followInstance)
        {
            _def = def;
            _parent = parent;
            _stream = stream;
            _followInstance = followInstance;
        }

        public StructDef Def
        {
            get { return _def; }
        }

        public Stream Stream
        {
            get { return _stream; }
        }

        public override ReadOnlyCollection<StructCell> Cells
        {
            get
            {
                NeedData();
                return _cells.AsReadOnly();
            }
        }

        public override InstanceTreeNode Parent
        {
            get { return _parent; }
        }

        public override bool HasChildren
        {
            get { return _def.HasChildProvidingFields(); }
        }
        
        public long Offset
        {
            get
            {
                if (_followInstance != null)
                    return _followInstance.EndOffset;
                return _offset;
            }
        }

        public long EndOffset
        {
            get
            {
                NeedData();
                return _endOffset;
            }
        }

        public long RewindOffset
        {
            get { return _rewindOffset; }
            set { _rewindOffset = value; }
        }
        
        public long CurOffset
        {
            get { return _stream.Position; }
        }

        public override string NodeName
        {
            get
            {
                if (_nodeName != null)
                    return _nodeName;
                return _def.Name;
            }
        }
        
        internal void SetNodeName(string nodeName)
        {
            _nodeName = nodeName;
            GetInstanceTree().NotifyNodeNameChanged(this);
        }

        public override ReadOnlyCollection<InstanceTreeNode> Children
        {
            get
            {
                NeedChildren();
                return _children.AsReadOnly();
            }
        }

        public int ParentCount
        {
            get
            {
                int count = 0;
                StructInstance p = EvaluateParent();
                while(p != null)
                {
                    count++;
                    p = p.EvaluateParent();
                }
                return count;
            }
        }

        internal void AddChildSeed(IChildSeed childSeed)
        {
            if (_childSeeds == null)
                _childSeeds = new List<IChildSeed>();
            _childSeeds.Add(childSeed);
        }

        public override void NeedChildren()
        {
            if (_children == null)
            {
                NeedData();
                _children = new List<InstanceTreeNode>();
                if (_childSeeds != null)
                {
                    foreach(IChildSeed childSeed in _childSeeds)
                    {
                        childSeed.LoadChildren(this, _stream);
                    }
                }
            }
        }

        internal void AddCell(StructCell cell, bool hidden)
        {
            if (hidden)
            {
                if (_hiddenCells == null) 
                    _hiddenCells = new List<StructCell>();
                _hiddenCells.Add(cell);
            }
            else
                _cells.Add(cell);
        }

        public override void AddChild(InstanceTreeNode instance)
        {
            _children.Add(instance);
            GetInstanceTree().NotifyInstanceAdded(this, instance);
        }
        
        private void NeedData()
        {
            if (_cells == null)
            {
                _cells = new List<StructCell>();
                _stream.Position = Offset;
                _def.LoadInstanceData(this, _stream);
                _endOffset = _stream.Position;
            }
        }

        public IConvertible EvaluateSymbol(string symbol)
        {
            NeedData();
            Predicate<StructCell> predicate = delegate(StructCell aCell) { return aCell.GetStructDef().Id == symbol; };
            StructCell cell = _cells.FindLast(predicate);
            if (cell == null && _hiddenCells != null)
                cell = _hiddenCells.FindLast(predicate);
            if (cell != null)
                return cell.GetValue();

            int? global = GetInstanceTree().GetGlobal(symbol);
            if (global.HasValue)
                return global.Value;

            int? enumValue = _def.StructFile.EvaluateGlobalEnumConstant(symbol);
            if (enumValue.HasValue)
                return enumValue.Value;

            IConvertible funcValue = ExpressionFunctions.Evaluate(symbol, this);
            if (funcValue != null)
                return funcValue;
            
            throw new Exception("Unknown symbol " + symbol);
        }

        public IEvaluateContext EvaluateContext(string symbol)
        {
            if (symbol.ToLowerInvariant() == "parent")
            {
                StructInstance parent = EvaluateParent();
                if (parent == null) throw new Exception("Expression does not have a parent");
                return parent;
            }
            throw new Exception("Unknown context " + symbol);
        }

        private StructInstance EvaluateParent()
        {
            InstanceTreeNode parent = _parent;
            while(parent != null && !(parent is StructInstance))
            {
                parent = parent.Parent;
            }
            return (StructInstance) parent;
        }

        public void RegisterGlobal(string id, int result)
        {
            GetInstanceTree().RegisterGlobal(id, result);
        }
    }
}
