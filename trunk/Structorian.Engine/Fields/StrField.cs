using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Structorian.Engine.Fields
{
    public class StrField: StructField
    {
        private bool _wide;

        public StrField(StructDef structDef, bool wide)
            : base(structDef)
        {
            _wide = wide;
        }

        public override void LoadData(BinaryReader reader, StructInstance instance)
        {
            int offset = (int)reader.BaseStream.Position;
            int charSize = 1;
            int cellSize;
            if (_wide)
            {
                reader = new BinaryReader(reader.BaseStream, Encoding.Unicode);
                charSize = 2;
            }
            
            string value;
            Expression lengthExpr = GetExpressionAttribute("len");
            if (lengthExpr != null)
            {
                int length = lengthExpr.EvaluateInt(instance);
                if (reader.BaseStream.Length - reader.BaseStream.Position < length)
                {
                    throw new LoadDataException("Length expression " + lengthExpr.ToString() +
                                                " has the result of " + length + " and points outside the file");
                }
                char[] chars = reader.ReadChars(length);
                cellSize = length*charSize;
                while (length > 0 && chars[length - 1] == '\0')
                    length--;
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

        public override int GetDataSize(StructCell cell, StructInstance instance)
        {
            return instance.GetCellSize(cell).Value;
        }
    }
}
