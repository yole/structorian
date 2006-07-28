using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Structorian.Engine.Fields
{
    public class StrField: StructField
    {
        private Expression _lengthExpr;
        private bool _wide;

        public StrField(StructDef structDef, bool wide)
            : base(structDef)
        {
            _wide = wide;
        }

        public Expression LengthExpression
        {
            get { return _lengthExpr; }
        }

        public override void SetAttribute(string key, string value)
        {
            if (key == "len")
                _lengthExpr = ExpressionParser.Parse(value);
            else
                base.SetAttribute(key, value);
        }

        public override void LoadData(BinaryReader reader, StructInstance instance)
        {
            if (_wide)
                reader = new BinaryReader(reader.BaseStream, Encoding.Unicode);
            
            string value;
            if (_lengthExpr != null)
            {
                int length = (int)_lengthExpr.Evaluate(instance);
                if (reader.BaseStream.Length - reader.BaseStream.Position < length)
                {
                    throw new LoadDataException("Length expression " + _lengthExpr.ToString() +
                                                " has the result of " + length + " and points outside the file");
                }
                char[] chars = reader.ReadChars(length);
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
            }
            AddCell(instance, value);
        }
    }
}
