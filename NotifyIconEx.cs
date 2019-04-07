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
        public delegate void ContextMenuShowingHandler(object sender, MenuEventHandler args);
        public event ContextMenuShowingHandler ContextMenuShowing;

        public NotifyIcon Tray { get; private set; }
        public ContextMenuStrip LeftClickMenu { get; set; }
        public ContextMenuStrip RightClickMenu { get; set; }
        public Icon Icon {
            get => Tray.Icon;
            set => Tray.Icon = value;
        }
        public string Text {
            get => Tray.Text;
            set => Tray.Text = value;
        }
        public ToolTipIcon BalloonTipIcon {
            get => Tray.BalloonTipIcon;
            set => Tray.BalloonTipIcon = value;
        }
        public string BalloonTipText {
            get => Tray.BalloonTipText;
            set => Tray.BalloonTipText = value;
        }
        public string BalloonTipTitle {
            get => Tray.BalloonTipTitle;
            set => Tray.BalloonTipTitle = value;
        }
        public bool Visible {
            get => Tray.Visible;
            set => Tray.Visible = value;
        }
        public object Tag {
            get => Tray.Tag;
            set => Tray.Tag = value;
        }
        MethodInfo showContextMenu = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);

        public NotifyIconEx(IContainer c = null) {
            if (c != null)
                Tray = new NotifyIcon(c);
            else
                Tray = new NotifyIcon();
            Tray.MouseUp += click;
        }
        protected override void Dispose(bool disposing) {
            ContextMenuShowing = null;
            Tray.Dispose();
            LeftClickMenu.Dispose();
            RightClickMenu.Dispose();
            base.Dispose(disposing);
        }

        void click(object sender, MouseEventArgs e) {
            Tray.ContextMenuStrip = null;

            if (e.Button == MouseButtons.Left && LeftClickMenu.Items.Count > 0)
                Tray.ContextMenuStrip = LeftClickMenu;
            else if (e.Button == MouseButtons.Right && RightClickMenu.Items.Count > 0)
                Tray.ContextMenuStrip = RightClickMenu;

            var a = new MenuEventHandler(Tray.ContextMenuStrip);
            ContextMenuShowing?.Invoke(this, a);
            if (!a.Cancel && Tray.ContextMenuStrip != null)
                showContextMenu.Invoke(Tray, null);
        }
    }

    public class MenuEventHandler : EventArgs {
        public ContextMenuStrip Menu { get; private set; }
        public bool Cancel { get; set; }

        public MenuEventHandler(ContextMenuStrip menu)
            => Menu = menu;
    }
}
