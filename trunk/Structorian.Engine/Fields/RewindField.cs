using System;
using System.IO;

namespace Structorian.Engine.Fields
{
    class RewindField: StructField
    {
        public RewindField(StructDef structDef) : base(structDef)
        {
        }

        public override void LoadData(BinaryReader reader, StructInstance instance)
        {
            reader.BaseStream.Position = instance.PopRewindOffset();
        }
    }
}
