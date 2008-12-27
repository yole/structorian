using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace Structorian.Engine.Fields
{
    public interface ImageDecoder
    {
        string Name { get; }
        Image Decode(Stream inputStream);
    }

    public class DefaultImageDecoder: ImageDecoder
    {
        public string Name
        {
            get { return "Default"; }
        }

        public Image Decode(Stream inputStream)
        {
            return Image.FromStream(inputStream);
        }
    }
}
