using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace Structorian.Engine.Fields
{
    class ImageField: StructField
    {
        public ImageField(StructDef structDef) : base(structDef)
        {
        }

        public override void LoadData(BinaryReader reader, StructInstance instance)
        {
            var format = GetStringAttribute("format");
            ImageDecoder decoder = FindImageDecoder(instance, format);
            var image = decoder.Decode(reader.BaseStream, instance);
            instance.AddCell(new ImageCell(this, image, (int)reader.BaseStream.Position), false);
        }

        private static ImageDecoder FindImageDecoder(StructInstance instance, string format)
        {
            if (format == "bmp")
            {
                return new DefaultImageDecoder();
            }
            var decoders = instance.Def.StructFile.GetPluginExtensions<ImageDecoder>();
            var result = decoders.Find(d => d.Name == format);
            if (result == null) throw new Exception("Couldn't find decoder for format " + format);
            return result;
        }
    }

    public class ImageCell : StructCell
    {
        private readonly Image _image;

        public ImageCell(StructField def, Image value, int offset) : base(def, null, offset)
        {
            _image = value;
        }

        public Image Image
        {
            get { return _image; }
        }
    }
}
