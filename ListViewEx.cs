#pragma warning disable CS0649
#define NO_MIDDLE_SCROLL
#undef NO_MIDDLE_SCROLL

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static System.Windows.Forms.ListView;
using static System.Windows.Forms.ListViewItem;

namespace Micro.WinForms {
    /// <summary>
    /// ListView supporting editing sub-items of any type and middle-click view dragging.
    /// </summary>
    public class ListViewEx : ListView {
        public const string
            YES = "Yes", NO = "No",
            NA = "N/A", PRESENT = "Present";
        static readonly Regex
            validUnsigned = new Regex(@"[0-9]"),
            validSigned = new Regex(@"[0-9\-]"),
            validFloating = new Regex(@"[0-9\-\+,.eE]");
        
        public delegate void SubItemEventHandler(object sender, SubItemEA e);
        public delegate void SubItemEditingHandler(object sender, SubItemEditingEA e);
        public delegate void SubItemEndEditingEventHandler(object sender, SubItemEndEditingEA e);
        public event SubItemEventHandler SubItemClicked, SubItemEditChange;
        public event SubItemEditingHandler SubItemEditing;
        public event SubItemEndEditingEventHandler SubItemEndEditing;

        #region WinAPI
        const int
            LVM_FIRST               = 0x1000,
            LVM_SCROLL              = LVM_FIRST + 20,
            LVM_GETCOLUMNORDERARRAY = LVM_FIRST + 59,
            WM_HSCROLL              = 0x114,
            WM_VSCROLL              = 0x115,
            WM_SIZE                 = 0x05,
            WM_NOTIFY               = 0x4E,
            HDN_FIRST               = -300,
            HDN_BEGINDRAG           = HDN_FIRST - 10,
            HDN_ITEMCHANGINGA       = HDN_FIRST,
            HDN_ITEMCHANGINGW       = HDN_FIRST - 20;

        struct NMHDR {
            public IntPtr hwndFrom;
            public int idFrom, code;
        }

        [DllImport("user32.dll")]
        static extern int GetScrollPos(IntPtr hWnd, Orientation nBar);
        [DllImport("user32.dll")]
        static extern int SetScrollPos(IntPtr hWnd, Orientation nBar, int nPos, bool bRedraw);
        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int length, ref int[] order);
        [DllImport("user32.dll")]
        static extern bool LockWindowUpdate(IntPtr Handle);

        #endregion

        public new LVIC_Ex Items { get; }
        public bool DoubleClickEdit { get; set; } = true;
        public int IntegralHeight { get; private set; }
        public bool integralScroll = false;
        /*object EditorValue {
            get {
                if (editor is TextBox tb)
                    return tb.Text;
                else if (editor is NumericUpDown nud)
                    return nud.Value;
                else if (editor is ComboBox cb)
                    return cb.SelectedIndex;
                else if (editor is MaskedTextBox mtb)
                    return mtb.Text;
                return editor.Text;
            }
            set {
                if (editor is TextBox tb)
                    tb.Text = value?.ToString() ?? default;
                else if (editor is NumericUpDown nud)
                    nud.Value = value != null ? Convert.ToDecimal(value) : 1;
                else if (editor is ComboBox cb) {
                    if (value is int n)
                        cb.SelectedIndex = n;
                    else
                        cb.SelectedItem = value;
                } else if (editor is MaskedTextBox mtb)
                    mtb.Text = value?.ToString() ?? default;
            }
        }
        string EditorFinalValue {
            get {
                if (editor is ComboBox cb)
                    return cb.Items[(int)EditorValue].ToString();
                return EditorValue.ToString();
            }
        }
        HorizontalAlignment EditorAlign {
            set {
                if (editor is TextBox tb)
                    tb.TextAlign = value;
                else if (editor is NumericUpDown nud)
                    nud.TextAlign = value;
                else if (editor is MaskedTextBox mtb)
                    mtb.TextAlign = value;
            }
        }*/
#if !NO_MIDDLE_SCROLL
        Point? centerFrom, centerFromScreen;
#endif
        public readonly AutoTextBox editor;
        TypeCode objType;
        EventHandler editorLeave;
        PreviewKeyDownEventHandler editorKeyDown;
        AutoTextBox.Transformer editorTransformer;
        ListViewItem editTarget;
        Form oldForm = null;
        bool? editorBool = null,
              keyDel = null;
        bool tabKey = false,
             managedKey = false;
        decimal editorNum = 0;
        decimal numMin, numMax;
        int editSubIndex;

