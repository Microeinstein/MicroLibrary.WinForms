using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Micro.WinForms.Core;

namespace Micro.WinForms {
    /// <summary>
    /// ListEditor supporting various common editing controls, plus some hotkeys.
    /// </summary>
    public partial class ListEditor : UserControl {
        public event Action updateGUIEx, listChanged;
        public Action addClick;
        public Func<bool> removeClick, editClick;
        
        public bool EnableSettings {
            get => es;
            set => es = value;
        }
        public bool EnableLoad {
            get => el;
            set => el = value;
        }
        public bool EnableBackup {
            get => eb;
            set => eb = value;
        }
        public bool EnableEdit {
            get => ee;
            set => ee = value;
        }
        public bool EnableMove {
            get => em;
            set => em = value;
        }
        public bool VisibleSettings {
            get => vs;
            set => vs = value;
        }
        public bool VisibleLoad {
            get => vl;
            set => vl = value;
        }
        public bool VisibleBackup {
            get => vb;
            set => vb = value;
        }
        public bool VisibleEdit {
            get => ve;
            set => ve = value;
        }
        public bool VisibleMove {
            get => vm;
            set => vm = value;
        }
        public bool AllowDuplicate {
            get => ad;
            set => ad = value;
        }
        public bool EditAfterDuplicate {
            get => ead;
            set => ead = value;
        }
        protected bool UpdateAutomatically = false,
                       ChangingList = false;
        bool es, el, eb, ee, em, ead,
             vs, vl, vb, ve, vm, ad;
        bool[] vis = new bool[5],
               sep = new bool[4];

        ListViewItem selected;

        public ListEditor() {
            InitializeComponent();
            InitializeMyComponent();
            EnableMove = true;
        }
        void InitializeMyComponent() {
            this.SuspendLayout();

            //
            // list
            // 
            list.SubItemClicked += list_SubItemClicked;
            list.SubItemEditChange += list_SubItemEditMove;

            this.ResumeLayout(false);
            this.PerformLayout();
        }
        void list_KeyDown(object sender, KeyEventArgs e) {
            if (!(ee && ve))
                return;
            var k = e.KeyCode;
            bool success = false;

            void hotkeyCond(bool condition, Func<bool> lambda) {
                if (success)
                    return;
                if (condition)
                    success = lambda();
            }
            void hotkey(bool condition, Action lambda)
                => hotkeyCond(condition, () => { lambda(); return true; });

            hotkeyCond(e.Control && k == Keys.D,
                () => duplicateSelected() && (editClick?.Invoke() ?? editLast()));
            hotkey(e.Shift && k == Keys.Enter, () => {
                if (addClick != null)
                    addClick.Invoke();
                else
                    insertItem();
            });
            hotkey(e.Alt && k == Keys.Up,
                () => moveSelected(false, e.Control));
            hotkey(e.Alt && k == Keys.Down,
                () => moveSelected(true, e.Control));
            hotkeyCond(k == Keys.F2 || k == Keys.Enter,
                () => editClick?.Invoke() ?? editLast());
            hotkeyCond(k == Keys.Delete,
                () => removeClick?.Invoke() ?? removeSelected());
            e.Handled = e.SuppressKeyPress = success;
        }
        void list_SubItemClicked(object sender, SubItemEA e) {
            if (!(ee && ve))
                return;
            if (editClick != null)
                editClick.Invoke();
            else {
                selected = e.Item;
                selected.EnsureVisible();
                list.StartEditing(selected, e.SubItem);
            }
        }
        void list_SubItemEditMove(object sender, SubItemEA e) {
            if (!(ee && ve))
                return;
            if (editClick == null) {
                selected = e.Item;
                selected.EnsureVisible();
                list.StartEditing(selected, e.SubItem);
            }
        }
        void btnAdd_Click(object sender, EventArgs e) {
            list.EndEditing(true);
            if (addClick == null)
                insertItem();
            else
                addClick.Invoke();
        }
        void btnDuplicate_Click(object sender, EventArgs e) {
            list.EndEditing(true);
            duplicateSelected();
            if (ead) {
                if (editClick == null)
                    editLast();
                else
                    editClick.Invoke();
            }
            list.Focus();
        }
        void btnRemove_Click(object sender, EventArgs e) {
            list.EndEditing(true);
            if (removeClick == null)
                removeSelected();
            else
                removeClick.Invoke();
            list.Focus();
        }
        void btnEdit_Click(object sender, EventArgs e) {
            list.EndEditing(true);
            if (editClick == null)
                editLast();
            else
                editClick.Invoke();
            list.Focus();
        }
        void btnMoveUp_Click(object sender, EventArgs e) {
            list.EndEditing(true);
            moveSelected(false, false);
        }
        void btnMoveDown_Click(object sender, EventArgs e) {
            list.EndEditing(true);
            moveSelected(true, false);
        }
        void btnMoveTop_Click(object sender, EventArgs e) {
            list.EndEditing(true);
            moveSelected(false, true);
        }
        void btnMoveBottom_Click(object sender, EventArgs e) {
            list.EndEditing(true);
            moveSelected(true, true);
        }
        /*void editBox_VisibleChanged(object sender, EventArgs e) {
            if (editBox.Font != list.Font)
                editBox.Font = list.Font;
        }
        void editBox_TextChanged(object sender, EventArgs e) {
            if (editBox.Visible) {
                Size s = TextRenderer.MeasureText(editBox.Text, editBox.Font, _prefSize, TextFormatFlags.TextBoxControl);
                s.Width += 8;
                editBox.Size = s;
            }
        }
        void editBox_LostFocus(object sender, EventArgs e)
            => saveLast();
        void editBox_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter)
                e.Handled = saveLast();
        }*/

