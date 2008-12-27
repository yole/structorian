using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Structorian.Engine;
using Structorian.Engine.Fields;

namespace Structorian
{
    interface NodeUI
    {
        Control CreateControl();
    }

    class ImageNodeUI: NodeUI
    {
        private readonly ImageCell _cell;

        public ImageNodeUI(ImageCell cell)
        {
            _cell = cell;
        }

        public Control CreateControl()
        {
            var panel = new Panel {AutoScroll = true};
            panel.Controls.Add(new PictureBox {Image = _cell.Image, Dock = DockStyle.Fill});
            return panel;
        }
    }

    class NodeUIRegistry
    {
        public static NodeUI GetNodeUI(StructCell cell)
        {
            if (cell is ImageCell)
            {
                return new ImageNodeUI((ImageCell) cell);
            }
            return null;
        }
    }
}

