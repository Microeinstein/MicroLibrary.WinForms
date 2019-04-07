using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Micro.WinForms {
    public partial class NiceToolStrip : ToolStrip {
        static FixedToolStripRenderer renderer = new FixedToolStripRenderer();

        public NiceToolStrip() {
            setDefaults();
        }
        public NiceToolStrip(params ToolStripItem[] items) : base(items) {
            setDefaults();
        }
        public NiceToolStrip(IContainer container) {
            container.Add(this);
            setDefaults();
        }
        void setDefaults() {
            Renderer = renderer;
            GripStyle = ToolStripGripStyle.Hidden;
            AutoSize = false;
            Height = 26;
        }
        public void fixItemMetrics() {
            Height = 28;
            foreach (ToolStripItem i in Items) {
                i.AutoSize = false;
                if (i.DisplayStyle != ToolStripItemDisplayStyle.ImageAndText) {
                    i.Width++;
                } else {
                    i.Width += 1;
                }
                Padding p;
                p = i.Margin;
                p.Left++;
                p.Top++;
                i.Margin = p;
            }
        }

        public class FixedToolStripRenderer : ToolStripProfessionalRenderer {
            public FixedToolStripRenderer() {
                RoundedEdges = false;
            }
        }
    }
}
