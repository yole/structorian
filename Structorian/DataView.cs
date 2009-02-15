using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32.SafeHandles;
using Structorian.Engine;

namespace Structorian
{
    public partial class DataView : UserControl
    {
        private class DataFile
        {
            private readonly string _name;

            public DataFile(string name, StructDef def)
            {
                _name = name;
                RootStructDef = def;
            }

            public string Name
            {
                get { return _name; }
            }

            public StructDef RootStructDef { get; set; }
            public Stream Stream { get; set; }
            public InstanceTree InstanceTree { get; set; }
        }

        private readonly List<DataFile> _dataFiles = new List<DataFile>();
        private InstanceTreeNode _activeInstance;
        private readonly Dictionary<InstanceTreeNode, TreeNode> _nodeMap = new Dictionary<InstanceTreeNode, TreeNode>();
        private readonly HexDump _hexDump;
        private bool _showLocalOffsets;
        private Control _nodeControl;
        private TreeNode _searchResultsRoot;
        private readonly List<InstanceAddedEventArgs> _pendingNodes = new List<InstanceAddedEventArgs>();
        private readonly List<InstanceTreeNode> _nameChangedNodes = new List<InstanceTreeNode>();
        private readonly HexDump.Highlighter _currentStructureHighlighter;

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
            _currentStructureHighlighter = _hexDump.AddHighlighter(null, Color.LightGoldenrodYellow);
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

        public InstanceTree ActiveInstanceTree
        {
            get { return _activeInstance != null ? _activeInstance.GetInstanceTree() : null; }
        }

        public List<InstanceTree> InstanceTrees
        {
            get { return _dataFiles.ConvertAll(f => f.InstanceTree); }
        }

        private DataFile FindDataFile(InstanceTreeNode instance)
        {
            InstanceTree tree = instance.GetInstanceTree();
            return _dataFiles.Find(f => f.InstanceTree == tree);
        }

        public void LoadData(string fileName, StructDef def)
        {
            var dataFile = new DataFile(fileName, def);
            _dataFiles.Add(dataFile);
            LoadDataFile(dataFile);
        }

        private void LoadDataFile(DataFile dataFile)
        {
            dataFile.Stream = new BufferedStream(new FileStream(dataFile.Name, FileMode.Open, FileAccess.Read, FileShare.Read), 16384);
            dataFile.InstanceTree = dataFile.RootStructDef.LoadData(dataFile.Stream);
            dataFile.InstanceTree.InstanceAdded += HandleInstanceAdded;
            dataFile.InstanceTree.NodeNameChanged += HandleNodeNameChanged;
            FillStructureTree(dataFile.InstanceTree);
            _hexDump.Stream = dataFile.Stream;
            
        }

        public void ReloadData(Func<string, StructDef> structMatcher)
        {
            DataViewState viewState = DataViewState.Save(this);
            _structTreeView.BeginUpdate();
            try
            {
                _structTreeView.Nodes.Clear();
                _nodeMap.Clear();
                _searchResultsRoot = null;
                foreach (DataFile f in _dataFiles)
                {
                    f.RootStructDef = structMatcher.Invoke(f.Name);
                    LoadDataFile(f);
                }
                if (viewState != null)
                    viewState.Restore(this);
            }
            finally
            {
                _structTreeView.EndUpdate();
            }
        }