        public ColumnHeader AddColumn(int index, string text, int initW, HorizontalAlignment align = HorizontalAlignment.Left) {
            var c = new ColumnHeader() {
                Text = text,
                TextAlign = align,
                Width = initW
            };
            list.Columns.Insert(index, c);
            return c;
        }
        public void ApplyIcons(ListEditorIcons icons) {
            btnSettings.Image   = icons.Settings;
            btnLoad.Image       = icons.Load;
            btnSave.Image       = icons.Save;
            btnExport.Image     = icons.Export;
            btnImport.Image     = icons.Import;
            btnAdd.Image        = icons.Add;
            btnDuplicate.Image  = icons.Duplicate;
            btnRemove.Image     = icons.Remove;
            btnEdit.Image       = icons.Edit;
            btnMoveUp.Image     = icons.MoveUp;
            btnMoveDown.Image   = icons.MoveDown;
            btnMoveTop.Image    = icons.MoveTop;
            btnMoveBottom.Image = icons.MoveBottom;
        }
        public void UpdateGUI() {
            btnSettings.Visible = vs;
            btnLoad.Visible =
            btnSave.Visible = vl;
            btnExport.Visible =
            btnImport.Visible = vb;
            btnAdd.Visible =
            btnRemove.Visible =
            btnEdit.Visible = ve;
            btnMoveUp.Visible =
            btnMoveDown.Visible =
            btnMoveTop.Visible =
            btnMoveBottom.Visible = vm;

            btnSettings.Enabled = es;
            btnLoad.Enabled =
            btnSave.Enabled = el;
            btnExport.Enabled =
            btnImport.Enabled = eb;
            btnAdd.Enabled =
            btnRemove.Enabled =
            btnEdit.Enabled = ee;
            btnMoveUp.Enabled =
            btnMoveDown.Enabled =
            btnMoveTop.Enabled =
            btnMoveBottom.Enabled = em;

            btnDuplicate.Visible = ve && ad;
            btnDuplicate.Enabled = ee;

            CopyTo(vis, vs, vl, vb, ve, vm);
            CopyTo(sep, false, false, false, false);
            if (vis.Length > 2) {
                bool atLeast1 = false;
                int s = 0;
                for (int v = 0; v < vis.Length; v++) {
                    bool on1 = vis[v];
                    if (on1 && atLeast1)
                        sep[s++] = true;
                    else
                        s = v;
                    atLeast1 = atLeast1 || on1;
                }
            }
            CopyFrom(sep,
                x => sep1.Visible = x,
                x => sep2.Visible = x,
                x => sep3.Visible = x,
                x => sep4.Visible = x
            );

            updateGUIEx?.Invoke();
        }
        protected virtual IExposable newItemBondedValue(int index, IExposable copyFrom = null) => null;
        void insertItem() {
            var si = list.SelectedItems;
            var it = list.Items;
            ListViewItem n;
            int ii;
            if (si.Count != 0)
                ii = si.ToArray<ListViewItem>().Last().Index + 1;
            else
                ii = it.Count;
            var exp = newItemBondedValue(ii);
            n = it.Insert(ii, exp == null ? new ListViewItem() : new LVI_Ex(exp));
            for (int i = n.SubItems.Count; i < list.Columns.Count; i++)
                n.SubItems.Add("");
            //n.BeginEdit();
            list.StartEditing(n, 0);
        }
        bool duplicateSelected() {
            var si = list.SelectedItems;
            var it = list.Items;
            if (si.Count == 0)
                return false;
            ChangingList = true;
            list.BeginUpdate();
            var cpy = si.ToArray<ListViewItem>();
            si.Clear();
            foreach (var item in cpy) {
                var ii = item.Index + 1;
                var exp = newItemBondedValue(ii, item is LVI_Ex x ? x.Value : null);
                var n = it.Insert(ii, exp == null ? new ListViewItem() : new LVI_Ex(exp));
                for (int i = n.SubItems.Count; i < list.Columns.Count; i++)
                    n.SubItems.Add("");
                n.Selected = true;
            }
            list.EndUpdate();
            ChangingList = false;
            listChanged?.Invoke();
            return true;
        }
        bool removeSelected() {
            var si = list.SelectedItems;
            var it = list.Items;
            if (si.Count == 0)
                return false;
            ChangingList = true;
            //bool notOne = si.Count > 1;
            //if (notOne)
            list.BeginUpdate();
            var cpy = si.ToArray<ListViewItem>();
            var fi = cpy[0].Index;
            int len = it.Count;
            int nsi = len == 1 ? -1 : (fi == len - 1 ? -2 : fi);
            si.Clear();
            foreach (var item in cpy)
                it.Remove(item);
            if (nsi != -1) {
                int li = it.Count - 1;
                if (li >= 0)
                    list.SelectedIndices.Add(nsi == -2 ? li : Math.Min(nsi, li));
            }
            list.EndUpdate();
            ChangingList = false;
            listChanged?.Invoke();
            return true;
        }
        bool editLast() {
            var si = list.SelectedItems;
            if (si.Count == 0)
                return false;
            //this.SuspendDrawing();
            selected = si[si.Count - 1];
            selected.EnsureVisible();
            //selected.BeginEdit();
            list.StartEditing(selected, 0);
            return true;
            /*Point p1 = this.PointToClient(table.PointToScreen(list.Location));
            Point p2 = selected.Position;
            editBox.Location = new Point(p1.X + p2.X, p1.Y + p2.Y);
            editBox.Text = "";
            editBox.Visible = true;
            editBox.Text = selected.Text;
            editBox.SelectionLength = 0;
            editBox.SelectionStart = selected.Text.Length - 1;
            selected.Text = "";
            editBox.Focus();
            this.ResumeDrawing();
            return true;*/
        }
        void moveSelected(bool down, bool bound)
            => list.Items.MoveItems(list.SelectedItems, down, bound);
        /*bool saveLast() {
            if (selected == null)
            return false;
            this.SuspendDrawing();
            selected.Text = editBox.Text;
            editBox.Visible = false;
            list.Focus();
            this.ResumeDrawing();
            return true;
        }*/

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            Keys key2 = keyData;
            if (key2.HasFlag(Keys.Shift))
                key2 &= ~Keys.Shift;
            if (key2.HasFlag(Keys.ShiftKey))
                key2 &= ~Keys.ShiftKey;
            if (key2 == Keys.Tab)
                return true;
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }

    public class ListEditorIcons {
        public Image
            Settings,
            Save, Load,
            Export, Import,
            Add, Duplicate, Remove, Edit,
            MoveUp, MoveDown, MoveTop, MoveBottom;
    }
}
