using System;
using System.IO;

namespace Structorian.Engine.Fields
{
    class CalcField: StructField
    {
        public CalcField(StructDef structDef, bool hidden) : base(structDef)
        {
            _hidden = hidden;
        }

        public override void LoadData(BinaryReader reader, StructInstance instance)
        {
            var context = new DependencyTrackingContext(instance);
            var value = GetExpressionAttribute("value").Evaluate(context);
            var cell = AddCell(instance, value, context.DependencyStartOffset);
            if (context.DependencyStartOffset >= 0)
            {
                instance.RegisterCellSize(cell, context.DependencySize);
            }
        }
    }

    class DependencyTrackingContext: DelegatingEvaluateContext
    {
        private readonly StructInstance _instance;
        private int _dependencyStartOffset = -1;
        private int _dependencySize = 0;

        public DependencyTrackingContext(StructInstance instance) : base(instance)
        {
            _instance = instance;
        }

        public override IConvertible EvaluateSymbol(string symbol)
        {
            var cell = _instance.FindSymbolCell(symbol);
            if (cell != null)
            {
                UpdateDependency(cell);
                return cell.GetValue();
            }
            return _instance.EvaluateSymbol(symbol);
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
