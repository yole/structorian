using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Structorian.Engine.Fields
{
    public interface BlobDecoder
    {
        string Name { get; }
        byte[] Decode(byte[] input);
    }
}
