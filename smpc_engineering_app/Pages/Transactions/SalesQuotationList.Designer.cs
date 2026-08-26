namespace smpc_engineering_app.Pages.Transactions
{
    partial class SalesQuotationList
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.txt_search = new System.Windows.Forms.TextBox();
            this.dgv_quotation_list = new System.Windows.Forms.DataGridView();
            this.col_id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_date_requested = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_client_name = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_document_no = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_project_name = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_sales_executive = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_status = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_wiring = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_quotation_list)).BeginInit();
            this.SuspendLayout();
            //
            // panel1
            //
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.MaximumSize = new System.Drawing.Size(1342, 47);
            this.panel1.MinimumSize = new System.Drawing.Size(1342, 47);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1342, 47);
            this.panel1.TabIndex = 21;
            //
            // label1
            //
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(18, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(259, 25);
            this.label1.TabIndex = 1;
            this.label1.Text = "SALES QUOTATION LIST";
            //
            // panel2
            //
            this.panel2.Controls.Add(this.txt_search);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 47);
            this.panel2.MaximumSize = new System.Drawing.Size(1342, 60);
            this.panel2.MinimumSize = new System.Drawing.Size(1342, 60);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1342, 60);
            this.panel2.TabIndex = 22;
            //
            // txt_search
            //
            this.txt_search.Location = new System.Drawing.Point(14, 21);
            this.txt_search.Name = "txt_search";
            this.txt_search.Size = new System.Drawing.Size(409, 20);
            this.txt_search.TabIndex = 254;
            //
            // dgv_quotation_list
            //
            this.dgv_quotation_list.AllowUserToAddRows = false;
            this.dgv_quotation_list.AllowUserToDeleteRows = false;
            this.dgv_quotation_list.ReadOnly = true;
            this.dgv_quotation_list.RowHeadersVisible = false;
            this.dgv_quotation_list.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgv_quotation_list.MultiSelect = false;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgv_quotation_list.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgv_quotation_list.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.col_id,
            this.col_date_requested,
            this.col_client_name,
            this.col_document_no,
            this.col_project_name,
            this.col_sales_executive,
            this.col_status,
            this.col_wiring});
            this.dgv_quotation_list.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_quotation_list.Location = new System.Drawing.Point(0, 107);
            this.dgv_quotation_list.Name = "dgv_quotation_list";
            this.dgv_quotation_list.Size = new System.Drawing.Size(1400, 843);
            this.dgv_quotation_list.TabIndex = 23;
            //
            // col_id
            //
            this.col_id.DataPropertyName = "id";
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.col_id.DefaultCellStyle = dataGridViewCellStyle2;
            this.col_id.HeaderText = "ID";
            this.col_id.Name = "col_id";
            this.col_id.ReadOnly = true;
            this.col_id.Visible = false;
            //
            // col_date_requested
            //
            this.col_date_requested.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.col_date_requested.DataPropertyName = "requested_for_engr_date";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.col_date_requested.DefaultCellStyle = dataGridViewCellStyle3;
            this.col_date_requested.HeaderText = "DATE REQUESTED";
            this.col_date_requested.Name = "col_date_requested";
            this.col_date_requested.ReadOnly = true;
            //
            // col_client_name
            //
            this.col_client_name.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.col_client_name.DataPropertyName = "client_name";
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.col_client_name.DefaultCellStyle = dataGridViewCellStyle4;
            this.col_client_name.HeaderText = "CLIENT";
            this.col_client_name.Name = "col_client_name";
            this.col_client_name.ReadOnly = true;
            this.col_client_name.Width = 200;
            //
            // col_document_no
            //
            this.col_document_no.DataPropertyName = "sales_quotation";
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(102)))), ((int)(((byte)(204)))));
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.col_document_no.DefaultCellStyle = dataGridViewCellStyle5;
            this.col_document_no.HeaderText = "DOC NO.";
            this.col_document_no.Name = "col_document_no";
            this.col_document_no.ReadOnly = true;
            this.col_document_no.Width = 150;
            //
            // col_project_name
            //
            this.col_project_name.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.col_project_name.DataPropertyName = "project_name";
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.col_project_name.DefaultCellStyle = dataGridViewCellStyle6;
            this.col_project_name.HeaderText = "PROJECT NAME";
            this.col_project_name.Name = "col_project_name";
            this.col_project_name.ReadOnly = true;
            this.col_project_name.Width = 250;
            //
            // col_sales_executive
            //
            this.col_sales_executive.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.col_sales_executive.DataPropertyName = "sales_executive";
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.col_sales_executive.DefaultCellStyle = dataGridViewCellStyle7;
            this.col_sales_executive.HeaderText = "SALES EXECUTIVE";
            this.col_sales_executive.Name = "col_sales_executive";
            this.col_sales_executive.ReadOnly = true;
            this.col_sales_executive.Width = 150;
            //
            // col_status
            //
            this.col_status.DataPropertyName = "status";
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.col_status.DefaultCellStyle = dataGridViewCellStyle8;
            this.col_status.HeaderText = "STATUS";
            this.col_status.Name = "col_status";
            this.col_status.ReadOnly = true;
            this.col_status.Width = 120;
            //
            // col_wiring
            //
            this.col_wiring.DataPropertyName = "remark";
            this.col_wiring.HeaderText = "WIRING";
            this.col_wiring.Name = "col_wiring";
            this.col_wiring.ReadOnly = true;
            this.col_wiring.Width = 130;
            //
            // SalesQuotationList
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dgv_quotation_list);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "SalesQuotationList";
            this.Size = new System.Drawing.Size(1400, 950);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_quotation_list)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TextBox txt_search;
        private System.Windows.Forms.DataGridView dgv_quotation_list;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_id;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_date_requested;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_client_name;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_document_no;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_project_name;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_sales_executive;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_status;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_wiring;
    }
}
