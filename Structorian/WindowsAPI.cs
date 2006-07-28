using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Structorian
{
    class WindowsAPI
    {
        public enum TreeViewItemFlags
        {
            TEXT = 0x0001,
            IMAGE = 0x0002,
            PARAM = 0x0004,
            STATE = 0x0008,
            HANDLE = 0x0010,
            SELECTEDIMAGE = 0x0020,
            CHILDREN = 0x0040
        }

        static int TV_FIRST = 0x1100;
        static int TVM_SETITEMA = (TV_FIRST + 13);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct TVITEM
        {
            public TreeViewItemFlags mask;
            public IntPtr hItem;
            public int state;
            public int stateMask;
            public IntPtr pszText;
            public int cchTextMax;
            public int iImage;
            public int iSelectedImage;
            public int cChildren;
            public int lParam;
        }

        [DllImport("user32", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, ref TVITEM lParam);

        public static void SetHasChildren(TreeNode node, bool hasChildren)
        {
            TVITEM item = new TVITEM();
            item.mask = TreeViewItemFlags.CHILDREN;
            item.hItem = node.Handle;
            item.cChildren = hasChildren ? 1 : 0;
            SendMessage(node.TreeView.Handle, TVM_SETITEMA, 0, ref item);
        }
    }
}
