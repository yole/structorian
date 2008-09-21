using System;
using System.Collections.Generic;
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
            BlobDecoder blobDecoder = FindBlobEncoding(instance, encoding);
            if (blobDecoder != null)
            {
                blobBytes = blobDecoder.Decode(blobBytes);
            }
            BlobCell cell = new BlobCell(this, blobBytes, offset);
            instance.AddCell(cell, _hidden);
            instance.RegisterCellSize(cell, len);

            StructDef structDef = GetStructAttribute("struct");
            if (structDef != null)
                instance.AddChildSeed(new BlobChildSeed(structDef, cell));
        }

        private static BlobDecoder FindBlobEncoding(StructInstance instance, string encoding)
        {
            if (encoding == null) return null;
            if (encoding == "zlib")
            {
                return new ZLibDecoder();
            }
            var decoders = instance.Def.StructFile.GetPluginExtensions<BlobDecoder>();
            var result = decoders.Find(d => d.Name == encoding);
            if (result == null) throw new Exception("Couldn't find decoder for encoding " + encoding);
            return result;
        }

        public override int GetDataSize()
        {
            Expression lengthExpr = GetExpressionAttribute("len");
            if (lengthExpr.IsConstant)
                return lengthExpr.EvaluateInt(null);

            return base.GetDataSize();
        }

        public override bool ProvidesChildren()
        {
            return GetStructAttribute("struct") != null;
        }

        private class BlobChildSeed: IChildSeed
        {
            private readonly StructDef _def;
            private readonly BlobCell _cell;

            public BlobChildSeed(StructDef def, BlobCell cell)
            {
                _def = def;
                _cell = cell;
            }

            public void LoadChildren(StructInstance instance, Stream stream)
            {
                StructInstance childInstance = new StructInstance(_def, instance, _cell.DataStream, 0);
                instance.AddChild(childInstance);
            }
        }
    }
}
