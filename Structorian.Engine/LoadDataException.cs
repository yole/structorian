using System;

namespace Structorian.Engine
{
    class LoadDataException: Exception
    {
        public LoadDataException(string message) : base(message)
        {
        }
    }
}
