
namespace smpc_engineering_app.Pages.PickActivity
{
    partial class PickActivity
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PickActivity));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.pnl_top = new System.Windows.Forms.Panel();
            this.cmb_reference_so = new System.Windows.Forms.ComboBox();
            this.txt_id = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.txt_doc_no = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txt_customer = new System.Windows.Forms.TextBox();
            this.txt_sales_person = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.txt_picked_by = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txt_prepared_by = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txt_code = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btn_new = new System.Windows.Forms.ToolStripButton();
            this.btn_search = new System.Windows.Forms.ToolStripButton();
            this.btn_edit = new System.Windows.Forms.ToolStripButton();
            this.btn_delete = new System.Windows.Forms.ToolStripButton();
            this.btn_save = new System.Windows.Forms.ToolStripButton();
            this.btn_close = new System.Windows.Forms.ToolStripButton();
            this.btn_next = new System.Windows.Forms.ToolStripButton();
            this.btn_prev = new System.Windows.Forms.ToolStripButton();
            this.panel6 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.pnl_bot = new System.Windows.Forms.Panel();
            this.btn_print = new System.Windows.Forms.Button();
            this.dgv_main = new System.Windows.Forms.DataGridView();
            this.number = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pa_id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.item_id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.so_id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sod_id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.item_code = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.item_description = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.left_qty = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.left_uom = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pick_qty = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pick_uom = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.actual_qty = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.actual_uom = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.bin_location = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.warehouse_id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnl_top.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.panel6.SuspendLayout();
            this.pnl_bot.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_main)).BeginInit();
            this.SuspendLayout();
            // 
            // pnl_top
            // 
            this.pnl_top.Controls.Add(this.cmb_reference_so);
            this.pnl_top.Controls.Add(this.txt_id);
            this.pnl_top.Controls.Add(this.label13);
            this.pnl_top.Controls.Add(this.txt_doc_no);
            this.pnl_top.Controls.Add(this.label4);
            this.pnl_top.Controls.Add(this.txt_customer);
            this.pnl_top.Controls.Add(this.txt_sales_person);
            this.pnl_top.Controls.Add(this.label10);
            this.pnl_top.Controls.Add(this.label9);
            this.pnl_top.Controls.Add(this.txt_picked_by);
            this.pnl_top.Controls.Add(this.label8);
            this.pnl_top.Controls.Add(this.txt_prepared_by);
            this.pnl_top.Controls.Add(this.label7);
            this.pnl_top.Controls.Add(this.txt_code);
            this.pnl_top.Controls.Add(this.label3);
            this.pnl_top.Controls.Add(this.label2);
            this.pnl_top.Controls.Add(this.toolStrip1);
            this.pnl_top.Controls.Add(this.panel6);
            this.pnl_top.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnl_top.Location = new System.Drawing.Point(0, 0);
            this.pnl_top.Name = "pnl_top";
            this.pnl_top.Size = new System.Drawing.Size(1400, 215);
            this.pnl_top.TabIndex = 1;
            // 
            // cmb_reference_so
            // 
            this.cmb_reference_so.BackColor = System.Drawing.Color.White;
            this.cmb_reference_so.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_reference_so.Enabled = false;
            this.cmb_reference_so.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmb_reference_so.FormattingEnabled = true;
            this.cmb_reference_so.Location = new System.Drawing.Point(139, 131);
            this.cmb_reference_so.MaxLength = 50;
            this.cmb_reference_so.MinimumSize = new System.Drawing.Size(200, 0);
            this.cmb_reference_so.Name = "cmb_reference_so";
            this.cmb_reference_so.Size = new System.Drawing.Size(200, 21);
            this.cmb_reference_so.TabIndex = 99;
            this.cmb_reference_so.TabStop = false;
            this.cmb_reference_so.Tag = "REQUIRED";
            this.cmb_reference_so.SelectedIndexChanged += new System.EventHandler(this.cmb_reference_so_SelectedIndexChanged);
            // 
            // txt_id
            // 
            this.txt_id.Location = new System.Drawing.Point(1164, 177);
            this.txt_id.Name = "txt_id";
            this.txt_id.Size = new System.Drawing.Size(200, 20);
            this.txt_id.TabIndex = 38;
            this.txt_id.Tag = "";
            this.txt_id.Visible = false;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(1045, 180);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(99, 13);
            this.label13.TabIndex = 37;
            this.label13.Text = "PICK ACTIVITY ID:";
            this.label13.Visible = false;
            // 
            // txt_doc_no
            // 
            this.txt_doc_no.Location = new System.Drawing.Point(1164, 90);
            this.txt_doc_no.Name = "txt_doc_no";
            this.txt_doc_no.Size = new System.Drawing.Size(99, 20);
            this.txt_doc_no.TabIndex = 36;
            this.txt_doc_no.Tag = "";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(1045, 93);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(52, 13);
            this.label4.TabIndex = 35;
            this.label4.Text = "DOC NO:";
            // 
            // txt_customer
            // 
            this.txt_customer.Location = new System.Drawing.Point(139, 88);
            this.txt_customer.Name = "txt_customer";
            this.txt_customer.Size = new System.Drawing.Size(370, 20);
            this.txt_customer.TabIndex = 34;
            this.txt_customer.Tag = "";
            // 
            // txt_sales_person
            // 
            this.txt_sales_person.Location = new System.Drawing.Point(139, 154);
            this.txt_sales_person.Name = "txt_sales_person";
            this.txt_sales_person.Size = new System.Drawing.Size(200, 20);
            this.txt_sales_person.TabIndex = 32;
            this.txt_sales_person.Tag = "";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(20, 157);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(92, 13);
            this.label10.TabIndex = 33;
            this.label10.Text = "SALES PERSON:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(20, 133);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(93, 13);
            this.label9.TabIndex = 31;
            this.label9.Text = "REFERENCE SO:";
            // 
            // txt_picked_by
            // 
            this.txt_picked_by.Location = new System.Drawing.Point(1164, 151);
            this.txt_picked_by.Name = "txt_picked_by";
            this.txt_picked_by.Size = new System.Drawing.Size(200, 20);
            this.txt_picked_by.TabIndex = 29;
            this.txt_picked_by.Tag = "";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(1045, 154);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(66, 13);
            this.label8.TabIndex = 26;
            this.label8.Text = "PICKED BY:";
            // 
            // txt_prepared_by
            // 
            this.txt_prepared_by.Location = new System.Drawing.Point(1164, 130);
            this.txt_prepared_by.Name = "txt_prepared_by";
            this.txt_prepared_by.Size = new System.Drawing.Size(200, 20);
            this.txt_prepared_by.TabIndex = 25;
            this.txt_prepared_by.Tag = "";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(1045, 133);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(86, 13);
            this.label7.TabIndex = 24;
            this.label7.Text = "PREPARED BY:";
            // 
            // txt_code
            // 
            this.txt_code.Location = new System.Drawing.Point(139, 109);
            this.txt_code.Name = "txt_code";
            this.txt_code.Size = new System.Drawing.Size(200, 20);
            this.txt_code.TabIndex = 15;
            this.txt_code.Tag = "";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(20, 112);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(40, 13);
            this.label3.TabIndex = 16;
            this.label3.Text = "CODE:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(20, 90);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 13);
            this.label2.TabIndex = 14;
            this.label2.Text = "CUSTOMER:";
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btn_new,
            this.btn_search,
            this.btn_edit,
            this.btn_delete,
            this.btn_save,
            this.btn_close,
            this.btn_next,
            this.btn_prev});
            this.toolStrip1.Location = new System.Drawing.Point(0, 47);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1400, 25);
            this.toolStrip1.Stretch = true;
            this.toolStrip1.TabIndex = 13;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btn_new
            // 
            this.btn_new.Image = ((System.Drawing.Image)(resources.GetObject("btn_new.Image")));
            this.btn_new.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btn_new.Name = "btn_new";
            this.btn_new.Size = new System.Drawing.Size(51, 22);
            this.btn_new.Text = "New";
            this.btn_new.Click += new System.EventHandler(this.btn_new_Click);
            // 
            // btn_search
            // 
            this.btn_search.Image = ((System.Drawing.Image)(resources.GetObject("btn_search.Image")));
            this.btn_search.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btn_search.Name = "btn_search";
            this.btn_search.Size = new System.Drawing.Size(62, 22);
            this.btn_search.Text = "Search";
            this.btn_search.Click += new System.EventHandler(this.btn_search_Click);
            // 
            // btn_edit
            // 
            this.btn_edit.Image = ((System.Drawing.Image)(resources.GetObject("btn_edit.Image")));
            this.btn_edit.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btn_edit.Name = "btn_edit";
            this.btn_edit.Size = new System.Drawing.Size(47, 22);
            this.btn_edit.Text = "Edit";
            this.btn_edit.Click += new System.EventHandler(this.btn_edit_Click);
            // 
            // btn_delete
            // 
            this.btn_delete.Image = ((System.Drawing.Image)(resources.GetObject("btn_delete.Image")));
            this.btn_delete.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btn_delete.Name = "btn_delete";
            this.btn_delete.Size = new System.Drawing.Size(60, 22);
            this.btn_delete.Text = "Delete";
            this.btn_delete.Click += new System.EventHandler(this.btn_delete_Click);
            // 
            // btn_save
            // 
            this.btn_save.Image = ((System.Drawing.Image)(resources.GetObject("btn_save.Image")));
            this.btn_save.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btn_save.Name = "btn_save";
            this.btn_save.Size = new System.Drawing.Size(51, 22);
            this.btn_save.Text = "Save";
            this.btn_save.Visible = false;
            this.btn_save.Click += new System.EventHandler(this.btn_save_Click);
            // 
            // btn_close
            // 
            this.btn_close.Image = ((System.Drawing.Image)(resources.GetObject("btn_close.Image")));
            this.btn_close.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btn_close.Name = "btn_close";
            this.btn_close.Size = new System.Drawing.Size(56, 22);
            this.btn_close.Text = "Close";
            this.btn_close.Visible = false;
            this.btn_close.Click += new System.EventHandler(this.btn_close_Click);
            // 
            // btn_next
            // 
            this.btn_next.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.btn_next.Image = ((System.Drawing.Image)(resources.GetObject("btn_next.Image")));
            this.btn_next.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btn_next.Name = "btn_next";
            this.btn_next.Size = new System.Drawing.Size(52, 22);
            this.btn_next.Text = "Next";
            this.btn_next.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.btn_next.Click += new System.EventHandler(this.btn_next_Click);
            // 
            // btn_prev
            // 
            this.btn_prev.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.btn_prev.Image = ((System.Drawing.Image)(resources.GetObject("btn_prev.Image")));
            this.btn_prev.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btn_prev.Name = "btn_prev";
            this.btn_prev.Size = new System.Drawing.Size(72, 22);
            this.btn_prev.Text = "Previous";
            this.btn_prev.Click += new System.EventHandler(this.btn_prev_Click);
            // 
            // panel6
            // 
            this.panel6.Controls.Add(this.label1);
            this.panel6.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel6.Location = new System.Drawing.Point(0, 0);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(1400, 47);
            this.panel6.TabIndex = 10;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F);
            this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label1.Location = new System.Drawing.Point(18, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(128, 25);
            this.label1.TabIndex = 1;
            this.label1.Text = "Pick Activity";
            // 
            // pnl_bot
            // 
            this.pnl_bot.Controls.Add(this.btn_print);
            this.pnl_bot.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnl_bot.Location = new System.Drawing.Point(0, 825);
            this.pnl_bot.Name = "pnl_bot";
            this.pnl_bot.Size = new System.Drawing.Size(1400, 125);
            this.pnl_bot.TabIndex = 2;
            // 
            // btn_print
            // 
            this.btn_print.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_print.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(207)))), ((int)(((byte)(226)))), ((int)(((byte)(243)))));
            this.btn_print.Location = new System.Drawing.Point(43, 85);
            this.btn_print.Name = "btn_print";
            this.btn_print.Size = new System.Drawing.Size(114, 23);
            this.btn_print.TabIndex = 38;
            this.btn_print.Text = "PRINT";
            this.btn_print.UseVisualStyleBackColor = false;
            // 
            // dgv_main
            // 
            this.dgv_main.AllowUserToAddRows = false;
            this.dgv_main.AllowUserToDeleteRows = false;
            this.dgv_main.AllowUserToResizeColumns = false;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgv_main.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgv_main.ColumnHeadersHeight = 50;
            this.dgv_main.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgv_main.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.number,
            this.id,
            this.pa_id,
            this.item_id,
            this.so_id,
            this.sod_id,
            this.item_code,
            this.item_description,
            this.left_qty,
            this.left_uom,
            this.pick_qty,
            this.pick_uom,
            this.actual_qty,
            this.actual_uom,
            this.bin_location,
            this.warehouse_id});
            this.dgv_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_main.EnableHeadersVisualStyles = false;
            this.dgv_main.Location = new System.Drawing.Point(0, 215);
            this.dgv_main.Name = "dgv_main";
            this.dgv_main.Size = new System.Drawing.Size(1400, 610);
            this.dgv_main.TabIndex = 3;
            this.dgv_main.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_main_CellClick);
            this.dgv_main.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(this.dgv_main_EditingControlShowing);
            this.dgv_main.RowPostPaint += new System.Windows.Forms.DataGridViewRowPostPaintEventHandler(this.dgv_main_RowPostPaint);
            // 
            // number
            // 
            this.number.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.number.DataPropertyName = "number";
            this.number.Frozen = true;
            this.number.HeaderText = "#";
            this.number.Name = "number";
            this.number.ReadOnly = true;
            this.number.Width = 50;
            // 
            // id
            // 
            this.id.DataPropertyName = "id";
            this.id.HeaderText = "ID";
            this.id.Name = "id";
            this.id.ReadOnly = true;
            this.id.Visible = false;
            // 
            // pa_id
            // 
            this.pa_id.DataPropertyName = "pa_id";
            this.pa_id.HeaderText = "PA ID";
            this.pa_id.Name = "pa_id";
            this.pa_id.ReadOnly = true;
            this.pa_id.Visible = false;
            // 
            // item_id
            // 
            this.item_id.DataPropertyName = "item_id";
            this.item_id.HeaderText = "ITEM ID";
            this.item_id.Name = "item_id";
            this.item_id.ReadOnly = true;
            this.item_id.Visible = false;
            // 
            // so_id
            // 
            this.so_id.DataPropertyName = "so_id";
            this.so_id.HeaderText = "SO ID";
            this.so_id.Name = "so_id";
            this.so_id.ReadOnly = true;
            this.so_id.Visible = false;
            // 
            // sod_id
            // 
            this.sod_id.DataPropertyName = "sod_id";
            this.sod_id.HeaderText = "SOD ID";
            this.sod_id.Name = "sod_id";
            this.sod_id.ReadOnly = true;
            this.sod_id.Visible = false;
            // 
            // item_code
            // 
            this.item_code.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.item_code.DataPropertyName = "item_code";
            this.item_code.HeaderText = "ITEM CODE";
            this.item_code.MinimumWidth = 100;
            this.item_code.Name = "item_code";
            this.item_code.ReadOnly = true;
            // 
            // item_description
            // 
            this.item_description.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.item_description.DataPropertyName = "item_description";
            this.item_description.HeaderText = "ITEM DESCRIPTION";
            this.item_description.MinimumWidth = 200;
            this.item_description.Name = "item_description";
            this.item_description.ReadOnly = true;
            // 
            // left_qty
            // 
            this.left_qty.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.left_qty.DataPropertyName = "left_qty";
            this.left_qty.HeaderText = "QTY";
            this.left_qty.MinimumWidth = 80;
            this.left_qty.Name = "left_qty";
            this.left_qty.ReadOnly = true;
            this.left_qty.Width = 80;
            // 
            // left_uom
            // 
            this.left_uom.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.left_uom.DataPropertyName = "left_uom";
            this.left_uom.HeaderText = "UOM";
            this.left_uom.MinimumWidth = 80;
            this.left_uom.Name = "left_uom";
            this.left_uom.ReadOnly = true;
            this.left_uom.Width = 80;
            // 
            // pick_qty
            // 
            this.pick_qty.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.pick_qty.DataPropertyName = "pick_qty";
            this.pick_qty.HeaderText = "QTY";
            this.pick_qty.MinimumWidth = 80;
            this.pick_qty.Name = "pick_qty";
            this.pick_qty.ReadOnly = true;
            this.pick_qty.Width = 80;
            // 
            // pick_uom
            // 
            this.pick_uom.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.pick_uom.DataPropertyName = "left_uom";
            this.pick_uom.HeaderText = "UOM";
            this.pick_uom.MinimumWidth = 80;
            this.pick_uom.Name = "pick_uom";
            this.pick_uom.ReadOnly = true;
            this.pick_uom.Width = 80;
            // 
            // actual_qty
            // 
            this.actual_qty.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.actual_qty.DataPropertyName = "actual_qty";
            this.actual_qty.HeaderText = "QTY";
            this.actual_qty.MinimumWidth = 80;
            this.actual_qty.Name = "actual_qty";
            this.actual_qty.ReadOnly = true;
            this.actual_qty.Width = 80;
            // 
            // actual_uom
            // 
            this.actual_uom.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.actual_uom.DataPropertyName = "actual_uom";
            this.actual_uom.HeaderText = "UOM";
            this.actual_uom.MinimumWidth = 80;
            this.actual_uom.Name = "actual_uom";
            this.actual_uom.ReadOnly = true;
            this.actual_uom.Width = 80;
            // 
            // bin_location
            // 
            this.bin_location.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.bin_location.DataPropertyName = "bin_location";
            this.bin_location.HeaderText = "NEW BIN LOCATION";
            this.bin_location.MinimumWidth = 250;
            this.bin_location.Name = "bin_location";
            this.bin_location.ReadOnly = true;
            this.bin_location.Width = 250;
            // 
            // warehouse_id
            // 
            this.warehouse_id.DataPropertyName = "warehouse_id";
            this.warehouse_id.HeaderText = "WAREHOUSE ID";
            this.warehouse_id.Name = "warehouse_id";
            this.warehouse_id.ReadOnly = true;
            this.warehouse_id.Visible = false;
            // 
            // PickActivity
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dgv_main);
            this.Controls.Add(this.pnl_bot);
            this.Controls.Add(this.pnl_top);
            this.Name = "PickActivity";
            this.Size = new System.Drawing.Size(1400, 950);
            this.Load += new System.EventHandler(this.PickActivity_Load);
            this.pnl_top.ResumeLayout(false);
            this.pnl_top.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.panel6.ResumeLayout(false);
            this.panel6.PerformLayout();
            this.pnl_bot.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_main)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnl_top;
        private System.Windows.Forms.TextBox txt_picked_by;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txt_prepared_by;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txt_code;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btn_new;
        private System.Windows.Forms.ToolStripButton btn_search;
        private System.Windows.Forms.ToolStripButton btn_edit;
        private System.Windows.Forms.ToolStripButton btn_delete;
        private System.Windows.Forms.ToolStripButton btn_save;
        private System.Windows.Forms.ToolStripButton btn_close;
        private System.Windows.Forms.ToolStripButton btn_next;
        private System.Windows.Forms.ToolStripButton btn_prev;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txt_sales_person;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txt_customer;
        private System.Windows.Forms.TextBox txt_doc_no;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel pnl_bot;
        private System.Windows.Forms.Button btn_print;
        private System.Windows.Forms.DataGridView dgv_main;
        private System.Windows.Forms.TextBox txt_id;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.ComboBox cmb_reference_so;
        private System.Windows.Forms.DataGridViewTextBoxColumn number;
        private System.Windows.Forms.DataGridViewTextBoxColumn id;
        private System.Windows.Forms.DataGridViewTextBoxColumn pa_id;
        private System.Windows.Forms.DataGridViewTextBoxColumn item_id;
        private System.Windows.Forms.DataGridViewTextBoxColumn so_id;
        private System.Windows.Forms.DataGridViewTextBoxColumn sod_id;
        private System.Windows.Forms.DataGridViewTextBoxColumn item_code;
        private System.Windows.Forms.DataGridViewTextBoxColumn item_description;
        private System.Windows.Forms.DataGridViewTextBoxColumn left_qty;
        private System.Windows.Forms.DataGridViewTextBoxColumn left_uom;
        private System.Windows.Forms.DataGridViewTextBoxColumn pick_qty;
        private System.Windows.Forms.DataGridViewTextBoxColumn pick_uom;
        private System.Windows.Forms.DataGridViewTextBoxColumn actual_qty;
        private System.Windows.Forms.DataGridViewTextBoxColumn actual_uom;
        private System.Windows.Forms.DataGridViewTextBoxColumn bin_location;
        private System.Windows.Forms.DataGridViewTextBoxColumn warehouse_id;
    }
}
