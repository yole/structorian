using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Structorian.Engine;

namespace Structorian
{
    public partial class DataView : UserControl
    {
        private string _dataFileName;
        private StructDef _rootStructDef;
        private InstanceTree _instanceTree;
        private InstanceTreeNode _activeInstance;
        private readonly Dictionary<InstanceTreeNode, TreeNode> _nodeMap = new Dictionary<InstanceTreeNode, TreeNode>();
        private readonly HexDump _hexDump;
        private bool _showLocalOffsets;
        private Stream _mainStream;
        private Control _nodeControl;

        public event CellSelectedEventHandler CellSelected;

        public DataView()
        {
            InitializeComponent();
            _hexDump = new HexDump
                           {
                               Font = new Font("Lucida Console", 9),
                               BackColor = SystemColors.Window,
                               Dock = DockStyle.Fill
                           };
            splitContainer2.Panel2.Controls.Add(_hexDump);
        }

        public TreeView StructTreeView
        {
            get { return _structTreeView; }
        }

        public DataGridView StructGridView
        {
            get { return _structGridView; }
        }

        internal HexDump HexDump
        {
            get { return _hexDump; }
        }

        public bool ShowLocalOffsets
        {
            get { return _showLocalOffsets; }
            set
            {
                _showLocalOffsets = value;
                _structGridView.Invalidate();
            }
        }

        public InstanceTree InstanceTree
        {
            get { return _instanceTree; }
        }

        public string DataFileName
        {
            get { return _dataFileName; }
        }

        public void LoadData(string fileName, StructDef def)
        {
            _dataFileName = fileName;
            ReloadData(def, false);
        }

        internal void ReloadData(StructDef def, bool keepState)
        {
            if (_dataFileName == null)
                return;
            
            DataViewState viewState = null;
            if (keepState)
                viewState = DataViewState.Save(this);
            
            _rootStructDef = def;
            _mainStream = new BufferedStream(new FileStream(_dataFileName, FileMode.Open, FileAccess.Read, FileShare.Read), 16384);
            if (_instanceTree != null)
            {
                _instanceTree.InstanceAdded -= HandleInstanceAdded;
                _instanceTree.NodeNameChanged -= HandleNodeNameChanged;
                _nodeMap.Clear();
            }
            _instanceTree = _rootStructDef.LoadData(_mainStream);
            _instanceTree.InstanceAdded += HandleInstanceAdded;
            _instanceTree.NodeNameChanged += HandleNodeNameChanged;
            FillStructureTree();
            _hexDump.Stream = _mainStream;
            
            if (viewState != null)
                viewState.Restore(this);
        }

        private void HandleInstanceAdded(object sender, InstanceAddedEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new InstanceAddedEventHandler(HandleInstanceAdded), sender, e);
                return;
            }
            
