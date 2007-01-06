using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ICSharpCode.TextEditor.Document;
using Structorian.Engine;
using Structorian.Properties;

namespace Structorian
{
    public partial class MainForm : Form
    {
        private string _structFileName;
        private StructFile _structFile;
        private DataView _dataView;
        private TextMarker _currentCellMarker;
        private Settings _settings = new Settings();
        private bool _structuresModified = false;
        
        public MainForm()
        {
            InitializeComponent();
            _dataView = new DataView();
            _dataView.CellSelected += new CellSelectedEventHandler(_dataView_OnCellSelected);
            _dataView.Dock = DockStyle.Fill;
            splitContainer2.Panel2.Controls.Add(_dataView);
            
            Application.AddMessageFilter(new WheelMessageFilter());
            string lastStrsFile = _settings.LastStrsFile;
            if (lastStrsFile != null && lastStrsFile.Length > 0)
                LoadStructsFile(lastStrsFile);
            _structEditControl.Document.DocumentChanged += delegate { _structuresModified = true; };

            RestoreFormPosition();
        }

        private void RestoreFormPosition()
        {
            if (_settings.MainFormMaximized)
                WindowState = FormWindowState.Maximized;
            else
            {
                if (_settings.PropertyValues ["MainFormLocation"].PropertyValue != null)
                    Location = _settings.MainFormLocation;
                if (_settings.PropertyValues["MainFormSize"].PropertyValue != null)
                    Size = _settings.MainFormSize;
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!CheckSaveStructures())
            {
                e.Cancel = true;
                return;
            }
            
            if (WindowState == FormWindowState.Maximized)
                _settings.MainFormMaximized = true;
            else
            {
                _settings.MainFormMaximized = false;
                _settings.MainFormLocation = Location;
                _settings.MainFormSize = Size;
            }
            _settings.Save();
        }
        
