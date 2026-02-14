
namespace smpc_engineering_app.Pages.ItemRequest
{
    partial class ItemRequest
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ItemRequest));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.pnl_top = new System.Windows.Forms.Panel();
            this.cmb_ref_doc = new System.Windows.Forms.ComboBox();
            this.dtp_req_date = new System.Windows.Forms.DateTimePicker();
            this.dtp_issue_date = new System.Windows.Forms.DateTimePicker();
            this.dtp_required_date = new System.Windows.Forms.DateTimePicker();
            this.txt_id = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.txt_doc_no = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.cmb_req_dept = new System.Windows.Forms.ComboBox();
            this.txt_purpose = new System.Windows.Forms.TextBox();
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
            this.cmb_received_by = new System.Windows.Forms.ComboBox();
            this.btn_forward = new System.Windows.Forms.Button();
            this.txt_issued_by = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.txt_approved_by = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.txt_req_by = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.btn_cancel = new System.Windows.Forms.Button();
            this.pnl_mid = new System.Windows.Forms.Panel();
            this.dgv_main = new System.Windows.Forms.DataGridView();
            this.number = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ir_id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.item_id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.so_id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sod_id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.item_description = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.order_qty = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.req_qty = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.req_uom = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.issued_qty = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.issued_uom = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.serial_no = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.remarks = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnl_top.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.panel6.SuspendLayout();
            this.pnl_bot.SuspendLayout();
            this.pnl_mid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_main)).BeginInit();
            this.SuspendLayout();
            // 
            // pnl_top
            // 
            this.pnl_top.Controls.Add(this.cmb_ref_doc);
            this.pnl_top.Controls.Add(this.dtp_req_date);
            this.pnl_top.Controls.Add(this.dtp_issue_date);
            this.pnl_top.Controls.Add(this.dtp_required_date);
            this.pnl_top.Controls.Add(this.txt_id);
            this.pnl_top.Controls.Add(this.label13);
            this.pnl_top.Controls.Add(this.label8);
            this.pnl_top.Controls.Add(this.txt_doc_no);
            this.pnl_top.Controls.Add(this.label7);
            this.pnl_top.Controls.Add(this.label6);
            this.pnl_top.Controls.Add(this.label5);
            this.pnl_top.Controls.Add(this.label4);
            this.pnl_top.Controls.Add(this.cmb_req_dept);
            this.pnl_top.Controls.Add(this.txt_purpose);
            this.pnl_top.Controls.Add(this.label3);
            this.pnl_top.Controls.Add(this.label2);
            this.pnl_top.Controls.Add(this.toolStrip1);
            this.pnl_top.Controls.Add(this.panel6);
            this.pnl_top.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnl_top.Location = new System.Drawing.Point(0, 0);
            this.pnl_top.Name = "pnl_top";
            this.pnl_top.Size = new System.Drawing.Size(1400, 215);
            this.pnl_top.TabIndex = 0;
            // 
            // cmb_ref_doc
            // 
            this.cmb_ref_doc.BackColor = System.Drawing.Color.White;
            this.cmb_ref_doc.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_ref_doc.Enabled = false;
            this.cmb_ref_doc.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmb_ref_doc.FormattingEnabled = true;
            this.cmb_ref_doc.Location = new System.Drawing.Point(1137, 108);
            this.cmb_ref_doc.MaxLength = 50;
            this.cmb_ref_doc.MinimumSize = new System.Drawing.Size(200, 0);
            this.cmb_ref_doc.Name = "cmb_ref_doc";
            this.cmb_ref_doc.Size = new System.Drawing.Size(200, 21);
            this.cmb_ref_doc.TabIndex = 303;
            this.cmb_ref_doc.TabStop = false;
            this.cmb_ref_doc.Tag = "REQUIRED";
            this.cmb_ref_doc.SelectedIndexChanged += new System.EventHandler(this.cmb_ref_doc_SelectedIndexChanged);
            // 
            // dtp_req_date
            // 
            this.dtp_req_date.Enabled = false;
            this.dtp_req_date.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtp_req_date.Location = new System.Drawing.Point(634, 88);
            this.dtp_req_date.Name = "dtp_req_date";
            this.dtp_req_date.Size = new System.Drawing.Size(200, 20);
            this.dtp_req_date.TabIndex = 98;
            this.dtp_req_date.Tag = "REQUIRED";
            // 
            // dtp_issue_date
            // 
            this.dtp_issue_date.Enabled = false;
            this.dtp_issue_date.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtp_issue_date.Location = new System.Drawing.Point(634, 130);
            this.dtp_issue_date.Name = "dtp_issue_date";
            this.dtp_issue_date.Size = new System.Drawing.Size(200, 20);
            this.dtp_issue_date.TabIndex = 97;
            this.dtp_issue_date.Tag = "REQUIRED";
            // 
            // dtp_required_date
            // 
            this.dtp_required_date.Enabled = false;
            this.dtp_required_date.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtp_required_date.Location = new System.Drawing.Point(634, 109);
            this.dtp_required_date.Name = "dtp_required_date";
            this.dtp_required_date.Size = new System.Drawing.Size(200, 20);
            this.dtp_required_date.TabIndex = 96;
            this.dtp_required_date.Tag = "REQUIRED";
            // 
            // txt_id
            // 
            this.txt_id.Location = new System.Drawing.Point(1137, 150);
            this.txt_id.Name = "txt_id";
            this.txt_id.Size = new System.Drawing.Size(200, 20);
            this.txt_id.TabIndex = 29;
            this.txt_id.Tag = "";
            this.txt_id.Visible = false;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(1018, 153);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(105, 13);
            this.label13.TabIndex = 28;
            this.label13.Text = "ITEM REQUEST ID:";
            this.label13.Visible = false;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(1018, 111);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(94, 13);
            this.label8.TabIndex = 26;
            this.label8.Text = "REFERENCE NO:";
            // 
            // txt_doc_no
            // 
            this.txt_doc_no.Enabled = false;
            this.txt_doc_no.Location = new System.Drawing.Point(1137, 87);
            this.txt_doc_no.Name = "txt_doc_no";
            this.txt_doc_no.Size = new System.Drawing.Size(200, 20);
            this.txt_doc_no.TabIndex = 25;
            this.txt_doc_no.Tag = "";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(1018, 90);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(55, 13);
            this.label7.TabIndex = 24;
            this.label7.Text = "DOC. NO:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(515, 133);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(74, 13);
            this.label6.TabIndex = 22;
            this.label6.Text = "ISSUE DATE:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(515, 112);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(99, 13);
            this.label5.TabIndex = 20;
            this.label5.Text = "REQUIRED DATE:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(515, 91);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(94, 13);
            this.label4.TabIndex = 18;
            this.label4.Text = "REQUEST DATE:";
            // 
            // cmb_req_dept
            // 
            this.cmb_req_dept.FormattingEnabled = true;
            this.cmb_req_dept.Items.AddRange(new object[] {
            "Management",
            "Sales",
            "Logistics",
            "Engineering",
            "Accounting",
            "Purchasing",
            "Warehouse"});
            this.cmb_req_dept.Location = new System.Drawing.Point(139, 87);
            this.cmb_req_dept.Name = "cmb_req_dept";
            this.cmb_req_dept.Size = new System.Drawing.Size(200, 21);
            this.cmb_req_dept.TabIndex = 17;
            this.cmb_req_dept.Tag = "REQUIRED";
            this.cmb_req_dept.TextChanged += new System.EventHandler(this.cmb_req_dept_TextChanged);
            // 
            // txt_purpose
            // 
            this.txt_purpose.Location = new System.Drawing.Point(139, 109);
            this.txt_purpose.Name = "txt_purpose";
            this.txt_purpose.Size = new System.Drawing.Size(200, 20);
            this.txt_purpose.TabIndex = 15;
            this.txt_purpose.Tag = "";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(20, 112);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(62, 13);
            this.label3.TabIndex = 16;
            this.label3.Text = "PURPOSE:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(20, 90);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(113, 13);
            this.label2.TabIndex = 14;
            this.label2.Text = "REQUESTING DEPT:";
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
            this.label1.Size = new System.Drawing.Size(138, 25);
            this.label1.TabIndex = 1;
            this.label1.Text = "Item Request";
            // 
            // pnl_bot
            // 
            this.pnl_bot.Controls.Add(this.cmb_received_by);
            this.pnl_bot.Controls.Add(this.btn_forward);
            this.pnl_bot.Controls.Add(this.txt_issued_by);
            this.pnl_bot.Controls.Add(this.label12);
            this.pnl_bot.Controls.Add(this.txt_approved_by);
            this.pnl_bot.Controls.Add(this.label11);
            this.pnl_bot.Controls.Add(this.label10);
            this.pnl_bot.Controls.Add(this.txt_req_by);
            this.pnl_bot.Controls.Add(this.label9);
            this.pnl_bot.Controls.Add(this.btn_cancel);
            this.pnl_bot.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnl_bot.Location = new System.Drawing.Point(0, 740);
            this.pnl_bot.Name = "pnl_bot";
            this.pnl_bot.Size = new System.Drawing.Size(1400, 210);
            this.pnl_bot.TabIndex = 1;
            // 
            // cmb_received_by
            // 
            this.cmb_received_by.FormattingEnabled = true;
            this.cmb_received_by.Items.AddRange(new object[] {
            "Management",
            "Sales",
            "Logistics",
            "Engineering",
            "Accounting",
            "Purchasing",
            "Warehouse"});
            this.cmb_received_by.Location = new System.Drawing.Point(139, 78);
            this.cmb_received_by.Name = "cmb_received_by";
            this.cmb_received_by.Size = new System.Drawing.Size(200, 21);
            this.cmb_received_by.TabIndex = 38;
            this.cmb_received_by.Tag = "REQUIRED";
            // 
            // btn_forward
            // 
            this.btn_forward.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_forward.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(234)))), ((int)(((byte)(211)))));
            this.btn_forward.Enabled = false;
            this.btn_forward.Location = new System.Drawing.Point(1124, 173);
            this.btn_forward.Name = "btn_forward";
            this.btn_forward.Size = new System.Drawing.Size(231, 23);
            this.btn_forward.TabIndex = 36;
            this.btn_forward.Text = "FORWARD TO WAREHOUSE";
            this.btn_forward.UseVisualStyleBackColor = false;
            this.btn_forward.Click += new System.EventHandler(this.btn_forward_Click);
            // 
            // txt_issued_by
            // 
            this.txt_issued_by.Location = new System.Drawing.Point(1155, 78);
            this.txt_issued_by.Name = "txt_issued_by";
            this.txt_issued_by.Size = new System.Drawing.Size(200, 20);
            this.txt_issued_by.TabIndex = 23;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(965, 81);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(67, 13);
            this.label12.TabIndex = 24;
            this.label12.Text = "ISSUED BY:";
            // 
            // txt_approved_by
            // 
            this.txt_approved_by.Location = new System.Drawing.Point(1155, 28);
            this.txt_approved_by.Name = "txt_approved_by";
            this.txt_approved_by.Size = new System.Drawing.Size(200, 20);
            this.txt_approved_by.TabIndex = 21;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(965, 31);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(165, 13);
            this.label11.TabIndex = 22;
            this.label11.Text = "APPROVED/ AUTHORIZED BY:";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(20, 81);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(81, 13);
            this.label10.TabIndex = 20;
            this.label10.Text = "RECEIVED BY:";
            // 
            // txt_req_by
            // 
            this.txt_req_by.Location = new System.Drawing.Point(139, 28);
            this.txt_req_by.Name = "txt_req_by";
            this.txt_req_by.Size = new System.Drawing.Size(200, 20);
            this.txt_req_by.TabIndex = 17;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(20, 31);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(94, 13);
            this.label9.TabIndex = 18;
            this.label9.Text = "REQUESTED BY:";
            // 
            // btn_cancel
            // 
            this.btn_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_cancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.btn_cancel.Enabled = false;
            this.btn_cancel.Location = new System.Drawing.Point(1124, 173);
            this.btn_cancel.Name = "btn_cancel";
            this.btn_cancel.Size = new System.Drawing.Size(231, 23);
            this.btn_cancel.TabIndex = 37;
            this.btn_cancel.Text = "CANCEL REQUEST";
            this.btn_cancel.UseVisualStyleBackColor = false;
            this.btn_cancel.Visible = false;
            this.btn_cancel.Click += new System.EventHandler(this.btn_cancel_Click);
            // 
            // pnl_mid
            // 
            this.pnl_mid.Controls.Add(this.dgv_main);
            this.pnl_mid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnl_mid.Location = new System.Drawing.Point(0, 215);
            this.pnl_mid.Name = "pnl_mid";
            this.pnl_mid.Size = new System.Drawing.Size(1400, 525);
            this.pnl_mid.TabIndex = 2;
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
            this.ir_id,
            this.item_id,
            this.so_id,
            this.sod_id,
            this.item_description,
            this.order_qty,
            this.req_qty,
            this.req_uom,
            this.issued_qty,
            this.issued_uom,
            this.serial_no,
            this.remarks});
            this.dgv_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_main.EnableHeadersVisualStyles = false;
            this.dgv_main.Location = new System.Drawing.Point(0, 0);
            this.dgv_main.Name = "dgv_main";
            this.dgv_main.Size = new System.Drawing.Size(1400, 525);
            this.dgv_main.TabIndex = 1;
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
            this.id.Visible = false;
            // 
            // ir_id
            // 
            this.ir_id.DataPropertyName = "ir_id";
            this.ir_id.HeaderText = "IR ID";
            this.ir_id.Name = "ir_id";
            this.ir_id.Visible = false;
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
            // item_description
            // 
            this.item_description.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.item_description.DataPropertyName = "item_description";
            this.item_description.HeaderText = "ITEM DESCRIPTION";
            this.item_description.MinimumWidth = 200;
            this.item_description.Name = "item_description";
            this.item_description.ReadOnly = true;
            // 
            // order_qty
            // 
            this.order_qty.DataPropertyName = "order_qty";
            this.order_qty.HeaderText = "ORDER QTY";
            this.order_qty.Name = "order_qty";
            this.order_qty.ReadOnly = true;
            this.order_qty.Visible = false;
            // 
            // req_qty
            // 
            this.req_qty.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.req_qty.DataPropertyName = "req_qty";
            this.req_qty.HeaderText = "QTY";
            this.req_qty.MinimumWidth = 80;
            this.req_qty.Name = "req_qty";
            this.req_qty.ReadOnly = true;
            this.req_qty.Width = 80;
            // 
            // req_uom
            // 
            this.req_uom.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.req_uom.DataPropertyName = "req_uom";
            this.req_uom.HeaderText = "UOM";
            this.req_uom.MinimumWidth = 80;
            this.req_uom.Name = "req_uom";
            this.req_uom.ReadOnly = true;
            this.req_uom.Width = 80;
            // 
            // issued_qty
            // 
            this.issued_qty.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.issued_qty.DataPropertyName = "issued_qty";
            this.issued_qty.HeaderText = "QTY";
            this.issued_qty.MinimumWidth = 80;
            this.issued_qty.Name = "issued_qty";
            this.issued_qty.ReadOnly = true;
            this.issued_qty.Width = 80;
            // 
            // issued_uom
            // 
            this.issued_uom.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.issued_uom.DataPropertyName = "issued_uom";
            this.issued_uom.HeaderText = "UOM";
            this.issued_uom.MinimumWidth = 80;
            this.issued_uom.Name = "issued_uom";
            this.issued_uom.ReadOnly = true;
            this.issued_uom.Width = 80;
            // 
            // serial_no
            // 
            this.serial_no.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.serial_no.DataPropertyName = "serial_no";
            this.serial_no.HeaderText = "SERIAL NUMBER";
            this.serial_no.MinimumWidth = 180;
            this.serial_no.Name = "serial_no";
            this.serial_no.ReadOnly = true;
            // 
            // remarks
            // 
            this.remarks.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.remarks.DataPropertyName = "remarks";
            this.remarks.HeaderText = "REMARKS";
            this.remarks.MinimumWidth = 180;
            this.remarks.Name = "remarks";
            this.remarks.ReadOnly = true;
            // 
            // ItemRequest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnl_mid);
            this.Controls.Add(this.pnl_bot);
            this.Controls.Add(this.pnl_top);
            this.Name = "ItemRequest";
            this.Size = new System.Drawing.Size(1400, 950);
            this.Load += new System.EventHandler(this.ItemRequest_Load);
            this.pnl_top.ResumeLayout(false);
            this.pnl_top.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.panel6.ResumeLayout(false);
            this.panel6.PerformLayout();
            this.pnl_bot.ResumeLayout(false);
            this.pnl_bot.PerformLayout();
            this.pnl_mid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_main)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnl_top;
        private System.Windows.Forms.Panel pnl_bot;
        private System.Windows.Forms.Panel pnl_mid;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btn_new;
        private System.Windows.Forms.ToolStripButton btn_search;
        private System.Windows.Forms.ToolStripButton btn_edit;
        private System.Windows.Forms.ToolStripButton btn_delete;
        private System.Windows.Forms.ToolStripButton btn_save;
        private System.Windows.Forms.ToolStripButton btn_close;
        private System.Windows.Forms.ToolStripButton btn_prev;
        private System.Windows.Forms.ToolStripButton btn_next;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txt_purpose;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cmb_req_dept;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txt_doc_no;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.DataGridView dgv_main;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txt_req_by;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txt_issued_by;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox txt_approved_by;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button btn_forward;
        private System.Windows.Forms.TextBox txt_id;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Button btn_cancel;
        private System.Windows.Forms.DateTimePicker dtp_required_date;
        private System.Windows.Forms.DateTimePicker dtp_issue_date;
        private System.Windows.Forms.ComboBox cmb_received_by;
        private System.Windows.Forms.DataGridViewTextBoxColumn number;
        private System.Windows.Forms.DataGridViewTextBoxColumn id;
        private System.Windows.Forms.DataGridViewTextBoxColumn ir_id;
        private System.Windows.Forms.DataGridViewTextBoxColumn item_id;
        private System.Windows.Forms.DataGridViewTextBoxColumn so_id;
        private System.Windows.Forms.DataGridViewTextBoxColumn sod_id;
        private System.Windows.Forms.DataGridViewTextBoxColumn item_description;
        private System.Windows.Forms.DataGridViewTextBoxColumn order_qty;
        private System.Windows.Forms.DataGridViewTextBoxColumn req_qty;
        private System.Windows.Forms.DataGridViewTextBoxColumn req_uom;
        private System.Windows.Forms.DataGridViewTextBoxColumn issued_qty;
        private System.Windows.Forms.DataGridViewTextBoxColumn issued_uom;
        private System.Windows.Forms.DataGridViewTextBoxColumn serial_no;
        private System.Windows.Forms.DataGridViewTextBoxColumn remarks;
        private System.Windows.Forms.DateTimePicker dtp_req_date;
        private System.Windows.Forms.ComboBox cmb_ref_doc;
    }
}
