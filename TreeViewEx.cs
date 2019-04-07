using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Micro.WinForms {
    public class TreeViewEx : TreeView {
        #region WinAPI
        const int
            LVM_FIRST = 0x1000,
            LVM_SCROLL = LVM_FIRST + 20,
            LVM_GETCOLUMNORDERARRAY = LVM_FIRST + 59,
            WM_HSCROLL = 0x114,
            WM_VSCROLL = 0x115,
            WM_SIZE = 0x05,
            WM_NOTIFY = 0x4E,
            HDN_FIRST = -300,
            HDN_BEGINDRAG = HDN_FIRST - 10,
            HDN_ITEMCHANGINGA = HDN_FIRST,
            HDN_ITEMCHANGINGW = HDN_FIRST - 20,
            TVM_SETEXTENDEDSTYLE = 0x1100 + 44,
            TVM_GETEXTENDEDSTYLE = 0x1100 + 45,
            TVS_EX_DOUBLEBUFFER = 0x0004;

        [DllImport("user32.dll")]
        static extern int GetScrollPos(IntPtr hWnd, Orientation nBar);
        [DllImport("user32.dll")]
        static extern int SetScrollPos(IntPtr hWnd, Orientation nBar, int nPos, bool bRedraw);
        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        static extern bool LockWindowUpdate(IntPtr Handle);

        #endregion

        public Point ScrollPosition {
            get => new Point(
                GetScrollPos(Handle, Orientation.Horizontal),
                GetScrollPos(Handle, Orientation.Vertical)
            );
            set {
                LockWindowUpdate(Handle);
                SetScrollPos(Handle, Orientation.Horizontal, value.X, false);
                SetScrollPos(Handle, Orientation.Vertical, value.Y, true);
                LockWindowUpdate(IntPtr.Zero);
            }
        }

        public TreeViewEx() { }
        protected override void OnHandleCreated(EventArgs e) {
            SendMessage(Handle, TVM_SETEXTENDEDSTYLE, (IntPtr)TVS_EX_DOUBLEBUFFER, (IntPtr)TVS_EX_DOUBLEBUFFER);
            base.OnHandleCreated(e);
        }
    }
}
