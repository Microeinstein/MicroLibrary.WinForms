namespace Micro.WinForms {
    partial class ListEditor {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.table = new System.Windows.Forms.TableLayoutPanel();
            this.toolstrip = new Micro.WinForms.NiceToolStrip(this.components);
            this.btnSettings = new System.Windows.Forms.ToolStripButton();
            this.sep1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnLoad = new System.Windows.Forms.ToolStripButton();
            this.btnSave = new System.Windows.Forms.ToolStripButton();
            this.sep2 = new System.Windows.Forms.ToolStripSeparator();
            this.btnExport = new System.Windows.Forms.ToolStripButton();
            this.btnImport = new System.Windows.Forms.ToolStripButton();
            this.sep3 = new System.Windows.Forms.ToolStripSeparator();
            this.btnAdd = new System.Windows.Forms.ToolStripButton();
            this.btnDuplicate = new System.Windows.Forms.ToolStripButton();
            this.btnRemove = new System.Windows.Forms.ToolStripButton();
            this.btnEdit = new System.Windows.Forms.ToolStripButton();
            this.sep4 = new System.Windows.Forms.ToolStripSeparator();
            this.btnMoveUp = new System.Windows.Forms.ToolStripButton();
            this.btnMoveDown = new System.Windows.Forms.ToolStripButton();
            this.btnMoveTop = new System.Windows.Forms.ToolStripButton();
            this.btnMoveBottom = new System.Windows.Forms.ToolStripButton();
            this.list = new Micro.WinForms.ListViewEx();
            this.cvalue = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.table.SuspendLayout();
            this.toolstrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // table
            // 
            this.table.AutoSize = true;
            this.table.ColumnCount = 1;
            this.table.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.table.Controls.Add(this.toolstrip, 0, 0);
            this.table.Controls.Add(this.list, 0, 1);
            this.table.Dock = System.Windows.Forms.DockStyle.Fill;
            this.table.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize;
            this.table.Location = new System.Drawing.Point(0, 0);
            this.table.Margin = new System.Windows.Forms.Padding(0);
            this.table.Name = "table";
            this.table.RowCount = 2;
            this.table.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.table.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.table.Size = new System.Drawing.Size(760, 375);
            this.table.TabIndex = 3;
            // 
            // toolstrip
            // 
            this.toolstrip.AutoSize = false;
            this.toolstrip.CanOverflow = false;
            this.toolstrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolstrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnSettings,
            this.sep1,
            this.btnLoad,
            this.btnSave,
            this.sep2,
            this.btnExport,
            this.btnImport,
            this.sep3,
            this.btnAdd,
            this.btnDuplicate,
            this.btnRemove,
            this.btnEdit,
            this.sep4,
            this.btnMoveUp,
            this.btnMoveDown,
            this.btnMoveTop,
            this.btnMoveBottom});
            this.toolstrip.Location = new System.Drawing.Point(0, 0);
            this.toolstrip.Name = "toolstrip";
            this.toolstrip.Size = new System.Drawing.Size(760, 25);
            this.toolstrip.TabIndex = 0;
            this.toolstrip.Text = "fixedToolStrip1";
            // 
            // btnSettings
            // 
            this.btnSettings.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(53, 22);
            this.btnSettings.Text = "Settings";
            // 
            // sep1
            // 
            this.sep1.Name = "sep1";
            this.sep1.Size = new System.Drawing.Size(6, 25);
            // 
            // btnLoad
            // 
            this.btnLoad.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(37, 22);
            this.btnLoad.Text = "Load";
            // 
            // btnSave
            // 
            this.btnSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(35, 22);
            this.btnSave.Text = "Save";
            // 
            // sep2
            // 
            this.sep2.Name = "sep2";
            this.sep2.Size = new System.Drawing.Size(6, 25);
            // 
            // btnExport
            // 
            this.btnExport.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(44, 22);
            this.btnExport.Text = "Export";
            // 
            // btnImport
            // 
            this.btnImport.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(47, 22);
            this.btnImport.Text = "Import";
            // 
            // sep3
            // 
            this.sep3.Name = "sep3";
            this.sep3.Size = new System.Drawing.Size(6, 25);
            // 
            // btnAdd
            // 
            this.btnAdd.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(33, 22);
            this.btnAdd.Text = "Add";
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnDuplicate
            // 
            this.btnDuplicate.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnDuplicate.Name = "btnDuplicate";
            this.btnDuplicate.Size = new System.Drawing.Size(61, 22);
            this.btnDuplicate.Text = "Duplicate";
            this.btnDuplicate.Click += new System.EventHandler(this.btnDuplicate_Click);
            // 
            // btnRemove
            // 
            this.btnRemove.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(54, 22);
            this.btnRemove.Text = "Remove";
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // btnEdit
            // 
            this.btnEdit.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(31, 22);
            this.btnEdit.Text = "Edit";
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
            // 
            // sep4
            // 
            this.sep4.Name = "sep4";
            this.sep4.Size = new System.Drawing.Size(6, 25);
            // 
            // btnMoveUp
            // 
            this.btnMoveUp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnMoveUp.Name = "btnMoveUp";
            this.btnMoveUp.Size = new System.Drawing.Size(26, 22);
            this.btnMoveUp.Text = "Up";
            this.btnMoveUp.Click += new System.EventHandler(this.btnMoveUp_Click);
            // 
            // btnMoveDown
            // 
            this.btnMoveDown.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnMoveDown.Name = "btnMoveDown";
            this.btnMoveDown.Size = new System.Drawing.Size(42, 22);
            this.btnMoveDown.Text = "Down";
            this.btnMoveDown.Click += new System.EventHandler(this.btnMoveDown_Click);
            // 
            // btnMoveTop
            // 
            this.btnMoveTop.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnMoveTop.Name = "btnMoveTop";
            this.btnMoveTop.Size = new System.Drawing.Size(31, 22);
            this.btnMoveTop.Text = "Top";
            this.btnMoveTop.Click += new System.EventHandler(this.btnMoveTop_Click);
            // 
            // btnMoveBottom
            // 
            this.btnMoveBottom.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnMoveBottom.Name = "btnMoveBottom";
            this.btnMoveBottom.Size = new System.Drawing.Size(51, 22);
            this.btnMoveBottom.Text = "Bottom";
            this.btnMoveBottom.Click += new System.EventHandler(this.btnMoveBottom_Click);
            // 
            // list
            // 
            this.list.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.list.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.cvalue});
            this.list.Dock = System.Windows.Forms.DockStyle.Fill;
            this.list.DoubleClickEdit = true;
            this.list.FullRowSelect = true;
            this.list.GridLines = true;
            this.list.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.list.LabelWrap = false;
            this.list.Location = new System.Drawing.Point(0, 25);
            this.list.Margin = new System.Windows.Forms.Padding(0);
            this.list.Name = "list";
            this.list.ShowItemToolTips = true;
            this.list.Size = new System.Drawing.Size(760, 350);
            this.list.TabIndex = 1;
            this.list.TabStop = false;
            this.list.UseCompatibleStateImageBehavior = false;
            this.list.View = System.Windows.Forms.View.Details;
            this.list.KeyDown += new System.Windows.Forms.KeyEventHandler(this.list_KeyDown);
            // 
            // cvalue
            // 
            this.cvalue.Text = "Values";
            this.cvalue.Width = 80;
            // 
            // ListEditor
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.Controls.Add(this.table);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "ListEditor";
            this.Size = new System.Drawing.Size(760, 375);
            this.table.ResumeLayout(false);
            this.toolstrip.ResumeLayout(false);
            this.toolstrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        protected System.Windows.Forms.TableLayoutPanel table;
        protected System.Windows.Forms.ToolStripSeparator sep4;
        protected System.Windows.Forms.ToolStripButton btnMoveUp;
        protected System.Windows.Forms.ToolStripButton btnMoveDown;
        protected System.Windows.Forms.ToolStripButton btnMoveTop;
        protected System.Windows.Forms.ToolStripButton btnMoveBottom;
        protected System.Windows.Forms.ColumnHeader cvalue;
        protected Micro.WinForms.NiceToolStrip toolstrip;
        protected Micro.WinForms.ListViewEx list;
        protected System.Windows.Forms.ToolStripButton btnAdd;
        protected System.Windows.Forms.ToolStripButton btnRemove;
        protected System.Windows.Forms.ToolStripButton btnEdit;
        protected System.Windows.Forms.ToolStripButton btnLoad;
        protected System.Windows.Forms.ToolStripButton btnSave;
        protected System.Windows.Forms.ToolStripSeparator sep2;
        protected System.Windows.Forms.ToolStripButton btnExport;
        protected System.Windows.Forms.ToolStripButton btnImport;
        protected System.Windows.Forms.ToolStripSeparator sep3;
        protected System.Windows.Forms.ToolStripButton btnSettings;
        protected System.Windows.Forms.ToolStripSeparator sep1;
        private System.Windows.Forms.ToolStripButton btnDuplicate;
    }
}
