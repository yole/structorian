using System;

namespace Structorian.Engine
{
    class BaseFunctions: FunctionRegistry<IEvaluateContext, IConvertible, Expression>
    {
        private static BaseFunctions _instance;

        public static BaseFunctions Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new BaseFunctions();
                return _instance;
            }
        }

        private BaseFunctions()
        {
            Register("EndsWith", EndsWith);
            Register("Length", Length);
        }

        private static IConvertible EndsWith(IEvaluateContext context, Expression[] parameters)
        {
            string param1 = parameters[0].EvaluateString(context);
            string param2 = parameters[1].EvaluateString(context);
            return param1.EndsWith(param2);
        }

        private static IConvertible Length(IEvaluateContext context, Expression[] parameters)
        {
            string param = parameters[0].EvaluateString(context);
            return param.Length;
        }
    }

    class StructFunctions: FunctionRegistry<StructInstance, IConvertible, Expression>
    {
        private static StructFunctions _instance;

        public static StructFunctions Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new StructFunctions();
                return _instance;
            }
        }

        private StructFunctions()
        {
            Register("StructOffset", (context, parameters) => context.Offset);
            Register("EndOffset", (context, parameters) => context.EndOffset);
            Register("ParentCount", ParentCount);
            Register("CurOffset", (context, parameters) => context.CurOffset);
            Register("StructName", (context, parameters) => context.Def.Name);
            Register("FileSize", (context, parameters) => context.Stream.Length);
            Register("SizeOf", SizeOf);
            Register("ChildIndex", (context, parameters) => context.ChildIndex);
        }

        private static IConvertible ParentCount(StructInstance context, Expression[] parameters)
        {
            int count = 0;
            StructInstance p = ContextFunctions.EvaluateParent(context, new IConvertible[0]);
            while (p != null)
            {
                count++;
                p = ContextFunctions.EvaluateParent(p, new IConvertible[0]);
            }
            return count;
        }

        private static IConvertible SizeOf(StructInstance context, Expression[] parameters)
        {
            if (parameters.Length != 1) 
                throw new LoadDataException("SizeOf function requires an argument");
            if (!(parameters [0] is SymbolExpression))
                throw new LoadDataException("SizeOf argument must be a symbol");
            string symbol = ((SymbolExpression) parameters[0]).Symbol;

            StructFile structFile = context.Def.StructFile;
            StructDef def = structFile.GetStructByName(symbol);
            if (def == null) throw new LoadDataException("Structure '" + symbol + "' not found");
            return def.GetDataSize();
        }
    }

    class ContextFunctions: FunctionRegistry<StructInstance, IEvaluateContext, IConvertible>
    {
        private static ContextFunctions _instance;

        public static ContextFunctions Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ContextFunctions();
                return _instance;
            }
        }

        private ContextFunctions()
        {
            Register("parent", EvaluateParent);
            Register("prevsibling", EvaluatePrevSibling);
            Register("child", EvaluateChild);
            Register("root", EvaluateRoot);
        }

        public static StructInstance EvaluateParent(StructInstance context, IConvertible[] parameters)
        {
            InstanceTreeNode parent = context.Parent;
            while (parent != null && !(parent is StructInstance))
            {
                parent = parent.Parent;
            }
            return (StructInstance)parent;
        }

        private static IEvaluateContext EvaluatePrevSibling(StructInstance context, IConvertible[] parameters)
        {
            StructInstance parent = (StructInstance) EvaluateParent(context, parameters);
            if (parent == null) throw new Exception("Structure does not have a previous sibling");
            int index = parent.Children.IndexOf(context);
            if (index <= 0) throw new Exception("Structure does not have a previous sibling"); ;
            return (StructInstance)parent.Children[index - 1];
        }

        private static IEvaluateContext EvaluateChild(StructInstance context, IConvertible[] parameters)
        {
            int childIndex;
            InstanceTreeNode parent = context;
            context.NeedChildren();
            if (parameters.Length == 1)
            {
                childIndex = parameters[0].ToInt32(null);
            }
            else if (parameters.Length == 2)
            {
                string groupName = parameters[0].ToString(null);
                bool groupFound = false;
                foreach (InstanceTreeNode child in context.Children)
                {
                    if (child is GroupContainer && child.NodeName == groupName)
                    {
                        parent = child;
                        groupFound = true;
                    }
                }
                if (!groupFound) throw new Exception("Could not find child group " + groupName);
                childIndex = parameters[1].ToInt32(null);
            }
            else
                throw new Exception("'child' context requires 1 or 2 parameters");
            parent.NeedChildren();
            var children = parent.Children;
            if (childIndex < 0 || childIndex >= children.Count)
                throw new Exception("Invalid child index " + childIndex + ": child count " + children.Count);
            return (IEvaluateContext)children[childIndex];
        }

        private static IEvaluateContext EvaluateRoot(StructInstance context, IConvertible[] parameters)
        {
            InstanceTreeNode root = context;
            while (!(root.Parent is InstanceTree))
            {
                root = root.Parent;
            }
            return (StructInstance)root;
        }
    }
}
