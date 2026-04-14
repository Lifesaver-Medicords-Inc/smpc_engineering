
namespace smpc_engineering_app.Pages.PickActivity.PickActivityModals
{
    partial class PickActivitySearch
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
            this.dgv_pa_search = new System.Windows.Forms.DataGridView();
            this.txt_search = new System.Windows.Forms.TextBox();
            this.id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.doc_no = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sales_person = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.customer = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.code = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.prepared_by = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.picked_by = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_pa_search)).BeginInit();
            this.SuspendLayout();
            // 
            // dgv_pa_search
            // 
            this.dgv_pa_search.AllowUserToAddRows = false;
            this.dgv_pa_search.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgv_pa_search.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgv_pa_search.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_pa_search.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.id,
            this.doc_no,
            this.sales_person,
            this.customer,
            this.code,
            this.prepared_by,
            this.picked_by});
            this.dgv_pa_search.Location = new System.Drawing.Point(-1, 31);
            this.dgv_pa_search.Name = "dgv_pa_search";
            this.dgv_pa_search.Size = new System.Drawing.Size(802, 389);
            this.dgv_pa_search.TabIndex = 4;
            this.dgv_pa_search.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_pa_search_CellClick);
            // 
            // txt_search
            // 
            this.txt_search.Location = new System.Drawing.Point(350, 215);
            this.txt_search.Name = "txt_search";
            this.txt_search.Size = new System.Drawing.Size(100, 20);
            this.txt_search.TabIndex = 5;
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
            // doc_no
            // 
            this.doc_no.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.doc_no.DataPropertyName = "doc_no";
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.Gainsboro;
            this.doc_no.DefaultCellStyle = dataGridViewCellStyle2;
            this.doc_no.HeaderText = "DOC NO";
            this.doc_no.Name = "doc_no";
            this.doc_no.ReadOnly = true;
            // 
            // sales_person
            // 
            this.sales_person.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.sales_person.DataPropertyName = "sales_person";
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.Gainsboro;
            this.sales_person.DefaultCellStyle = dataGridViewCellStyle3;
            this.sales_person.HeaderText = "SALES PERSON";
            this.sales_person.Name = "sales_person";
            this.sales_person.ReadOnly = true;
            // 
            // customer
            // 
            this.customer.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.customer.DataPropertyName = "customer";
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.Gainsboro;
            this.customer.DefaultCellStyle = dataGridViewCellStyle4;
            this.customer.HeaderText = "CUSTOMER";
            this.customer.Name = "customer";
            this.customer.ReadOnly = true;
            // 
            // code
            // 
            this.code.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.code.DataPropertyName = "code";
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.Gainsboro;
            this.code.DefaultCellStyle = dataGridViewCellStyle5;
            this.code.HeaderText = "CODE";
            this.code.MinimumWidth = 150;
            this.code.Name = "code";
            this.code.ReadOnly = true;
            // 
            // prepared_by
            // 
            this.prepared_by.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.prepared_by.DataPropertyName = "prepared_by";
            dataGridViewCellStyle6.BackColor = System.Drawing.Color.Gainsboro;
            this.prepared_by.DefaultCellStyle = dataGridViewCellStyle6;
            this.prepared_by.HeaderText = "PREPARED BY";
            this.prepared_by.Name = "prepared_by";
            this.prepared_by.ReadOnly = true;
            // 
            // picked_by
            // 
            this.picked_by.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.picked_by.DataPropertyName = "picked_by";
            dataGridViewCellStyle7.BackColor = System.Drawing.Color.Gainsboro;
            this.picked_by.DefaultCellStyle = dataGridViewCellStyle7;
            this.picked_by.HeaderText = "PICKED BY";
            this.picked_by.Name = "picked_by";
            this.picked_by.ReadOnly = true;
            // 
            // PickActivitySearch
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.dgv_pa_search);
            this.Controls.Add(this.txt_search);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "PickActivitySearch";
            this.Text = "PickActivitySearch";
            this.Load += new System.EventHandler(this.PickActivitySearch_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_pa_search)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dgv_pa_search;
        private System.Windows.Forms.TextBox txt_search;
        private System.Windows.Forms.DataGridViewTextBoxColumn id;
        private System.Windows.Forms.DataGridViewTextBoxColumn doc_no;
        private System.Windows.Forms.DataGridViewTextBoxColumn sales_person;
        private System.Windows.Forms.DataGridViewTextBoxColumn customer;
        private System.Windows.Forms.DataGridViewTextBoxColumn code;
        private System.Windows.Forms.DataGridViewTextBoxColumn prepared_by;
        private System.Windows.Forms.DataGridViewTextBoxColumn picked_by;
    }
}