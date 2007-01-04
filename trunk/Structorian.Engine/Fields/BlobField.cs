using System;
using System.IO;
using System.Text;

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
            int bytesToDisplay = Math.Min(16, len);
            StringBuilder bytesBuilder = new StringBuilder();
            for(int i=0; i<bytesToDisplay; i++)
            {
                if (bytesBuilder.Length > 0)
                    bytesBuilder.Append(' ');
                bytesBuilder.Append(blobBytes[i].ToString("X2"));
            }
            StructCell cell = AddCell(instance, bytesBuilder.ToString(), offset);
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
