using System;
using System.IO;

namespace Structorian.Engine.Fields
{
    class MessageField: StructField
    {
        private bool _error;

        public MessageField(StructDef structDef, bool error) 
            : base(structDef, "text", false)
        {
            _error = error;
        }


        public override void LoadData(BinaryReader reader, StructInstance instance)
        {
            string text = GetStringAttribute("text");
            if (_error)
                throw new LoadDataException(text);
            AddCell(instance, text, -1);
        }
    }
}
