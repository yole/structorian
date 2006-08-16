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
            int offset = (int)reader.BaseStream.Position;
            uint value = reader.ReadUInt32();
            DateTime dt = new DateTime(1970, 1, 1).AddSeconds(value);
            AddCell(instance, dt, offset);
        }

        public override int GetDataSize()
        {
            return 4;
        }
    }
}
