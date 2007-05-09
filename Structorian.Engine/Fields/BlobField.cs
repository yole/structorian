using System;
using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace Structorian.Engine.Fields
{
    class BlobField: StructField
    {
        public BlobField(StructDef structDef) : base(structDef, "len", false)
        {
        }

        public override void LoadData(BinaryReader reader, StructInstance instance)
        {
            int offset = (int)reader.BaseStream.Position;
            int len = GetExpressionAttribute("len").EvaluateInt(instance);
            if (offset + len > reader.BaseStream.Length)
                throw new LoadDataException("Blob size " + len + " exceeds stream length");
            byte[] blobBytes = reader.ReadBytes(len);
            string encoding = GetStringAttribute("encoding");
            if (encoding == "zlib")
            {
                InflaterInputStream stream = new InflaterInputStream(new MemoryStream(blobBytes));
                byte[] data = new byte[4096];
                MemoryStream outStream = new MemoryStream();
                int size;
                while((size = stream.Read(data, 0, data.Length)) > 0)
                {
                    outStream.Write(data, 0, size);
                }
                outStream.Capacity = (int) outStream.Length;
                blobBytes = outStream.GetBuffer();
            }
            BlobCell cell = new BlobCell(this, blobBytes, offset);
            instance.AddCell(cell, _hidden);
            instance.RegisterCellSize(cell, len);
        }

        public override int GetDataSize()
        {
            Expression lengthExpr = GetExpressionAttribute("len");
            if (lengthExpr.IsConstant)
                return lengthExpr.EvaluateInt(null);

            return base.GetDataSize();
        }
    }
}
