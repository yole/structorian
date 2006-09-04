using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Structorian.Engine.Fields
{
    public class StrField: StructField
    {
        private bool _wide;
        private bool _requireNullTerminated;

        public StrField(StructDef structDef, bool wide, bool requireNullTerminated)
            : base(structDef)
        {
            _wide = wide;
            _requireNullTerminated = requireNullTerminated;
        }

        public override void LoadData(BinaryReader reader, StructInstance instance)
        {
            int offset = (int)reader.BaseStream.Position;
            int charSize = 1;
            int cellSize;
            if (_wide)
                charSize = 2;
            
            string value;
            Expression lengthExpr = GetExpressionAttribute("len");
            if (lengthExpr != null)
            {
                int length = lengthExpr.EvaluateInt(instance);
                cellSize = length * charSize;
                if (reader.BaseStream.Length - reader.BaseStream.Position < cellSize)
                {
                    throw new LoadDataException("Length expression " + lengthExpr.ToString() +
                                                " has the result of " + length + " and points outside the file");
                }
                byte[] bytes = reader.ReadBytes(length * charSize);
                char[] chars = _wide ? Encoding.Unicode.GetChars(bytes) : Encoding.Default.GetChars(bytes);
                for (int i = 0; i < length; i++)
                {
                    if (chars [i] == '\0')
                    {
                        length = i;
                        break;
                    }
                }
                value = new string(chars, 0, length);
            }
            else
            {
                StringBuilder valueBuilder = new StringBuilder();
                while (true)
                {
                    char c = reader.ReadChar();
                    if (c == '\0') break;
                    valueBuilder.Append(c);
                }
                value = valueBuilder.ToString();
                cellSize = value.Length*charSize;
            }
            StructCell cell = AddCell(instance, value, offset);
            instance.RegisterCellSize(cell, cellSize);
        }

        public override int GetDataSize()
        {
            Expression lengthExpr = GetExpressionAttribute("len");
            if (lengthExpr.IsConstant)
            {
                int result = lengthExpr.EvaluateInt(null);
                return _wide ? result*2 : result;
            }
            return base.GetDataSize();
        }
    }
}
