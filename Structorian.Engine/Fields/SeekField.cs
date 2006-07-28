using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Structorian.Engine.Fields
{
    class SeekField: StructField
    {
        private Expression _offsetExpr;
        private bool _relative;
        
        public SeekField(StructDef structDef, bool relative) : base(structDef, "offset", false)
        {
            _relative = relative;
        }

        public override void SetAttribute(string key, string value)
        {
            if (key == "offset")
                _offsetExpr = ExpressionParser.Parse(value);
            else
                base.SetAttribute(key, value);
        }

        public override void LoadData(BinaryReader reader, StructInstance instance)
        {
            long offset = _offsetExpr.EvaluateLong(instance);
            if (_relative)
                reader.BaseStream.Seek(offset, SeekOrigin.Current);
            else
            {
                if (instance.RewindOffset == -1)
                    instance.RewindOffset = reader.BaseStream.Position;
                reader.BaseStream.Position = offset;
            }
        }
    }
}
