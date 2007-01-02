using System;
using System.IO;

namespace Structorian.Engine.Fields
{
    class BreakField: StructField
    {
        public BreakField(StructDef structDef) : base(structDef)
        {
        }

        public override void LoadData(BinaryReader reader, StructInstance instance)
        {
            throw new BreakRepeatException();
        }
    }
}
