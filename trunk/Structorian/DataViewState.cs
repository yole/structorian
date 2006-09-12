using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Structorian.Engine;

namespace Structorian
{
    class DataViewState
    {
        private List<int> _selectedIndices = new List<int>();
        
        private DataViewState()
        {
        }
        
        public static DataViewState Save(DataView view)
        {
            DataViewState result = new DataViewState();
            TreeNode node = view.StructTreeView.SelectedNode;
            while(node != null)
            {
                result._selectedIndices.Insert(0, node.Index);
                node = node.Parent;
            }
            return result;
        }
        
        public void Restore(DataView view)
        {
            if (_selectedIndices.Count > 0)
            {
                TreeNodeCollection nodeCollection = view.StructTreeView.Nodes;
                TreeNode node;
                TreeNode nodeToSelect = null;
                foreach(int index in _selectedIndices)
                {
                    node = LoadNode(nodeCollection, index);
                    if (node == null) break;
                    nodeToSelect = node;
                    node.Expand();
                    nodeCollection = node.Nodes;
                }
                view.StructTreeView.SelectedNode = nodeToSelect;
            }
        }

        private TreeNode LoadNode(TreeNodeCollection nodes, int index)
        {
            int lastLoadedIndex = 0;
            while(index >= nodes.Count && lastLoadedIndex < nodes.Count)
            {
                InstanceTreeNode instance = (InstanceTreeNode) nodes [lastLoadedIndex].Tag;
                instance.NeedData();
                lastLoadedIndex++;
            }
            if (index < nodes.Count)
                return nodes[index];
            return null;
        }
    }
}