        private void loadStructuresToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_openStructsDialog.ShowDialog(this) == DialogResult.OK)
            {
                LoadStructsFile(_openStructsDialog.FileName);
                _settings.LastStrsFile = _openStructsDialog.FileName;
                _settings.Save();
            }
        }

        private void LoadStructsFile(string name)
        {
            if (!CheckSaveStructures()) return;
            _structFileName = Path.GetFullPath(name);
            using(Stream stream = new FileStream(name, FileMode.Open))
            {
                string strs = new StreamReader(stream).ReadToEnd();
                _structEditControl.Text = strs;
                _structEditControl.ShowEOLMarkers = false;
                _structEditControl.ShowInvalidLines = false;
                _structEditControl.ShowSpaces = false;
                _structuresModified = false;
                ParseStructures();
            }
            _btnSaveStructures.Enabled = true;
            Text = Path.GetFileName(name) + " - Structorian";
        }

        private bool CheckSaveStructures()
        {
            if (!_structuresModified) 
                return true;
            if (_structFileName == null) 
                return true;
            
            DialogResult dr = MessageBox.Show(
                "The file " + _structFileName + " has been modified. Would you like to save the changes?",
                "Structorian",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Warning);
            if (dr == DialogResult.Cancel) 
                return false;
            if (dr == DialogResult.Yes)
                SaveStructuresToDisk();
            return true;
        }

        private void ParseStructures()
        {
            StructParser parser = new StructParser();
            try
            {
                StructSourceContext context = new StructSourceContext();
                context.BaseDirectory = Path.GetDirectoryName(_structFileName);
                context.AddSourceText(_structFileName, _structEditControl.Text);
                _structFile = parser.LoadStructs(_structFileName, context);
            }
            catch(ParseException ex)
            {
                MessageBox.Show(this, "Error in " + ex.Position + ": " + ex.Message);
                List<ParseException> list = new List<ParseException>();
                list.Add(ex);
                HighlightErrors(list.AsReadOnly());
                return;
            }
            catch(Exception ex)
            {
                MessageBox.Show(this, "Error loading structures: " + ex.Message);
            }
            HighlightErrors(parser.Errors);
            if (parser.Errors.Count > 0)
            {
                ParseException ex = parser.Errors[0];
                MessageBox.Show(this, "Error in " + ex.Position + ": " + ex.Message);
            }
        }

        private void _btnSaveStructures_Click(object sender, EventArgs e)
        {
            SaveStructuresToDisk();
            ParseStructures();
            
            if (_structFile != null)
                _dataView.ReloadData(_structFile.Structs [0], true);
        }

        private void SaveStructuresToDisk()
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
            _structuresModified = false;
        }

        private void loadDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_structFile == null || _structFile.Structs.Count == 0) return;
            if (_openDataDialog.ShowDialog(this) == DialogResult.OK)
            {
                string fileName = _openDataDialog.FileName;
                _dataView.LoadData(Path.GetFullPath(fileName), FindMatchingStruct(fileName));
            }
        }

        private StructDef FindMatchingStruct(string fileName)
        {
            foreach(StructDef def in _structFile.Structs)
            {
                string fileMask = def.FileMask;
                if (fileMask == null)
                    continue;
                string rx = fileMask.Replace(".", "\\.").Replace("*", ".+").Replace("?", ".");
                if (new Regex(rx, RegexOptions.IgnoreCase).IsMatch(fileName))
                {
                    return def;
                }
            }
            return _structFile.Structs[0];
        }

        private void showLocalOffsetsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _dataView.ShowLocalOffsets = !_dataView.ShowLocalOffsets;
            ((ToolStripMenuItem) sender).Checked = _dataView.ShowLocalOffsets;
        }

        private void _dataView_OnCellSelected(object sender, CellSelectedEventArgs e)
        {
            TextPosition pos = e.Cell.GetStructDef().Position;
            TextPosition endPos = e.Cell.GetStructDef().EndPosition;
            IDocument doc = _structEditControl.Document;
            
            if (_currentCellMarker != null)
                doc.MarkerStrategy.RemoveMarker(_currentCellMarker);
            int offset = doc.PositionToOffset(new Point(pos.Col, pos.Line-1));
            int endOffset = doc.PositionToOffset(new Point(endPos.Col, endPos.Line - 1));
            if (offset != endOffset)
            {
                _currentCellMarker = new TextMarker(offset, endOffset - offset, TextMarkerType.SolidBlock,
                                                    Color.LightSkyBlue);
                doc.MarkerStrategy.AddMarker(_currentCellMarker);
            }
            _structEditControl.ActiveTextAreaControl.ScrollTo(pos.Line-1);
            _structEditControl.Refresh();
        }

        private void HighlightErrors(ReadOnlyCollection<ParseException> exceptions)
        {
            IDocument doc = _structEditControl.Document;
            doc.MarkerStrategy.RemoveAll(
                delegate(TextMarker m) { return m.TextMarkerType == TextMarkerType.WaveLine; } );
            foreach(ParseException ex in exceptions)
            {
                int offset = doc.PositionToOffset(new Point(ex.Position.Col, ex.Position.Line - 1));
                TextMarker marker = new TextMarker(offset, ex.Length, TextMarkerType.WaveLine, Color.Red);
                marker.ToolTip = ex.Message;
                doc.MarkerStrategy.AddMarker(marker);
            }
            if (exceptions.Count > 0)
            {
                TextPosition pos = exceptions [0].Position;
                _structEditControl.ActiveTextAreaControl.Caret.Position = new Point(pos.Col, pos.Line-1);
            }
            _structEditControl.Refresh();
        }
        
        private class WheelMessageFilter : IMessageFilter
        {
            public bool PreFilterMessage(ref Message m)
            {
                if (m.Msg == WindowsAPI.WM_MOUSEWHEEL)
                {
                    WindowsAPI.POINTAPI ptapi = new WindowsAPI.POINTAPI(m.LParam.ToInt32());
                    IntPtr pWnd = WindowsAPI.WindowFromPoint(ptapi);
                    if (pWnd.ToInt32() != 0)
                    {
                        WindowsAPI.SendMessage(pWnd, WindowsAPI.WM_MOUSEWHEEL, m.WParam.ToInt32(), m.LParam.ToInt32());
                        return true;
                    }
                }
                return false;
            }
        }
    }
}