using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Micro.WinForms {
    /// <summary>
    /// TextBox supporting automatic text transformations on text change w/o flickering.
    /// </summary>
    public partial class AutoTextBox : TextBox {
        public delegate void Transformer(AutoTextBox from, StringBuilder text, ref int caret);
        public event Transformer TextTransformer;
        public new event EventHandler TextChanged;

        bool lockChangeEvent = false;
        StringBuilder txtBuffer = new StringBuilder();

        public AutoTextBox() {
            //Activate double buffering
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

            base.TextChanged += textChanged;
        }
        protected override void OnNotifyMessage(Message m) {
            //Filter out the WM_ERASEBKGND message
            if (m.Msg != 0x14)
                base.OnNotifyMessage(m);
        }
        void textChanged(object sender, EventArgs e) {
            if (lockChangeEvent)
                return;
            lockChangeEvent = true;
            if (TextTransformer != null) {
                txtBuffer.Clear();
                txtBuffer.Append(this.Text);
                int caret = this.SelectionStart;
                TextTransformer?.Invoke(this, txtBuffer, ref caret);
                this.Text = txtBuffer.ToString();
                this.SelectionStart = caret;
            }
            this.TextChanged?.Invoke(sender, e);
            lockChangeEvent = false;
        }
    }
}
