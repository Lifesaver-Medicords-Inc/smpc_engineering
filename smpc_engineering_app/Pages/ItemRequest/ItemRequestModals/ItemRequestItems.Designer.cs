
namespace smpc_engineering_app.Pages.ItemRequest.ItemRequestModals
{
    partial class ItemRequestItems
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
            this.dgv_all_item = new System.Windows.Forms.DataGridView();
            this.txt_search = new System.Windows.Forms.TextBox();
            this.item_id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.short_desc = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.item_code = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.general_name = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.item_model = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.uom_name = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.size = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_all_item)).BeginInit();
            this.SuspendLayout();
            // 
            // dgv_all_item
            // 
            this.dgv_all_item.AllowUserToAddRows = false;
            this.dgv_all_item.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgv_all_item.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgv_all_item.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_all_item.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.item_id,
            this.short_desc,
            this.item_code,
            this.general_name,
            this.item_model,
            this.uom_name,
            this.size});
            this.dgv_all_item.Location = new System.Drawing.Point(-1, 31);
            this.dgv_all_item.Name = "dgv_all_item";
            this.dgv_all_item.Size = new System.Drawing.Size(802, 389);
            this.dgv_all_item.TabIndex = 4;
            this.dgv_all_item.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_all_item_CellClick);
            // 
            // txt_search
            // 
            this.txt_search.Location = new System.Drawing.Point(350, 215);
            this.txt_search.Name = "txt_search";
            this.txt_search.Size = new System.Drawing.Size(100, 20);
            this.txt_search.TabIndex = 5;
            this.txt_search.TextChanged += new System.EventHandler(this.txt_search_TextChanged);
            // 
            // item_id
            // 
            this.item_id.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.item_id.DataPropertyName = "item_id";
            this.item_id.HeaderText = "ID";
            this.item_id.Name = "item_id";
            this.item_id.ReadOnly = true;
            this.item_id.Visible = false;
            this.item_id.Width = 80;
            // 
            // short_desc
            // 
            this.short_desc.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.short_desc.DataPropertyName = "short_desc";
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.Gainsboro;
            this.short_desc.DefaultCellStyle = dataGridViewCellStyle2;
            this.short_desc.HeaderText = "DESCRIPTION";
            this.short_desc.Name = "short_desc";
            this.short_desc.ReadOnly = true;
            // 
            // item_code
            // 
            this.item_code.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.item_code.DataPropertyName = "item_code";
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.Gainsboro;
            this.item_code.DefaultCellStyle = dataGridViewCellStyle3;
            this.item_code.HeaderText = "ITEM CODE";
            this.item_code.Name = "item_code";
            this.item_code.ReadOnly = true;
            // 
            // general_name
            // 
            this.general_name.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.general_name.DataPropertyName = "general_name";
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.Gainsboro;
            this.general_name.DefaultCellStyle = dataGridViewCellStyle4;
            this.general_name.HeaderText = "GENERAL NAME";
            this.general_name.Name = "general_name";
            this.general_name.ReadOnly = true;
            // 
            // item_model
            // 
            this.item_model.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.item_model.DataPropertyName = "item_model";
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.Gainsboro;
            this.item_model.DefaultCellStyle = dataGridViewCellStyle5;
            this.item_model.HeaderText = "ITEM MODEL";
            this.item_model.MinimumWidth = 150;
            this.item_model.Name = "item_model";
            this.item_model.ReadOnly = true;
            // 
            // uom_name
            // 
            this.uom_name.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.uom_name.DataPropertyName = "uom_name";
            dataGridViewCellStyle6.BackColor = System.Drawing.Color.Gainsboro;
            this.uom_name.DefaultCellStyle = dataGridViewCellStyle6;
            this.uom_name.HeaderText = "UOM";
            this.uom_name.Name = "uom_name";
            this.uom_name.ReadOnly = true;
            // 
            // size
            // 
            this.size.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.size.DataPropertyName = "size";
            dataGridViewCellStyle7.BackColor = System.Drawing.Color.Gainsboro;
            this.size.DefaultCellStyle = dataGridViewCellStyle7;
            this.size.HeaderText = "SIZE";
            this.size.Name = "size";
            this.size.ReadOnly = true;
            // 
            // ItemRequestItems
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.dgv_all_item);
            this.Controls.Add(this.txt_search);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "ItemRequestItems";
            this.Text = "ItemRequestItems";
            this.Load += new System.EventHandler(this.ItemRequestItems_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_all_item)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dgv_all_item;
        private System.Windows.Forms.TextBox txt_search;
        private System.Windows.Forms.DataGridViewTextBoxColumn item_id;
        private System.Windows.Forms.DataGridViewTextBoxColumn short_desc;
        private System.Windows.Forms.DataGridViewTextBoxColumn item_code;
        private System.Windows.Forms.DataGridViewTextBoxColumn general_name;
        private System.Windows.Forms.DataGridViewTextBoxColumn item_model;
        private System.Windows.Forms.DataGridViewTextBoxColumn uom_name;
        private System.Windows.Forms.DataGridViewTextBoxColumn size;
    }
}