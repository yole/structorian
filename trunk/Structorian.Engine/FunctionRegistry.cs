using System;
using System.Collections.Generic;

namespace Structorian.Engine
{
    class FunctionRegistry<T> where T : IEvaluateContext
    {
        protected delegate IConvertible FunctionDelegate(T context, Expression[] parameters);

        private Dictionary<String, FunctionDelegate> _functions = new Dictionary<string, FunctionDelegate>(StringComparer.InvariantCultureIgnoreCase);

        protected void Register(string name, FunctionDelegate functionDelegate)
        {
            _functions[name] = functionDelegate;
        }

        public IConvertible Evaluate(string function, Expression[] parameters, IEvaluateContext context)
        {
            FunctionDelegate evalDelegate;
            if (!_functions.TryGetValue(function, out evalDelegate))
                return null;
            if (context != null && !(context is T))
                throw new Exception("Invalid context type");

            return evalDelegate((T) context, parameters);
        }
    }
}
