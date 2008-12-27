using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace Structorian.Engine.Fields
{
    public interface ImageDecoder
    {
        string Name { get; }
        Image Decode(Stream inputStream, StructInstance instance);
    }

    public class DefaultImageDecoder: ImageDecoder
    {
        public string Name
        {
            get { return "Default"; }
        }

        public Image Decode(Stream inputStream, StructInstance instance)
        {
            return Image.FromStream(inputStream);
        }
    }

    public abstract class ImageDecoderBase: ImageDecoder
    {
        protected static Bitmap CreateBitmapWithPalette(int width, int height, byte[] rawPixels, Color[] palette)
        {
            var bitmap = new Bitmap(width, height);

            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite,
                  PixelFormat.Format24bppRgb);

            IntPtr ptr = bmpData.Scan0;

            int bytes = width * height * 3;
            byte[] rgbValues = new byte[bytes];

            for (int i = 0; i < width * height; i++)
            {
                byte c = rawPixels[i];
                rgbValues[i * 3] = palette[c].R;
                rgbValues[i * 3 + 1] = palette[c].G;
                rgbValues[i * 3 + 2] = palette[c].B;
            }

            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);

            bitmap.UnlockBits(bmpData);

            return bitmap;
        }

        public abstract string Name { get; }
        public abstract Image Decode(Stream inputStream, StructInstance instance);
    }
}
