using System;
using ManagedLZO;

namespace Structorian.Engine.Fields
{
    class MiniLZODecoder: BlobDecoder
    {
        public string Name
        {
            get { return "minilzo"; }
        }

        public byte[] Decode(byte[] input, int decodedSize)
        {
            if (decodedSize < 0)
                throw new LoadDataException("Decoded size must be specified for MiniLZO encoding");
            var result = new byte[decodedSize];
            MiniLZO.Decompress(input, result);
            return result;
        }
    }
}
