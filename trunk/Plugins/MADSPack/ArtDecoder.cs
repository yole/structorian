using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using Structorian.Engine;
using Structorian.Engine.Fields;

namespace MADSPack
{
    public class ArtDecoder: ImageDecoderBase
    {
        public override string Name
        {
            get { return "ART"; }
        }

        public override Image Decode(Stream inputStream, StructInstance instance)
        {
            var metaInstance = (StructInstance) instance.Parent.Parent.Children[0].Children[0];
            int width = metaInstance.EvaluateSymbol("SceneWidth").ToInt32(CultureInfo.CurrentCulture);
            int height = metaInstance.EvaluateSymbol("SceneHeight").ToInt32(CultureInfo.CurrentCulture);
            byte[] pixelData = new byte[width*height];
            instance.Stream.Read(pixelData, 0, width*height);
            Color[] palette = LoadPalette(metaInstance);
            return CreateBitmapWithPalette(width, height, pixelData, palette);
        }

        private static Color[] LoadPalette(StructInstance instance)
        {
            var colorCount = instance.EvaluateSymbol("PaletteColors").ToInt32(CultureInfo.CurrentCulture);
            instance.Stream.Seek(instance.Offset + 6, SeekOrigin.Begin);
            var result = new Color[colorCount+4];
            result[0] = Color.FromArgb(0, 0, 0);
            result[1] = Color.FromArgb(0x54, 0x54, 0x54);
            result[2] = Color.FromArgb(0xb4, 0xb4, 0xb4);
            result[3] = Color.FromArgb(0xff, 0xff, 0xff);
            for(int i=0; i<colorCount; i++)
            {
                int r = instance.Stream.ReadByte();
                int g = instance.Stream.ReadByte();
                int b = instance.Stream.ReadByte();
                instance.Stream.Seek(3, SeekOrigin.Current);
                result[i+4] = Color.FromArgb(r*4, g*4, b*4);
            }
            return result;
        }
    }
}
