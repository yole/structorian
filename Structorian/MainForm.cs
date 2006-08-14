using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Structorian.Engine;

namespace Structorian
{
    public partial class MainForm : Form
    {
        private string _structFileName;
        private StructFile _structFile;
        private DataView _dataView;
        
        public MainForm()
        {
            InitializeComponent();
            _dataView = new DataView();
            _dataView.Dock = DockStyle.Fill;
            splitContainer2.Panel2.Controls.Add(_dataView);
        }

        private void loadStructuresToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_openStructsDialog.ShowDialog(this) == DialogResult.OK)
            {
                _structFileName = Path.GetFullPath(_openStructsDialog.FileName);
                using(Stream stream = _openStructsDialog.OpenFile())
                {
                    string strs = new StreamReader(stream).ReadToEnd();
                    _structEditControl.Text = strs;
                    _structEditControl.ShowEOLMarkers = false;
                    _structEditControl.ShowInvalidLines = false;
                    ParseStructures();
                }
                _btnSaveStructures.Enabled = true;
            }
        }

        private void ParseStructures()
        {
            StructParser parser = new StructParser();
            try
            {
                _structFile = parser.LoadStructs(_structFileName, _structEditControl.Text);
            }
            catch(ParseException ex)
            {
                MessageBox.Show(this, "Error in " + ex.Position + ": " + ex.Message);
            }
            catch(Exception ex)
            {
                MessageBox.Show(this, "Error loading structures: " + ex.Message);
            }
            if (parser.Errors.Count > 0)
            {
                ParseException ex = parser.Errors[0];
                MessageBox.Show(this, "Error in " + ex.Position + ": " + ex.Message);
            }
        }

        private void _btnSaveStructures_Click(object sender, EventArgs e)
        {
            StreamWriter writer = new StreamWriter(_structFileName);
            try
            {
                writer.Write(_structEditControl.Text);
            }
            finally
            {
                writer.Close();
            }
            ParseStructures();
            
            _dataView.ReloadData(_structFile.Structs [0], true);
        }
        
        private void loadDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_structFile == null || _structFile.Structs.Count == 0) return;
            if (_openDataDialog.ShowDialog(this) == DialogResult.OK)
            {
                _dataView.LoadData(Path.GetFullPath(_openDataDialog.FileName), _structFile.Structs[0]);
            }
        }
    }
}