        private void HandleInstanceAdded(object sender, InstanceAddedEventArgs e)
        {
            if (InvokeRequired)
            {
                _pendingNodes.Add(e);
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
            UpdateNodeName(e.Node);
        }

        private void UpdateNodeName(InstanceTreeNode instance)
        {
            TreeNode node;
            if (_nodeMap.TryGetValue(instance, out node))
            {
                if (InvokeRequired)
                {
                    _nameChangedNodes.Add(instance);
                }
                else
                {
                    node.Text = AppendSequenceIndex(instance);
                }
            }
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

        public void FlushPendingNodes()
        {
            _structTreeView.BeginUpdate();
            try
            {
                _pendingNodes.ForEach(n => HandleInstanceAdded(this, n));
                _nameChangedNodes.ForEach(UpdateNodeName);
                _pendingNodes.Clear();
                _nameChangedNodes.Clear();
            }
            finally
            {
                _structTreeView.EndUpdate();
            }
        }

        private void FillStructureTree(InstanceTree instanceTree)
        {
            foreach (InstanceTreeNode instance in instanceTree.Children)
            {
                AddInstanceNode(null, instance);
            }
        }

        private void AddInstanceNode(TreeNode parent, InstanceTreeNode instance)
        {
            TreeNode node;
            if (parent == null)
            {
                string fileName = FindDataFile(instance).Name;
                node = _structTreeView.Nodes.Add(instance.NodeName + " (" + Path.GetFileName(fileName) + ")");
            }
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
            _structTreeView.BeginUpdate();
            try
            {
                if (_activeInstance == null)
                {
                    _structGridView.DataSource = new List<StructCell>();
                }
                else
                {
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
                    // while we're in BeginUpdate, pre-evaluate all cell values
                    _activeInstance.Cells.ToList().ForEach(cell => cell.Value.ToString());
                }
            }
            finally
            {
                _structTreeView.EndUpdate();
            }
            var instance = _activeInstance as StructInstance;
            if (instance == null)
            {
                InstanceTree tree = ActiveInstanceTree;
                if (tree != null)
                {
                    _hexDump.Stream = FindDataFile(tree).Stream;
                }
                _currentStructureHighlighter.SetRange(-1, -1);
            }
            else
            {
                _hexDump.Stream = instance.Stream;
                _currentStructureHighlighter.SetRange(instance.Offset, instance.EndOffset);
            }
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
                _structTreeView.BeginUpdate();
                try
                {
                    instance.NeedChildren();
                }
                finally
                {
                    _structTreeView.EndUpdate();
                }
                if (instance.Children.Count == 0)
                    WindowsAPI.SetHasChildren(e.Node, false);
            }
        }

        private void _structGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (_structGridView.SelectedRows.Count > 0)
            {
                var cell = (StructCell)_structGridView.SelectedRows[0].DataBoundItem;
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

        private void closeDataFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var tree = ActiveInstanceTree;
            if (tree != null)
            {
                foreach(var node in tree.Children)
                {
                    _structTreeView.Nodes.Remove(_nodeMap [node]);
                }
                var nodesToRemove = new List<InstanceTreeNode>();
                foreach(var node in _nodeMap.Keys)
                {
                    if (node.GetInstanceTree() == tree)
                        nodesToRemove.Add(node);
                }
                foreach(InstanceTreeNode node in nodesToRemove)
                {
                    _nodeMap.Remove(node);
                }
                tree.InstanceAdded -= HandleInstanceAdded;
                tree.NodeNameChanged -= HandleNodeNameChanged;
                var dataFile = FindDataFile(tree);
                _dataFiles.Remove(dataFile);
                dataFile.Stream.Close();
                if (_dataFiles.Count == 0)
                {
                    _hexDump.Stream = null;
                }
            }
        }

        public void ShowSearchResults(List<InstanceTreeNode> results)
        {
            if (results.Count == 1)
            {
                if (_searchResultsRoot != null)
                {
                    _searchResultsRoot.Nodes.Clear();
                    _searchResultsRoot.Remove();
                    _searchResultsRoot = null;
                }
                _structTreeView.SelectedNode = _nodeMap[results[0]];
            }
            else
            {
                ShowSearchResultsNode(results);
            }
        }

        private void ShowSearchResultsNode(List<InstanceTreeNode> results)
        {
            if (_searchResultsRoot == null)
            {
                _searchResultsRoot = _structTreeView.Nodes.Add("Search Results");
            }
            else
            {
                _searchResultsRoot.Nodes.Clear();
            }
            foreach (InstanceTreeNode result in results)
            {
                var resultNode = _searchResultsRoot.Nodes.Add(result.NodeName);
                resultNode.Tag = result;
            }
            _structTreeView.SelectedNode = _searchResultsRoot;
        }

        private void contextMenuStrip2_Opening(object sender, CancelEventArgs e)
        {
            var node = _structTreeView.SelectedNode;
            miShowInStructureTree.Visible = (node != null && node.Parent == _searchResultsRoot);
        }

        private void miShowInStructureTree_Click(object sender, EventArgs e)
        {
            var node = (InstanceTreeNode) _structTreeView.SelectedNode.Tag;
            _structTreeView.SelectedNode = _nodeMap[node];
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
