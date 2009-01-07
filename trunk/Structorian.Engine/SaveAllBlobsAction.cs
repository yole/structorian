using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Structorian.Engine.Fields;

namespace Structorian.Engine
{
    public class SaveAllBlobsAction
    {
        private readonly InstanceTree _tree;
        private readonly string _outDir;
        private readonly byte[] _buffer = new byte[64000];
        private readonly List<string> _errors = new List<string>();

        public SaveAllBlobsAction(InstanceTree tree, string outDir)
        {
            _tree = tree;
            _outDir = outDir;
        }

        public void Run()
        {
            SaveBlobsIn(_tree);
        }

        private void SaveBlobsIn(InstanceTreeNode node)
        {
            SaveBlobCells(node);

            int i = 0;
            while (i < node.Children.Count)
            {
                SaveBlobsIn(node.Children [i]);
                i++;
            }
        }

        private void SaveBlobCells(InstanceTreeNode node)
        {
            string name = null;
            foreach(StructCell cell in node.Cells)
            {
                if (cell.GetValue() is string)
                {
                    name = (string) cell.GetValue();
                }
                else if (cell is BlobCell && name != null)
                {
                    try
                    {
                        SaveBlobCell((BlobCell) cell, name);
                    }
                    catch (Exception e)
                    {
                        _errors.Add("Failed to extract data for " + name + ": " + e.Message);
                    }
                }
            }
        }

        private void SaveBlobCell(BlobCell blobCell, string name)
        {
            string outName = Path.Combine(_outDir, name);
            Directory.CreateDirectory(Path.GetDirectoryName(outName));
            Stream ms = blobCell.DataStream;
            using(var fs = new FileStream(outName, FileMode.CreateNew))
            {
                while(true)
                {
                    int bytes = ms.Read(_buffer, 0, 64000);
                    if (bytes == 0) break;
                    fs.Write(_buffer, 0, bytes);
                }
            }
        }
    }
}
