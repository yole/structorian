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
            if (instance.RewindOffset < 0)
                throw new Exception("No rewind offset found");
            reader.BaseStream.Position = instance.RewindOffset;
            instance.RewindOffset = -1;
        }
    }
}
