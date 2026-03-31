
namespace smpc_engineering_app.Pages.SalesOrderEngineering.SalesOrderEngineeringModals
{
    partial class SalesOrderEngSearch
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dgv_so_search = new System.Windows.Forms.DataGridView();
            this.txt_search = new System.Windows.Forms.TextBox();
            this.id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.customer = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.code = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tin = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.doc_no = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.doc_date = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.delivery_date = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.delivery_to = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.bill_to = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_so_search)).BeginInit();
            this.SuspendLayout();
            // 
            // dgv_so_search
            // 
            this.dgv_so_search.AllowUserToAddRows = false;
            this.dgv_so_search.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgv_so_search.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgv_so_search.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_so_search.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.id,
            this.customer,
            this.code,
            this.tin,
            this.doc_no,
            this.doc_date,
            this.delivery_date,
            this.delivery_to,
            this.bill_to});
            this.dgv_so_search.Location = new System.Drawing.Point(-1, 31);
            this.dgv_so_search.Name = "dgv_so_search";
            this.dgv_so_search.Size = new System.Drawing.Size(802, 389);
            this.dgv_so_search.TabIndex = 10;
            this.dgv_so_search.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_so_search_CellClick);
            // 
            // txt_search
            // 
            this.txt_search.Location = new System.Drawing.Point(350, 215);
            this.txt_search.Name = "txt_search";
            this.txt_search.Size = new System.Drawing.Size(100, 20);
            this.txt_search.TabIndex = 11;
            // 
            // id
            // 
            this.id.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.id.DataPropertyName = "id";
            this.id.HeaderText = "ID";
            this.id.Name = "id";
            this.id.ReadOnly = true;
            this.id.Visible = false;
            this.id.Width = 80;
            // 
            // customer
            // 
            this.customer.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.customer.DataPropertyName = "customer";
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.Gainsboro;
            this.customer.DefaultCellStyle = dataGridViewCellStyle2;
            this.customer.HeaderText = "CUSTOMER";
            this.customer.Name = "customer";
            this.customer.ReadOnly = true;
            // 
            // code
            // 
            this.code.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.code.DataPropertyName = "code";
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.Gainsboro;
            this.code.DefaultCellStyle = dataGridViewCellStyle3;
            this.code.HeaderText = "CUSTOMER CODE";
            this.code.Name = "code";
            this.code.ReadOnly = true;
            // 
            // tin
            // 
            this.tin.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.tin.DataPropertyName = "tin";
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.Gainsboro;
            this.tin.DefaultCellStyle = dataGridViewCellStyle4;
            this.tin.HeaderText = "TIN";
            this.tin.Name = "tin";
            this.tin.ReadOnly = true;
            // 
            // doc_no
            // 
            this.doc_no.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.doc_no.DataPropertyName = "doc_no";
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.Gainsboro;
            this.doc_no.DefaultCellStyle = dataGridViewCellStyle5;
            this.doc_no.HeaderText = "DOC NO";
            this.doc_no.Name = "doc_no";
            this.doc_no.ReadOnly = true;
            // 
            // doc_date
            // 
            this.doc_date.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.doc_date.DataPropertyName = "doc_date";
            dataGridViewCellStyle6.BackColor = System.Drawing.Color.Gainsboro;
            this.doc_date.DefaultCellStyle = dataGridViewCellStyle6;
            this.doc_date.HeaderText = "DOC DATE";
            this.doc_date.Name = "doc_date";
            this.doc_date.ReadOnly = true;
            // 
            // delivery_date
            // 
            this.delivery_date.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.delivery_date.DataPropertyName = "delivery_date";
            dataGridViewCellStyle7.BackColor = System.Drawing.Color.Gainsboro;
            this.delivery_date.DefaultCellStyle = dataGridViewCellStyle7;
            this.delivery_date.HeaderText = "DELIVERY DATE";
            this.delivery_date.Name = "delivery_date";
            this.delivery_date.ReadOnly = true;
            // 
            // delivery_to
            // 
            this.delivery_to.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.delivery_to.DataPropertyName = "delivery_to";
            dataGridViewCellStyle8.BackColor = System.Drawing.Color.Gainsboro;
            this.delivery_to.DefaultCellStyle = dataGridViewCellStyle8;
            this.delivery_to.HeaderText = "DELIVERY TO";
            this.delivery_to.Name = "delivery_to";
            this.delivery_to.ReadOnly = true;
            // 
            // bill_to
            // 
            this.bill_to.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.bill_to.DataPropertyName = "bill_to";
            dataGridViewCellStyle9.BackColor = System.Drawing.Color.Gainsboro;
            this.bill_to.DefaultCellStyle = dataGridViewCellStyle9;
            this.bill_to.HeaderText = "BILL TO";
            this.bill_to.Name = "bill_to";
            this.bill_to.ReadOnly = true;
            // 
            // SalesOrderEngSearch
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.dgv_so_search);
            this.Controls.Add(this.txt_search);
            this.Name = "SalesOrderEngSearch";
            this.Text = "SalesOrderEngSearch";
            this.Load += new System.EventHandler(this.SalesOrderEngSearch_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_so_search)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dgv_so_search;
        private System.Windows.Forms.TextBox txt_search;
        private System.Windows.Forms.DataGridViewTextBoxColumn id;
        private System.Windows.Forms.DataGridViewTextBoxColumn customer;
        private System.Windows.Forms.DataGridViewTextBoxColumn code;
        private System.Windows.Forms.DataGridViewTextBoxColumn tin;
        private System.Windows.Forms.DataGridViewTextBoxColumn doc_no;
        private System.Windows.Forms.DataGridViewTextBoxColumn doc_date;
        private System.Windows.Forms.DataGridViewTextBoxColumn delivery_date;
        private System.Windows.Forms.DataGridViewTextBoxColumn delivery_to;
        private System.Windows.Forms.DataGridViewTextBoxColumn bill_to;
    }
}