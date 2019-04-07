using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Micro.WinForms {
    public class ComponentContext : ApplicationContext {
        public readonly Component Component;

        public ComponentContext(Component component)
            => Component = component;
    }
}
