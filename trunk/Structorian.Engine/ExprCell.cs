using System;

namespace Structorian.Engine
{
    class ExprCell: StructCell
    {
        private readonly StructInstance _instance;
        private readonly Expression _expression;
        private int _offset = -2;

        public ExprCell(StructField def, StructInstance instance, Expression expression) : base(def)
        {
            _instance = instance;
            _expression = expression;
        }

        public override int Offset
        {
            get
            {
                if (_offset == -2)
                {
                    var context = new DependencyTrackingContext(_instance, this);
                    try
                    {
                        _expression.Evaluate(context);
                    }
                    catch (LoadDataException)
                    {
                        return -1;    // will be reported when the cell is displayed
                    }
                    _offset = context.DependencyStartOffset;
                    if (context.DependencyStartOffset >= 0)
                    {
                        _instance.RegisterCellSize(this, context.DependencySize);
                    }
                }
                return _offset;
            }
        }

        public override string Value
        {
            get { return GetValue().ToString(); }
        }

        public override IConvertible GetValue()
        {
            return _expression.Evaluate(new DependencyTrackingContext(_instance, this));
        }
    }

    class DependencyTrackingContext : DelegatingEvaluateContext
    {
        private readonly StructInstance _instance;
        private readonly StructCell _cell;
        private int _dependencyStartOffset = -1;
        private int _dependencySize = 0;

        public DependencyTrackingContext(StructInstance instance, StructCell cell)
            : base(instance)
        {
            _instance = instance;
            _cell = cell;
        }

        public override IConvertible EvaluateSymbol(string symbol)
        {
            var cell = _instance.FindSymbolCellBefore(symbol, _cell);
            if (cell != null)
            {
                UpdateDependency(cell);
                return cell.GetValue();
            }
            return _instance.EvaluateGlobalOrFunction(symbol);
        }

        private void UpdateDependency(StructCell cell)
        {
            var dataSize = cell.GetStructDef().GetInstanceDataSize(cell, _instance);
            if (_dependencyStartOffset < 0)
            {
                _dependencyStartOffset = cell.Offset;
                _dependencySize = dataSize;
            }
            // include only adjacent cells in dependency range calculation    
            else if (cell.Offset + dataSize == _dependencyStartOffset)
            {
                _dependencyStartOffset = cell.Offset;
                _dependencySize += dataSize;
            }
            else if (cell.Offset == _dependencyStartOffset + _dependencySize)
            {
                _dependencySize += dataSize;
            }
        }

        public int DependencyStartOffset
        {
            get { return _dependencyStartOffset; }
        }

        public int DependencySize
        {
            get { return _dependencySize; }
        }
    }
}
