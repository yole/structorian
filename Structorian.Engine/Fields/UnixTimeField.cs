using System;
using System.IO;

namespace Structorian.Engine.Fields
{
    class UnixTimeField: StructField
    {
        public UnixTimeField(StructDef structDef) : base(structDef)
        {
        }

        public override void LoadData(BinaryReader reader, StructInstance instance)
        {
            uint value = reader.ReadUInt32();
            DateTime dt = new DateTime(1970, 1, 1).AddSeconds(value);
            AddCell(instance, dt);
        }
    }
}
