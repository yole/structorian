using System.IO;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace Structorian.Engine.Fields
{
    public class ZLibDecoder: BlobDecoder
    {
        public string Name
        {
            get { return "zlib";  }
        }

        public byte[] Decode(byte[] input, int decompressedSize)
        {
            InflaterInputStream stream = new InflaterInputStream(new MemoryStream(input));
            byte[] data = new byte[4096];
            MemoryStream outStream = new MemoryStream();
            int size;
            while ((size = stream.Read(data, 0, data.Length)) > 0)
            {
                outStream.Write(data, 0, size);
            }
            outStream.Capacity = (int)outStream.Length;
            return outStream.GetBuffer();
        }
    }
}