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
        private StructFile _structFile;
        private InstanceTree _instanceTree;
        private InstanceTreeNode _activeInstance;
        private Dictionary<InstanceTreeNode, TreeNode> _nodeMap = new Dictionary<InstanceTreeNode, TreeNode>();
        
        public MainForm()
        {
            InitializeComponent();
        }

        private void loadStructuresToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_openStructsDialog.ShowDialog(this) == DialogResult.OK)
            {
                StructParser parser = new StructParser();
                using(Stream stream = _openStructsDialog.OpenFile())
                {
                    string strs = new StreamReader(stream).ReadToEnd();
                    try
                    {
                        _structFile = parser.LoadStructs(strs);
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(this, "Error loading structures: " + ex.Message);
                    }
                }
            }
        }

        private void loadDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_structFile == null || _structFile.Structs.Count == 0) return;
            if (_openDataDialog.ShowDialog(this) == DialogResult.OK)
            {
                Stream stream = _openDataDialog.OpenFile();
                if (_instanceTree != null)
                {
                    _instanceTree.InstanceAdded -= new InstanceAddedEventHandler(HandleInstanceAdded);
                    _instanceTree.NodeNameChanged -= new NodeNameChangedEventHandler(HandleNodeNameChanged);
                    _nodeMap.Clear();
                }
                _instanceTree = _structFile.Structs[0].LoadData(stream);
                _instanceTree.InstanceAdded += new InstanceAddedEventHandler(HandleInstanceAdded);
                _instanceTree.NodeNameChanged += new NodeNameChangedEventHandler(HandleNodeNameChanged);
                FillStructureTree();
            }
        }

        private void HandleInstanceAdded(object sender, InstanceAddedEventArgs e)
        {
            if (e.Parent is InstanceTree)
                AddInstanceNode(null, e.Child);
            else
            {
                TreeNode parent = _nodeMap[e.Parent];
                AddInstanceNode(parent, e.Child);
            }
        }

        private void HandleNodeNameChanged(object sender, NodeNameChangedEventArgs e)
        {
            TreeNode node = _nodeMap[e.Node];
            node.Text = e.Node.NodeName;
        }

        private void FillStructureTree()
        {
            _structTreeView.Nodes.Clear();
            foreach(InstanceTreeNode instance in _instanceTree.Children)
            {
                AddInstanceNode(null, instance);
            }
        }

        private void AddInstanceNode(TreeNode parent, InstanceTreeNode instance)
        {
            TreeNode node;
            if (parent == null)
                node = _structTreeView.Nodes.Add(instance.NodeName);
            else
                node = parent.Nodes.Add(instance.NodeName);
            
            _nodeMap.Add(instance, node);
            node.Tag = instance;
            if (instance.HasChildren)
            {
                WindowsAPI.SetHasChildren(node, true);
            }
        }

        private void _structTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            _activeInstance = (InstanceTreeNode) e.Node.Tag;
            _structGridView.DataSource = _activeInstance.Cells;
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
    }
}