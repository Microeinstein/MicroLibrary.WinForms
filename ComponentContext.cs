using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Micro.WinForms {
    public abstract class ComponentContext : ApplicationContext {
        public readonly Component Component;
        public readonly CloseHandler closeGrab;

        public ComponentContext(Component component) {
            closeGrab = new CloseHandler();
            closeGrab.WM_CLOSE += close;
            closeGrab.WM_CLOSE += () => Application.Exit();
            Application.AddMessageFilter(closeGrab);
            Component = component;
        }

        protected abstract void close();
    }

    public interface IKeepFormOpen : IDisposable {
        void Close();
        void CloseForReal();
    }

    public class CloseHandler : IMessageFilter {
        public event Action WM_CLOSE;
        public bool PreFilterMessage(ref Message m) {
            if (m.Msg == 0x10) { /*WM_CLOSE*/
                WM_CLOSE?.Invoke();
                return true;
            }
            return false;
        }
    }
}
