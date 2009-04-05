using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Structorian.UI
{
    public class WindowsAPI
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

        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct POINTAPI
        {
            public POINTAPI(System.Drawing.Point p) { x = p.X; y = p.Y; }
            public POINTAPI(Int32 X, Int32 Y) { x = X; y = Y; }
            public POINTAPI(Int32 dw)
            {
                x = dw & 0xFFFF;
                y = (dw >> 16) & 0xFFFF;
            }

            public Int32 x;
            public Int32 y;
        }

        static int TV_FIRST = 0x1100;
        static int TVM_SETITEMA = (TV_FIRST + 13);
        public static int WM_MOUSEWHEEL = 0x20A;

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
        [DllImport("user32", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        [DllImport("user32")]
        public static extern IntPtr WindowFromPoint(POINTAPI point);

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
