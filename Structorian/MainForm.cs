using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ICSharpCode.TextEditor.Document;
using Structorian.Engine;

namespace Structorian
{
    public partial class MainForm : Form
    {
        private string _structFileName;
        private StructFile _structFile;
        private DataView _dataView;
        private TextMarker _currentCellMarker;
        
        public MainForm()
        {
            InitializeComponent();
            _dataView = new DataView();
            _dataView.CellSelected += new CellSelectedEventHandler(_dataView_OnCellSelected);
            _dataView.Dock = DockStyle.Fill;
            splitContainer2.Panel2.Controls.Add(_dataView);
            
            Application.AddMessageFilter(new WheelMessageFilter());
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
                    _structEditControl.ShowSpaces = false;
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
            
            if (_structFile != null)
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

        private void _dataView_OnCellSelected(object sender, CellSelectedEventArgs e)
        {
            TextPosition pos = e.Cell.GetStructDef().Position;
            TextPosition endPos = e.Cell.GetStructDef().EndPosition;
            IDocument doc = _structEditControl.Document;
            
            if (_currentCellMarker != null)
                doc.MarkerStrategy.RemoveMarker(_currentCellMarker);
            int offset = doc.PositionToOffset(new Point(pos.Col, pos.Line-1));
            int endOffset = doc.PositionToOffset(new Point(endPos.Col, endPos.Line - 1));
            _currentCellMarker = new TextMarker(offset, endOffset-offset, TextMarkerType.SolidBlock, 
                                                Color.LightSkyBlue);
            doc.MarkerStrategy.AddMarker(_currentCellMarker);
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