
namespace smpc_engineering_app.Pages.ItemRequest.ItemRequestModals
{
    partial class ItemRequestSearch
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dgv_ir_search = new System.Windows.Forms.DataGridView();
            this.txt_search = new System.Windows.Forms.TextBox();
            this.id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.doc_no = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.purpose = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.req_by = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.req_date = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.required_date = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_ir_search)).BeginInit();
            this.SuspendLayout();
            // 
            // dgv_ir_search
            // 
            this.dgv_ir_search.AllowUserToAddRows = false;
            this.dgv_ir_search.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgv_ir_search.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgv_ir_search.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_ir_search.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.id,
            this.doc_no,
            this.purpose,
            this.req_by,
            this.req_date,
            this.required_date});
            this.dgv_ir_search.Location = new System.Drawing.Point(-1, 31);
            this.dgv_ir_search.Name = "dgv_ir_search";
            this.dgv_ir_search.Size = new System.Drawing.Size(802, 389);
            this.dgv_ir_search.TabIndex = 3;
            this.dgv_ir_search.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_ir_search_CellClick);
            // 
            // txt_search
            // 
            this.txt_search.Location = new System.Drawing.Point(350, 215);
            this.txt_search.Name = "txt_search";
            this.txt_search.Size = new System.Drawing.Size(100, 20);
            this.txt_search.TabIndex = 4;
            this.txt_search.TextChanged += new System.EventHandler(this.txt_search_TextChanged);
            // 
            // id
            // 
            this.id.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.id.DataPropertyName = "id";
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.Gainsboro;
            this.id.DefaultCellStyle = dataGridViewCellStyle2;
            this.id.HeaderText = "ID";
            this.id.Name = "id";
            this.id.ReadOnly = true;
            this.id.Visible = false;
            this.id.Width = 80;
            // 
            // doc_no
            // 
            this.doc_no.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.doc_no.DataPropertyName = "doc_no";
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.Gainsboro;
            this.doc_no.DefaultCellStyle = dataGridViewCellStyle3;
            this.doc_no.HeaderText = "DOC NO";
            this.doc_no.Name = "doc_no";
            this.doc_no.ReadOnly = true;
            // 
            // purpose
            // 
            this.purpose.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.purpose.DataPropertyName = "purpose";
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.Gainsboro;
            this.purpose.DefaultCellStyle = dataGridViewCellStyle4;
            this.purpose.HeaderText = "PURPOSE";
            this.purpose.Name = "purpose";
            this.purpose.ReadOnly = true;
            // 
            // req_by
            // 
            this.req_by.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.req_by.DataPropertyName = "req_by";
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.Gainsboro;
            this.req_by.DefaultCellStyle = dataGridViewCellStyle5;
            this.req_by.HeaderText = "REQUESTOR";
            this.req_by.Name = "req_by";
            this.req_by.ReadOnly = true;
            // 
            // req_date
            // 
            this.req_date.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.req_date.DataPropertyName = "req_date";
            dataGridViewCellStyle6.BackColor = System.Drawing.Color.Gainsboro;
            this.req_date.DefaultCellStyle = dataGridViewCellStyle6;
            this.req_date.HeaderText = "REQUEST DATE";
            this.req_date.MinimumWidth = 150;
            this.req_date.Name = "req_date";
            this.req_date.ReadOnly = true;
            // 
            // required_date
            // 
            this.required_date.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.required_date.DataPropertyName = "required_date";
            dataGridViewCellStyle7.BackColor = System.Drawing.Color.Gainsboro;
            this.required_date.DefaultCellStyle = dataGridViewCellStyle7;
            this.required_date.HeaderText = "REQUIRED DATE";
            this.required_date.Name = "required_date";
            this.required_date.ReadOnly = true;
            // 
            // ItemRequestSearch
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.dgv_ir_search);
            this.Controls.Add(this.txt_search);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "ItemRequestSearch";
            this.Text = "ItemRequestSearch";
            this.Load += new System.EventHandler(this.ItemRequestSearch_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_ir_search)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dgv_ir_search;
        private System.Windows.Forms.TextBox txt_search;
        private System.Windows.Forms.DataGridViewTextBoxColumn id;
        private System.Windows.Forms.DataGridViewTextBoxColumn doc_no;
        private System.Windows.Forms.DataGridViewTextBoxColumn purpose;
        private System.Windows.Forms.DataGridViewTextBoxColumn req_by;
        private System.Windows.Forms.DataGridViewTextBoxColumn req_date;
        private System.Windows.Forms.DataGridViewTextBoxColumn required_date;
    }
}