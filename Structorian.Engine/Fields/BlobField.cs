using System;
using System.IO;

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
            if (len < 0)
                throw new LoadDataException("Blob size " + len + " is negative");
            var decodedSizeExpr = GetExpressionAttribute("decodedsize");
            int decodedSize = decodedSizeExpr != null ? decodedSizeExpr.EvaluateInt(instance) : -1;
            string encoding = GetStringAttribute("encoding");
            BlobDecoder blobDecoder = FindBlobEncoding(instance, encoding);
            BlobCell cell = new BlobCell(this, reader.BaseStream, offset, len, blobDecoder, decodedSize);
            instance.AddCell(cell, _hidden);
            instance.RegisterCellSize(cell, len);
            reader.BaseStream.Position += len;

            StructDef structDef = GetStructAttribute("struct");
            if (structDef != null)
                instance.AddChildSeed(new BlobChildSeed(structDef, cell));
        }

        private static BlobDecoder FindBlobEncoding(StructInstance instance, string encoding)
        {
            if (encoding == null) return null;
            if (encoding.ToLowerInvariant() == "zlib")
            {
                return new ZLibDecoder();
            }
            if (encoding.ToLowerInvariant() == "minilzo")
            {
                return new MiniLZODecoder();
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