        public ListViewEx() {
            Items = new LVIC_Ex(this);
            FullRowSelect = true;

            //Activate double buffering
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            
            //Enable the OnNotifyMessage event so we get a chance to filter out 
            // Windows messages before they get to the form's WndProc
            SetStyle(ControlStyles.EnableNotifyMessage, true);

            //fillLast();
            //SizeChanged += (a, b) => fillLast();
            TabStop = false;
            FontChanged += ListViewEx_FontChanged;

            //
            // editor
            // 
            editor = new AutoTextBox();
            editor.Visible = false;
            editor.BorderStyle = BorderStyle.None;
            editor.Margin = new Padding(0);
            editor.Multiline = true;
            editor.AcceptsReturn = false;
            editor.TabStop = false;
            editor.Leave += editor_Leave;
            editor.PreviewKeyDown += editor_PreviewKeyDown;
            editor.KeyDown += editor_KeyDown;
            editor.TextTransformer += editor_TextTransformer;
        }

        void ListViewEx_FontChanged(object sender, EventArgs e)
            => IntegralHeight = "I".GetSize(Font).Height;
        void editor_TextTransformer(AutoTextBox from, StringBuilder text, ref int caret)
            => editorTransformer?.Invoke(from, text, ref caret);
        void editor_Leave(object sender, EventArgs e) {
            //Debug.WriteLine($@"editor leave ({!tabKey});");
            if (tabKey) {
                tabKey = false;
                editor.Focus();
                return;
            }
            editorLeave?.Invoke(sender, e);
            EndEditing(true);
        }
        void editor_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e) {
            bool enter = e.KeyCode == Keys.Enter;
            if (enter || e.KeyCode == Keys.Escape) {
                EndEditing(enter);
                return;
            }
            editorKeyDown?.Invoke(sender, e);
            if (!editor.AcceptsTab && e.KeyCode == Keys.Tab) {
                bool back = e.Shift;
                var it = editTarget;
                var ii = editSubIndex + (back ? -1 : 1);
                if (ii < 0) {
                    if (it.Index == 0)
                        it = Items[Items.Count - 1];
                    else
                        it = Items[it.Index - 1];
                    ii = it.SubItems.Count - 1;
                } else if (ii >= it.SubItems.Count) {
                    if (it.Index == Items.Count - 1)
                        it = Items[0];
                    else
                        it = Items[it.Index + 1];
                    ii = 0;
                }
                //Debug.WriteLine($@"editor tab;");
                oldForm.ActiveControl = null;
                SubItemEditChange?.Invoke(this, new SubItemEA(it, ii));
                tabKey = true;
            }
        }
        void editor_KeyDown(object sender, KeyEventArgs e) {
            e.Handled = e.SuppressKeyPress = managedKey;
            managedKey = false;
        }
        protected override void OnMouseDown(MouseEventArgs e) {
#if !NO_MIDDLE_SCROLL
            if (e.Button == MouseButtons.Middle) {
                centerFrom = e.Location;
                centerFromScreen = PointToScreen(e.Location);
                Cursor = Cursors.NoMove2D;
                return;
            }
#endif
            base.OnMouseDown(e);
        }
        protected override void OnMouseMove(MouseEventArgs e) {
#if !NO_MIDDLE_SCROLL
            if (centerFrom != null) {
                Point p2 = e.Location;
                Point p1 = centerFrom.Value;
                if (p2 == p1)
                    goto skip;
                int dx = p2.X - p1.X;
                int dy = p2.Y - p1.Y;
                Cursor.Position = centerFromScreen.Value;
                //int i = list.findNearest(19, dy > 0) + ddy;
                //i = Math.Min(Math.Max(0, i), list.Items.Count - 1);
                //list.EnsureVisible(i);
                if (integralScroll)
                    dy = (int)Math.Round(dy * IntegralHeight / 5f * 4);
                else
                    dy *= 5;
                MoveView(dx * 5, dy);
                return;
            }
            skip:
#endif
            base.OnMouseMove(e);
        }
        protected override void OnMouseUp(MouseEventArgs e) {
#if !NO_MIDDLE_SCROLL
            if (e.Button == MouseButtons.Middle) {
                centerFrom = null;
                Cursor = Cursors.Default;
                return;
            }
#endif
            if (!DoubleClickEdit && triggerEventSubitemAt(new Point(e.X, e.Y)))
                return;

            base.OnMouseUp(e);
        }
        protected override void OnDoubleClick(EventArgs e) {
            if (DoubleClickEdit && triggerEventSubitemAt(PointToClient(Cursor.Position)))
                return;
            base.OnDoubleClick(e);
        }
        protected override void OnNotifyMessage(Message m) {
            //Filter out the WM_ERASEBKGND message
            if (m.Msg != 0x14)
                base.OnNotifyMessage(m);
        }
        protected override void WndProc(ref Message msg) {
            switch (msg.Msg) {
                case WM_VSCROLL:
                case WM_HSCROLL:
                case WM_SIZE:
                    EndEditing(false);
                    break;
                case WM_NOTIFY:
                    NMHDR h = (NMHDR)Marshal.PtrToStructure(msg.LParam, typeof(NMHDR));
                    if (h.code == HDN_BEGINDRAG
                     || h.code == HDN_ITEMCHANGINGA
                     || h.code == HDN_ITEMCHANGINGW)
                        EndEditing(false);
                    break;
            }

            base.WndProc(ref msg);
        }

        public void MoveView(int dx, int dy) {
            LockWindowUpdate(Handle);
            
            if (dx != 0) {
                int h = GetScrollPos(Handle, Orientation.Horizontal);
                if (h + dx < 0)
                    dx = -h;
                //SetScrollPos(Handle, Orientation.Horizontal, dx, true);
            }
            if (dy != 0) {
                int v = GetScrollPos(Handle, Orientation.Vertical);
                if (integralScroll)
                    v *= IntegralHeight;
                if (v + dy < 0)
                    dy = -v;
                //SetScrollPos(Handle, Orientation.Vertical, dy, true);
            }
            SendMessage(Handle, LVM_SCROLL, (IntPtr)dx, (IntPtr)dy);

            LockWindowUpdate(IntPtr.Zero);
        }
        public int[] GetColumnOrder() {
            int cc = Columns.Count;
            IntPtr l = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)) * cc);

            IntPtr res = SendMessage(Handle, LVM_GETCOLUMNORDERARRAY, new IntPtr(cc), l);
            if (res == IntPtr.Zero) {
                Marshal.FreeHGlobal(l);
                return null;
            }

            int[] order = new int[cc];
            Marshal.Copy(l, order, 0, cc);
            Marshal.FreeHGlobal(l);

            return order;
        }
        public int GetSubItemAt(int x, int y, out ListViewItem item) {
            item = GetItemAt(x, y);
            if (item == null)
                goto err;

            int[] order = GetColumnOrder();
            Rectangle lviBounds = item.GetBounds(ItemBoundsPortion.Entire);
            int startX = lviBounds.Left;

            for (int i = 0; i < order.Length; i++) {
                ColumnHeader h = Columns[order[i]];
                if (x < startX + h.Width)
                    return h.Index;
                startX += h.Width;
            }

            err:
            return -1;
        }
        public Rectangle GetSubItemBounds(ListViewItem item, int subIndex) {
            int[] order = GetColumnOrder();
            
            if (subIndex >= order.Length)
                throw new ArgumentOutOfRangeException(nameof(subIndex));
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            Rectangle lviBounds = item.GetBounds(ItemBoundsPortion.Entire);
            int startX = lviBounds.Left;

            int i = 0;
            ColumnHeader col = Columns[order[i]];
            for (; i < order.Length && col.Index != subIndex; col = Columns[order[++i]])
                startX += col.Width;

            Rectangle rect = new Rectangle(startX, lviBounds.Top, Columns[order[i]].Width, lviBounds.Height);
            return rect;
        }
        public void StartEditing(ListViewItem item, int subIndex) {
            //Debug.WriteLine($@"start editing;");
            var e = new SubItemEditingEA(item, subIndex);
            SubItemEditing?.Invoke(this, e);
            var sub = item.SubItems[subIndex];
            string value;
            bool hide = false;
            if (sub is LVSI_Ex x) {
                var o = x.Value;
                objType = o?.GetType().GetInterfaces()
                    .Where(i => i.Name == "IConvertible").Count() >= 1
                        ? Convert.GetTypeCode(o)
                        : TypeCode.Object;
                if (o is bool b)
                    value = b ? YES : NO;
                else
                    value = o?.ToString() ?? "";
                hide = x.ShouldBeHidden;
            } else {
                value = sub.Text;
                objType = TypeCode.String;
            }
            item.EnsureVisible();

            Rectangle subRect = GetSubItemBounds(item, subIndex);

            if (subRect.X < 0) {
                subRect.Width += subRect.X;
                subRect.X = 0;
            }
            if (subRect.X + subRect.Width > Width)
                subRect.Width = Width - subRect.Left;
            subRect.Offset(Left, Top);
            
            oldForm = FindForm();
            oldForm?.Controls.Add(editor);
            Point origin = Point.Empty;
            Point pPos = Parent.PointToScreen(origin);
            Point ePPos = oldForm.PointToScreen(origin);
            var align = Columns[subIndex].TextAlign;
            if (align == HorizontalAlignment.Left) {
                subRect.X += 3;
                subRect.Width -= 3;
            } else if (align == HorizontalAlignment.Right) {
                subRect.X += 1;
                subRect.Width -= 4;
            }
            subRect.Y += 2;
            subRect.Height -= 3;
            subRect.Offset(pPos.X - ePPos.X, pPos.Y - ePPos.Y);
            if (subIndex == 0 && item.ImageList != null) {
                int iconRes = 20;
                subRect.X += iconRes - 1;
                subRect.Width -= iconRes;
            }
            SelectedItems.Clear();
            item.Selected = true;
            editTarget = item;
            editSubIndex = subIndex;

            editor.TextAlign = align;
            editor.PasswordChar = hide ? '\u25CF' : '\0';
            editor.Bounds = subRect;
            editor.Font = item.Font;
            editorLeave = null;
            editorKeyDown = null;
            editorTransformer = null;
            editor.Text = value;
            prepareEditor();
            editor.Visible = true;
            editor.BringToFront();
            editor.Focus();
        }
        public void EndEditing(bool SaveChanges) {
            //Debug.WriteLine($@"end editing ({editTarget != null});");
            if (editTarget == null)
                return;

            var sub = editTarget.SubItems[editSubIndex];
            var t = editor.Text;
            var e = new SubItemEndEditingEA(editTarget, editSubIndex, t, SaveChanges);
            if (SaveChanges) {
                if (sub is LVSI_Ex x)
                    x.Text = t;
                else
                    sub.Text = t;
            }
            SubItemEndEditing?.Invoke(this, e);

            oldForm?.Controls.Remove(editor);
            editor.Visible = false;
            editor.Text = "";
            editTarget = null;
            editSubIndex = -1;
            this.Focus();
        }
        /*public int findNearest(int itemHeight, bool fromDown) {
            int h = Height;
            int y = fromDown ? (h / itemHeight * itemHeight - itemHeight / 2) : (View == View.Details ? 20 : 0);
            ListViewItem it;
            bool none;
            var sorted = Items.GetOrderedByPosition();
            if (fromDown)
                sorted = sorted.Reverse();
            do {
                it = GetItemAt(10, y) ?? GetItemAt(23, y);
                none = it == null;
                if (none)
                    y += fromDown ? -1 : 1;
            } while (none && (fromDown ? y >= 0 : y < h));

            if (none)
                return -1;
            return it.Index;
        }*/
        
        void fillLast() {
            int len = Columns.Count;
            if (len > 0) {
                var cols = Enumerable.Range(0, len).Select(i => Columns[i]).ToArray();
                cols[len - 1].Width = Width - cols.Take(len - 1).Sum(c => c.Width) - 30;
            }
        }
        bool triggerEventSubitemAt(Point p) {
            int idx = GetSubItemAt(p.X, p.Y, out ListViewItem item);
            var ok = idx >= 0;
            if (ok)
                SubItemClicked?.Invoke(this, new SubItemEA(item, idx));
            return ok;
        }
        void prepareEditor() {
            switch (objType) {
                case TypeCode.Boolean:
                    editorKeyDown = (s, e) => {
                        if (e.KeyCode == Keys.Y)
                            editorBool = true;
                        else if (e.KeyCode == Keys.N)
                            editorBool = false;
                    };
                    break;
                case TypeCode.Char:
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    objType.GetLimits(out var mi, out var ma);
                    numMin = Convert.ToDecimal(mi);
                    numMax = Convert.ToDecimal(ma);
                    decimal num() {
                        decimal n = 0;
                        string t = editor.Text;
                        if (!string.IsNullOrWhiteSpace(t))
                            n = (decimal)Convert.ChangeType(t, TypeCode.Decimal);
                        return n;
                    }
                    editorNum = num();
                    editorKeyDown = (s, e) => {
                        decimal n2, n = 0;
                        string result = null;
                        try {
                            n2 = n = num();
                            editorNum = n;
                            Keys k = e.KeyCode;
                            if (k == Keys.Up || k == Keys.Oemplus)
                                n += 1 * (e.Control ? 5 : 1);
                            else if (k == Keys.Down || k == Keys.OemMinus)
                                n -= 1 * (e.Control ? 5 : 1);
                            else if (k == Keys.PageUp)
                                n += 4 * (e.Control ? 5 : 1);
                            else if (k == Keys.PageDown)
                                n -= 4 * (e.Control ? 5 : 1);
                            else if (e.Control && k == Keys.Back)
                                keyDel = false;
                            else if (e.Control && k == Keys.Delete)
                                keyDel = true;
                            if (n < numMin)
                                n = numMin;
                            else if (n > numMax)
                                n = numMax;
                            if (e.IsInputKey = (n != n2))
                                result = n.ToString();//Convert.ChangeType(n, objType).ToString();
                        } catch (OverflowException oe) {
                            var type = (Type)oe.TargetSite.GetType().GetProperty("ReturnType")
                            .GetMethod.Invoke(oe.TargetSite, new object[0]);
                            var limit = type.GetField(n >= 0 ? "MaxValue" : "MinValue").GetValue(null);
                            result = limit.ToString();
                        } catch (Exception) {
                            result = Convert.ChangeType(editorNum, objType).ToString();
                        }
                        if (result != null) {
                            editor.SuspendDrawing();
                            managedKey = true;
                            editor.Text = result;
                            editor.ResumeDrawing();
                        }
                    };
                    editorLeave = (s, e) => {
                        object r;
                        bool ok = Core.TryChangeType(editor.Text, objType, out r)
                               || Core.TryChangeType(editorNum, objType, out r);
                        var si = editTarget.SubItems[editSubIndex];
                        editor.Text = (ok ? r : (si is LVSI_Ex x ? x.Value : si.Text)).ToString();
                    };
                    break;
            }

            switch (objType) {
                case TypeCode.Boolean:
                    editorTransformer = (AutoTextBox from, StringBuilder text, ref int caret) => {
                        if (text.Length < 1)
                            return;
                        if (editorBool == true) {
                            text.Clear();
                            text.Append(YES);
                        } else if (editorBool == false) {
                            text.Clear();
                            text.Append(NO);
                        }
                        caret = text.Length;
                    };
                    break;
                case TypeCode.Char:
                    editorTransformer = (AutoTextBox from, StringBuilder text, ref int caret) => {
                        if (text.Length > 1)
                            text.Remove(0, text.Length - 1);
                        else
                            text.Append('\0');
                        caret = text.Length;
                    };
                    break;
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    editorTransformer = (AutoTextBox from, StringBuilder text, ref int caret)
                        => selectiveNumber(validUnsigned, from, text, ref caret);
                    break;
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    editorTransformer = (AutoTextBox from, StringBuilder text, ref int caret)
                        => selectiveNumber(validSigned, from, text, ref caret);
                    break;
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    editorTransformer = (AutoTextBox from, StringBuilder text, ref int caret)
                        => selectiveNumber(validFloating, from, text, ref caret);
                    break;
            }
        }
        void selectiveNumber(Regex allowed, AutoTextBox from, StringBuilder text, ref int caret) {
            bool one = false;
            char c;
            int i;
            if (keyDel == false) {
                text.Remove(0, caret);
                caret = 0;
            } else if (keyDel == true) {
                text.Remove(caret, text.Length - caret);
                caret = text.Length;
            }
            keyDel = null;
            for (i = 0; i < text.Length; i++) {
                one = true;
                c = text[i];
                if (!allowed.Match(c + "").Success) {
                    text.Remove(i, 1);
                    i--;
                }
            }
            if (!one) {
                text.Append(0);
                caret = 1;
            }
            bool? moreTrail = null;
            for (i = 0; i < text.Length - 1 && moreTrail != false; i++) {
                c = text[i];
                if ((moreTrail = c == '0').Value) {
                    text.Remove(i, 1);
                    i--;
                }
            }
            if (Core.TryChangeType(text.ToString(), TypeCode.Decimal, out decimal r)) {
                if (r > numMax) {
                    text.Clear();
                    text.Append(numMax);
                } else if (r < numMin) {
                    text.Clear();
                    text.Append(numMin);
                }
            }
            if (managedKey)
                caret = text.Length;
        }
    }

    /// <summary>
    /// Allows object/string binding
    /// </summary>
    public interface IExposable {
        int ValuesCount { get; }
        IEnumerable<object> RealValues { get; }
        IEnumerable<string> TextValues(bool forUI = true);
        void ParseAndSet(string v, int index, bool fromUI = true);
        bool ShouldBeHidden(int index);
    }
    /// <summary>
    /// Boundable object single sub-item
    /// </summary>
    public class LVSI_Ex : ListViewSubItem {
        public IExposable Bound { get; protected set; }
        public new string Text {
            get {
                string v = Bound.TextValues().ElementAt(pos);
                if (Bound.ShouldBeHidden(pos))
                    return string.IsNullOrEmpty(v) ? ListViewEx.NA : ListViewEx.PRESENT;
                else
                    return v;
            }
            set {
                Bound.ParseAndSet(value, pos);
                base.Text = Text;
            }
        }
        public bool ShouldBeHidden => Bound.ShouldBeHidden(pos);
        public object Value => Bound.RealValues.ElementAt(pos);
        int pos;

        internal protected LVSI_Ex(ListViewItem owner, IExposable value, int index) : base(owner, "") {
            Bound = value;
            pos = index;
            base.Text = Text;
        }
    }
    /// <summary>
    /// Allows using boundable object as a ListViewItem
    /// </summary>
    public class LVI_Ex : ListViewItem {
        public IExposable Value { get; protected set; }

        public LVI_Ex(IExposable value) : base() {
            Value = value;
            //SubItems are always at least 1
            SubItems.AddRange(value.TextValues().Select((t, i) => new LVSI_Ex(this, Value, i)).ToArray());
            SubItems.RemoveAt(0);
        }
    }
    /// <summary>
    /// Allows to move easily <see cref="ListViewItem"/>s and to be notified about items addition/removal.
    /// </summary>
    public class LVIC_Ex : ListViewItemCollection {
        static readonly YComparer yComp = new YComparer();
        public delegate void ItemEventHandler(object sender, ListViewItem item);
        public delegate void ItemsEventHandler(object sender, IList items);
        public event ItemEventHandler AddingItem, AddedItem, RemovingItem, RemovedItem;
        public event ItemsEventHandler MovingItems, MovedItems;
        bool noEvents = false;
        ListView owner;

        public LVIC_Ex(ListView owner) : base(owner) {
            this.owner = owner;
        }

        public override ListViewItem Add(ListViewItem value) {
            if (!noEvents)
                AddingItem?.Invoke(this, value);
            var o = base.Add(value);
            if (!noEvents)
                AddedItem?.Invoke(this, value);
            return o;
        }
        public override void Remove(ListViewItem item) {
            if (!noEvents)
                RemovingItem?.Invoke(this, item);
            base.Remove(item);
            if (!noEvents)
                RemovedItem?.Invoke(this, item);
        }
        public void MoveItems(IList items, bool down, bool bound) {
            MovingItems?.Invoke(this, items);
            int itemsCount = items.Count;
            if (itemsCount <= 0)
                return;

            noEvents = true;
            var itemsCopy = items.ToArray<ListViewItem>();
            var first = itemsCopy[0];
            int firstIndex = first.Index;
            var last = first;
            int? newIndexFirst = null;
            int newIndex = 0;
            int fullCount = Count; //complete list;
            int afterCount; //list without selected
            int listBottom = fullCount - 1;

            void pause() {
                owner.BeginUpdate();
            }
            void remove(params ListViewItem[] a) {
                pause();
                foreach (var item in a)
                    Remove(item);
                afterCount = Count; //list without selected
            }

            if (itemsCount == 1) {
                if (bound) {
                    if (down) {
                        if (firstIndex < listBottom)
                            newIndexFirst = listBottom;
                    } else {
                        if (firstIndex > 0)
                            newIndexFirst = 0;
                    }
                } else if (down) {
                    if (firstIndex < listBottom)
                        newIndexFirst = firstIndex + 1;
                } else if (firstIndex > 0)
                    newIndexFirst = firstIndex - 1;
                if (newIndexFirst != null) {
                    remove(first);
                    Insert(newIndexFirst.Value, first);
                }
            } else { //Multiple selected items
                last = itemsCopy[itemsCount - 1];
                int lastIndex = last.Index;
                int itemsDistance = lastIndex - firstIndex + 1;
                bool touchingBound = !((down && lastIndex < listBottom) || (!down && firstIndex > 0));
                bool sameSize = itemsDistance == itemsCount;
                if (sameSize && touchingBound)
                    goto end;
                else if (sameSize || (touchingBound && bound)) {
                    remove(itemsCopy);
                    if (bound) {
                        if (down) {
                            foreach (var item in itemsCopy)
                                Add(item);
                            newIndexFirst = firstIndex;
                            goto end;
                        } //if not down newIndexFirst already at 0
                    } else if (down)
                        newIndex = firstIndex + 1;
                    else
                        newIndex = firstIndex - 1;
                    newIndexFirst = firstIndex;
                    foreach (var item in itemsCopy)
                        Insert(newIndex++, item);
                } else {
                    int[] relative = (from it in itemsCopy
                                        select it.Index - firstIndex).ToArray();
                    if (down) {
                        if (lastIndex == listBottom) {
                            bool nowShift = false;
                            int li = itemsCount - 1;
                            int offset = relative[li];
                            for (int i = 0, j = li; i < itemsCount; j = li - ++i) {
                                if (nowShift || (nowShift = offset - relative[j] - i > 0))
                                    relative[j]++;
                            }
                            firstIndex--;
                        }
                    } else if (firstIndex == 0) {
                        bool nowShift = false;
                        for (int i = 0; i < itemsCount; i++) {
                            if (nowShift || (nowShift = relative[i] > i))
                                relative[i]--;
                        }
                        firstIndex++;
                    }
                    remove(itemsCopy);
                    int anchor = bound
                        ? (down ? fullCount - itemsDistance : 0)
                        : firstIndex + (down ? 1 : -1);
                    newIndexFirst = anchor;
                    foreach (var item in itemsCopy)
                        Insert(anchor + relative[newIndex++], item);
                }
            }
            end:
            if (newIndexFirst != null) {
                owner.EndUpdate();
                owner.EnsureVisible(down ? last.Index : first.Index);
            }
            noEvents = false;
            MovedItems?.Invoke(this, items);

        }
        public IEnumerable<ListViewItem> GetOrderedByPosition()
            => ((IEnumerable<ListViewItem>)this).OrderBy(lvi => lvi.Position, yComp);

        class YComparer : IComparer<Point> {
            public int Compare(Point x, Point y)
                => x.Y.CompareTo(y.Y);
        }
    }

    //EventArgs
    public class SubItemEA : EventArgs {
        public ListViewItem Item { get; }
        public int SubItem { get; } = -1;

        public SubItemEA(ListViewItem item, int subItem) {
            SubItem = subItem;
            Item = item;
        }
    }
    public class SubItemEditingEA : SubItemEA {
        public bool Cancel { get; set; }

        public SubItemEditingEA(ListViewItem item, int subItem) : base(item, subItem) { }
    }
    public class SubItemEndEditingEA : SubItemEA {
        public bool Saved { get; protected set; }
        public object Value { get; protected set; }

        public SubItemEndEditingEA(ListViewItem item, int subItem, object value, bool saved) : base(item, subItem) {
            Saved = saved;
            Value = value;
        }
    }
}
