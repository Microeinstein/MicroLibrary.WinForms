using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Micro.WinForms {
    public class TextBoxWatermark : TextBox {
        public new event EventHandler TextChanged;

        public string Watermark { get; set; }
        public override string Text {
            get => _text;
            set {
                _text = value;
                base.Text = value; //TextChanged
            }
        }
        public override Color ForeColor {
            get => _forecolor;
            set {
                _forecolor = value;
                if (!watermarked)
                    base.ForeColor = value;
            }
        }
        bool watermarked = true;
        string _text = "";
        Color _forecolor = Color.Black; 

        public TextBoxWatermark() {
            base.TextChanged += textChanged;
            toWatermark();
            GotFocus += (a, b) => toNormal();
            LostFocus += (a, b) => toWatermark();
        }

        void textChanged(object sender, EventArgs e) {
            if (!toNormal())
                toWatermark();
            if (!watermarked)
                TextChanged?.Invoke(sender, e);
        }
        bool toNormal() {
            if (_text != "" || Focused) {
                if (watermarked) {
                    base.Text = "";
                    base.ForeColor = ForeColor;
                    base.Font = new Font(base.Font, FontStyle.Regular);
                    watermarked = false;
                }
                return true;
            }
            return false;
        }
        bool toWatermark() {
            if (_text == "" && !Focused) {
                if (!watermarked) {
                    watermarked = true;
                    base.Text = Watermark;
                    base.ForeColor = Color.Gray;
                    base.Font = new Font(base.Font, FontStyle.Italic);
                }
                return true;
            }
            return false;
        }
    }
}
