using System;
using System.Windows.Forms;
using Structorian.Engine;
using Structorian.Engine.Fields;

namespace Structorian
{
    interface CellUI
    {
        void ContextMenuStripNeeded(DataGridViewCellContextMenuStripNeededEventArgs e);
    }

    class BlobCellUI: CellUI
    {
        private readonly BlobCell _cell;

        public BlobCellUI(BlobCell cell)
        {
            _cell = cell;
        }

        public void ContextMenuStripNeeded(DataGridViewCellContextMenuStripNeededEventArgs e)
        {
            var strip = new ContextMenuStrip();
            strip.Items.Add(new ToolStripMenuItem("Save Data...", null, SaveData));
            e.ContextMenuStrip = strip;
        }

        private void SaveData(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog();
            if (dlg.ShowDialog() != DialogResult.OK) return;
            using(var outStream = dlg.OpenFile())
            {
                var buffer = new byte[65536];
                var inStream = _cell.DataStream;
                inStream.Position = 0;
                int bytes;
                while ((bytes = inStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    outStream.Write(buffer, 0, bytes);
                }
            }
        }
    }

    class CellUIRegistry
    {
        public static CellUI GetUI(StructCell cell)
        {
            if (cell is BlobCell)
            {
                return new BlobCellUI((BlobCell) cell);
            }
            return null;
        }
    }
}
