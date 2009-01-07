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

    internal delegate void CellHandler(StructCell cell);

    public class StructInstance: InstanceTreeNode, IEvaluateContext
    {
        private StructDef _def;
        private InstanceTreeNode _parent;
        private Stream _stream;
        private long _offset;
        private long _endOffset;
        private Stack<long> _rewindStack;
        private string _nodeName;
        private List<StructCell> _cells = null;
        private List<StructCell> _hiddenCells = null;
        private List<InstanceTreeNode> _children = null;
        private List<IChildSeed> _childSeeds = null;
        private Dictionary<StructCell, int> _cellSizes = null;
        private bool _preloading = false;
        private Stack<CellHandler> _addedCellHandlers = new Stack<CellHandler>();

        private StructInstance _followInstance = null;
        private bool _followChildren;

        public StructInstance(StructDef def, InstanceTreeNode parent, Stream stream, long offset)
        {
            _def = def;
            _parent = parent;
            _stream = stream;
            _offset = offset;
        }

        public StructInstance(StructDef def, InstanceTreeNode parent, Stream stream, 
            StructInstance followInstance, bool followChildren)
        {
            _def = def;
            _parent = parent;
            _stream = stream;
            _followInstance = followInstance;
            _followChildren = followChildren;
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
                    return _followChildren ? _followInstance.EndChildrenOffset : _followInstance.EndOffset;
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

        public override long EndChildrenOffset
        {
            get
            {
                if (HasChildren)
                {
                    NeedChildren();
                    if (_children.Count > 0)
                        return _children[_children.Count - 1].EndChildrenOffset;
                }
                return EndOffset;
            }
        }

        public void MarkRewindOffset(long offset)
        {
            if (_rewindStack == null)
                _rewindStack = new Stack<long>();
            _rewindStack.Push(offset);
        }

        public long PopRewindOffset()
        {
            if (_rewindStack == null || _rewindStack.Count == 0)
                throw new LoadDataException("No rewind offset found");
            return _rewindStack.Pop();
        }

        public long GetLastRewindOffset()
        {
            long result = -1;
            if (_rewindStack != null)
            {
                while (_rewindStack.Count > 0)
                    result = _rewindStack.Pop();
            }
            return result;
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

        internal bool Preloading
        {
            get { return _preloading; }
            set { _preloading = value; }
        }

        public override ReadOnlyCollection<InstanceTreeNode> Children
        {
            get
            {
                NeedChildren();
                return _children.AsReadOnly();
            }
        }

        public override StructInstance LastChild
        {
            get
            {
                if (_children != null && _children.Count > 0)
                {
                    var child = _children [_children.Count-1];
                    if (child.Children.Count > 0)
                        return child.LastChild;
                    return (StructInstance) child;
                }
                return this;
            }
        }

        public IConvertible ChildIndex
        {
            get
            {
                InstanceTreeNode parent = Parent;
                if (parent == null)
                    throw new Exception("Structure instance doesn't have any parent");
                return parent.Children.IndexOf(this);
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
                _children = new List<InstanceTreeNode>();
                NeedData();
                if (_childSeeds != null)
                {
                    foreach(IChildSeed childSeed in _childSeeds)
                    {
                        childSeed.LoadChildren(this, _stream);
                    }
                }
                if (_def.Preload && !_preloading)
                {
                    PreloadChildren();
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

            foreach(CellHandler handler in _addedCellHandlers)
            {
                handler(cell);
            }
        }

        public override void AddChild(InstanceTreeNode instance)
        {
            _children.Add(instance);
            GetInstanceTree().NotifyInstanceAdded(this, instance);
        }
        
        public override void NeedData()
        {
            if (_cells == null)
            {
                _cells = new List<StructCell>();
                long oldPosition = _stream.Position;
                _stream.Position = Offset;
                _def.LoadInstanceData(this, _stream);
                _endOffset = _stream.Position;
                if (_def.Preload && !_preloading)
                {
                    NeedChildren();
                }
                _stream.Position = oldPosition;
            }
        }

        private void PreloadChildren()
        {
            int i = 0;
            while (i<_children.Count)
            {
                InstanceTreeNode child = _children [i];
                if (child is StructInstance)
                {
                    (child as StructInstance).Preloading = true;
                    try
                    {
                        child.NeedData();
                    }
                    finally
                    {
                        (child as StructInstance).Preloading = false;
                    }
                }
                i++;
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

            uint? enumValue = _def.StructFile.EvaluateGlobalEnumConstant(symbol);
            if (enumValue.HasValue)
                return enumValue.Value;

            IConvertible funcValue = StructFunctions.Instance.Evaluate(symbol, null, this);
            if (funcValue != null)
                return funcValue;
            
            throw new LoadDataException("Unknown symbol " + symbol);
        }

        public IConvertible EvaluateFunction(string symbol, Expression[] parameters)
        {
            NeedData();

            IConvertible funcValue = StructFunctions.Instance.Evaluate(symbol, parameters, this);
            if (funcValue != null)
                return funcValue;

            throw new LoadDataException("Unknown symbol " + symbol);
        }

        public IEvaluateContext EvaluateContext(string symbol, IConvertible[] parameters)
        {
            IEvaluateContext context = ContextFunctions.Instance.Evaluate(symbol, parameters, this);
            if (context != null)
                return context;

            throw new LoadDataException("Unknown context " + symbol);
        }

        public void RegisterGlobal(string id, int result)
        {
            GetInstanceTree().RegisterGlobal(id, result);
        }
        
        internal void RegisterCellSize(StructCell cell, int size)
        {
            if (_cellSizes == null)
                _cellSizes = new Dictionary<StructCell, int>();
            _cellSizes[cell] = size;
        }
        
        internal int? GetCellSize(StructCell cell)
        {
            int result;
            if (_cellSizes != null && _cellSizes.TryGetValue(cell, out result))
                return result;
            return null;
        }

        internal void PushAddedCellHandler(CellHandler handler)
        {
            _addedCellHandlers.Push(handler);
        }

        internal void PopAddedCellHandler()
        {
            _addedCellHandlers.Pop();
        }
    }
}
