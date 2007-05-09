using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Structorian.Engine.Fields
{
    public class BlobCell: StructCell
    {
        private MemoryStream _dataStream;

        public BlobCell(StructField def, byte[] data, int offset) : base(def, null, offset)
        {
            _dataStream = new MemoryStream(data);

            int bytesToDisplay = Math.Min(16, (int)_dataStream.Length);
            StringBuilder bytesBuilder = new StringBuilder();
            for (int i = 0; i < bytesToDisplay; i++)
            {
                if (bytesBuilder.Length > 0)
                    bytesBuilder.Append(' ');
                bytesBuilder.Append(data[i].ToString("X2"));
            }
            _displayValue = bytesBuilder.ToString();
        }

        public Stream DataStream
        {
            get { return _dataStream; }
        }
    }
}