            if (e.Parent is InstanceTree)
                AddInstanceNode(null, e.Child);
            else
            {
                TreeNode parent;
                if (_nodeMap.TryGetValue(e.Parent, out parent))
                {
                    AddInstanceNode(parent, e.Child);
                }
            }
        }

        private void HandleNodeNameChanged(object sender, NodeNameChangedEventArgs e)
        {
            TreeNode node = _nodeMap[e.Node];
            node.Text = AppendSequenceIndex(e.Node);
        }

        private static string AppendSequenceIndex(InstanceTreeNode node)
        {
            string name = node.NodeName;
            var structInstance = node as StructInstance;
            if (structInstance != null && structInstance.SequenceIndex >= 0)
            {
                name = structInstance.SequenceIndex + ". " + name;
            }
            return name;
        }

        private void FillStructureTree()
        {
            _structTreeView.Nodes.Clear();
            foreach (InstanceTreeNode instance in _instanceTree.Children)
            {
                AddInstanceNode(null, instance);
            }
        }

        private void AddInstanceNode(TreeNode parent, InstanceTreeNode instance)
        {
            TreeNode node;
            if (parent == null)
                node = _structTreeView.Nodes.Add(instance.NodeName + " (" + Path.GetFileName(_dataFileName) + ")");
            else
            {
                node = parent.Nodes.Add(AppendSequenceIndex(instance));
            }

            _nodeMap.Add(instance, node);
            node.Tag = instance;
            if (instance.HasChildren)
            {
                WindowsAPI.SetHasChildren(node, true);
            }
        }

        private void _structTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            _activeInstance = (InstanceTreeNode)e.Node.Tag;
            if (_nodeControl != null)
            {
                _nodeControl.Parent.Controls.Remove(_nodeControl);
                _nodeControl.Dispose();
                _nodeControl = null;
            }
            NodeUI ui = FindNodeUI(_activeInstance);
            if (ui != null)
            {
                _nodeControl = ui.CreateControl();
                _nodeControl.Dock = DockStyle.Fill;
                splitContainer1.Panel2.Controls.Add(_nodeControl);
                _structGridView.Visible = false;
            }
            else
            {
                _structGridView.Visible = true;
                _structGridView.DataSource = _activeInstance.Cells;
            }
            StructInstance instance = _activeInstance as StructInstance;
            if (instance == null)
                _hexDump.Stream = _mainStream;
            else
                _hexDump.Stream = instance.Stream;
        }

        private static NodeUI FindNodeUI(InstanceTreeNode instance)
        {
            foreach(var cell in instance.Cells)
            {
                var ui = NodeUIRegistry.GetNodeUI(cell);
                if (ui != null) return ui;
            }
            return null;
        }

        private void _structTreeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.Nodes.Count == 0)
            {
                InstanceTreeNode instance = (InstanceTreeNode)e.Node.Tag;
                instance.NeedChildren();
                if (instance.Children.Count == 0)
                    WindowsAPI.SetHasChildren(e.Node, false);
            }
        }

        private void _structGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (_structGridView.SelectedRows.Count > 0)
            {
                StructCell cell = (StructCell)_structGridView.SelectedRows[0].DataBoundItem;
                int offset = cell.Offset;
                if (offset >= 0)
                {
                    int dataSize = cell.GetDataSize((StructInstance) _activeInstance);
                    if (dataSize <= 0)
                        dataSize = 1;
                    _hexDump.SelectBytes(offset, dataSize);
                }
                if (CellSelected != null)
                    CellSelected(this, new CellSelectedEventArgs(cell));
            }
        }

        private void _structGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex == 0 && _showLocalOffsets && _activeInstance is StructInstance)
            {
                StructInstance instance = (StructInstance) _activeInstance;
                e.Value = ((int) e.Value - instance.Offset).ToString();
                e.FormattingApplied = true;
            }
            else if (e.ColumnIndex == 1)
            {
                var cell = (StructCell)_structGridView.Rows[e.RowIndex].DataBoundItem;
                if (e.Value == null)
                {
                    e.Value = cell.GetStructDef().Name;
                    e.CellStyle.ForeColor = Color.DarkGray;
                    e.FormattingApplied = true;
                }
            }
        }

        private void _structGridView_CellContextMenuStripNeeded(object sender, DataGridViewCellContextMenuStripNeededEventArgs e)
        {
            if (_structGridView.SelectedRows.Count > 0)
            {
                StructCell cell = (StructCell) _structGridView.SelectedRows[0].DataBoundItem;
                CellUI ui = CellUIRegistry.GetUI(cell);
                if (ui != null)
                {
                    ui.ContextMenuStripNeeded(e);
                }
                else
                {
                    e.ContextMenuStrip = contextMenuStrip1;
                }
            }
        }

        private void miFollowOffset_Click(object sender, EventArgs e)
        {
            var cell = (StructCell)_structGridView.SelectedRows[0].DataBoundItem;
            IConvertible value = cell.GetValue();
            long offset = value.ToInt64(CultureInfo.CurrentCulture);
            _hexDump.SelectBytes(offset, 1);
        }
    }

    public class CellSelectedEventArgs: EventArgs
    {
        private readonly StructCell _cell;

        public CellSelectedEventArgs(StructCell cell)
        {
            _cell = cell;
        }

        public StructCell Cell
        {
            get { return _cell; }
        }
    }
    
    public delegate void CellSelectedEventHandler(object sender, CellSelectedEventArgs e);
}
