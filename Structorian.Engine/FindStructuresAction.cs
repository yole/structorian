using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Structorian.Engine
{
    public class FindStructuresAction: StructorianAction
    {
        private readonly List<InstanceTree> _scope;
        private readonly StructDef _def;
        private readonly Expression _expr;
        private readonly List<InstanceTreeNode> _results = new List<InstanceTreeNode>();

        public FindStructuresAction(List<InstanceTree> scope, StructDef def, string expr)
        {
            _scope = scope;
            _def = def;
            if (expr.Length > 0)
            {
                _expr = ExpressionParser.Parse(expr);
            }
        }

        public string Text
        {
            get { return "Searching for structures..."; }
        }

        public void Run()
        {
            _scope.ForEach(tree => tree.EachNode(CheckMatch));
        }

        private void CheckMatch(InstanceTreeNode node)
        {
            if (node is StructInstance)
            {
                var instance = (StructInstance) node;
                if (instance.Def == _def && (_expr == null || _expr.EvaluateBool(instance)))
                {
                    _results.Add(instance);
                }
            }
        }

        public List<InstanceTreeNode> Results
        {
            get { return _results; }
        }
    }
}
