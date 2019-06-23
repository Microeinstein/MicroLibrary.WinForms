using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Micro.WinForms {
    public class NotifyIconEx : Component {
        public delegate void ContextMenuShowingHandler(object sender, MenuEventArgs args);
        public event ContextMenuShowingHandler ContextMenuShowing;

        public NotifyIcon Wrapped { get; private set; }
        public ContextMenuStrip LeftClickMenu { get; set; }
        public ContextMenuStrip RightClickMenu { get; set; }
        public Icon Icon {
            get => Wrapped.Icon;
            set => Wrapped.Icon = value;
        }
        public string Text {
            get => Wrapped.Text;
            set => Wrapped.Text = value;
        }
        public ToolTipIcon BalloonTipIcon {
            get => Wrapped.BalloonTipIcon;
            set => Wrapped.BalloonTipIcon = value;
        }
        public string BalloonTipText {
            get => Wrapped.BalloonTipText;
            set => Wrapped.BalloonTipText = value;
        }
        public string BalloonTipTitle {
            get => Wrapped.BalloonTipTitle;
            set => Wrapped.BalloonTipTitle = value;
        }
        public bool Visible {
            get => Wrapped.Visible;
            set => Wrapped.Visible = value;
        }
        public object Tag {
            get => Wrapped.Tag;
            set => Wrapped.Tag = value;
        }
        static readonly MethodInfo
            showContextMenu = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);

        public NotifyIconEx(IContainer c = null) {
            if (c != null)
                Wrapped = new NotifyIcon(c);
            else
                Wrapped = new NotifyIcon();
            Wrapped.MouseUp += click;
        }
        protected override void Dispose(bool disposing) {
            ContextMenuShowing = null;
            Wrapped.Dispose();
            LeftClickMenu.Dispose();
            RightClickMenu.Dispose();
            base.Dispose(disposing);
        }

        public void Click(MouseEventArgs e)
            => click(Wrapped, e);
        void click(object sender, MouseEventArgs e) {
            Wrapped.ContextMenuStrip = null;

            if (e.Button == MouseButtons.Left && LeftClickMenu.Items.Count > 0)
                Wrapped.ContextMenuStrip = LeftClickMenu;
            else if (e.Button == MouseButtons.Right && RightClickMenu.Items.Count > 0)
                Wrapped.ContextMenuStrip = RightClickMenu;

            var a = new MenuEventArgs(Wrapped.ContextMenuStrip);
            ContextMenuShowing?.Invoke(this, a);
            if (!a.Cancel && Wrapped.ContextMenuStrip != null)
                showContextMenu.Invoke(Wrapped, null);
        }
    }

    public class MenuEventArgs : EventArgs {
        public ContextMenuStrip Menu { get; private set; }
        public bool Cancel { get; set; }

        public MenuEventArgs(ContextMenuStrip menu)
            => Menu = menu;
    }
}
