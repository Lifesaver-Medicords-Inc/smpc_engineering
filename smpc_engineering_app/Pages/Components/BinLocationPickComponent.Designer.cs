
namespace smpc_engineering_app.Pages.Components
{
    partial class BinLocationPickComponent
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
            this.dgv_main = new System.Windows.Forms.DataGridView();
            this.btn_save = new System.Windows.Forms.Button();
            this.bin_id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.warehouse_id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.item_id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.IsSelected = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.selected_qty = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.selected_uom = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.stock_qty = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.stock_uom = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.bin_location = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_main)).BeginInit();
            this.SuspendLayout();
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
            this.bin_id,
            this.warehouse_id,
            this.item_id,
            this.IsSelected,
            this.selected_qty,
            this.selected_uom,
            this.stock_qty,
            this.stock_uom,
            this.bin_location});
            this.dgv_main.EnableHeadersVisualStyles = false;
            this.dgv_main.Location = new System.Drawing.Point(0, 27);
            this.dgv_main.Name = "dgv_main";
            this.dgv_main.Size = new System.Drawing.Size(802, 326);
            this.dgv_main.TabIndex = 5;
            this.dgv_main.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_main_CellValueChanged);
            this.dgv_main.CurrentCellDirtyStateChanged += new System.EventHandler(this.dgv_main_CurrentCellDirtyStateChanged);
            this.dgv_main.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(this.dgv_main_EditingControlShowing);
            // 
            // btn_save
            // 
            this.btn_save.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_save.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(234)))), ((int)(((byte)(211)))));
            this.btn_save.Location = new System.Drawing.Point(668, 404);
            this.btn_save.Name = "btn_save";
            this.btn_save.Size = new System.Drawing.Size(111, 23);
            this.btn_save.TabIndex = 38;
            this.btn_save.Text = "SAVE";
            this.btn_save.UseVisualStyleBackColor = false;
            this.btn_save.Click += new System.EventHandler(this.btn_save_Click);
            // 
            // bin_id
            // 
            this.bin_id.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.bin_id.DataPropertyName = "bin_id";
            this.bin_id.HeaderText = "ID";
            this.bin_id.Name = "bin_id";
            this.bin_id.ReadOnly = true;
            this.bin_id.Visible = false;
            // 
            // warehouse_id
            // 
            this.warehouse_id.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.warehouse_id.DataPropertyName = "warehouse_id";
            this.warehouse_id.HeaderText = "WAREHOUSE ID";
            this.warehouse_id.Name = "warehouse_id";
            this.warehouse_id.ReadOnly = true;
            this.warehouse_id.Visible = false;
            // 
            // item_id
            // 
            this.item_id.DataPropertyName = "item_id";
            this.item_id.HeaderText = "ITEM ID";
            this.item_id.Name = "item_id";
            this.item_id.ReadOnly = true;
            this.item_id.Visible = false;
            // 
            // IsSelected
            // 
            this.IsSelected.HeaderText = "SELECT";
            this.IsSelected.Name = "IsSelected";
            this.IsSelected.Width = 80;
            // 
            // selected_qty
            // 
            this.selected_qty.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.selected_qty.DataPropertyName = "selected_qty";
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.Gainsboro;
            this.selected_qty.DefaultCellStyle = dataGridViewCellStyle2;
            this.selected_qty.HeaderText = "QTY";
            this.selected_qty.Name = "selected_qty";
            this.selected_qty.ReadOnly = true;
            this.selected_qty.Width = 80;
            // 
            // selected_uom
            // 
            this.selected_uom.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.selected_uom.DataPropertyName = "stock_uom";
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.Gainsboro;
            this.selected_uom.DefaultCellStyle = dataGridViewCellStyle3;
            this.selected_uom.HeaderText = "UOM";
            this.selected_uom.Name = "selected_uom";
            this.selected_uom.ReadOnly = true;
            this.selected_uom.Width = 80;
            // 
            // stock_qty
            // 
            this.stock_qty.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.stock_qty.DataPropertyName = "stock_qty";
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.Gainsboro;
            this.stock_qty.DefaultCellStyle = dataGridViewCellStyle4;
            this.stock_qty.HeaderText = "QTY";
            this.stock_qty.Name = "stock_qty";
            this.stock_qty.ReadOnly = true;
            this.stock_qty.Width = 80;
            // 
            // stock_uom
            // 
            this.stock_uom.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.stock_uom.DataPropertyName = "stock_uom";
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.Gainsboro;
            this.stock_uom.DefaultCellStyle = dataGridViewCellStyle5;
            this.stock_uom.HeaderText = "UOM";
            this.stock_uom.Name = "stock_uom";
            this.stock_uom.ReadOnly = true;
            this.stock_uom.Width = 80;
            // 
            // bin_location
            // 
            this.bin_location.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.bin_location.DataPropertyName = "bin_location";
            dataGridViewCellStyle6.BackColor = System.Drawing.Color.Gainsboro;
            this.bin_location.DefaultCellStyle = dataGridViewCellStyle6;
            this.bin_location.HeaderText = "BIN LOCATION";
            this.bin_location.Name = "bin_location";
            this.bin_location.ReadOnly = true;
            // 
            // BinLocationPickComponent
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btn_save);
            this.Controls.Add(this.dgv_main);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "BinLocationPickComponent";
            this.Text = "BinLocationPickComponent";
            this.Load += new System.EventHandler(this.BinLocationPickComponent_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_main)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgv_main;
        private System.Windows.Forms.Button btn_save;
        private System.Windows.Forms.DataGridViewTextBoxColumn bin_id;
        private System.Windows.Forms.DataGridViewTextBoxColumn warehouse_id;
        private System.Windows.Forms.DataGridViewTextBoxColumn item_id;
        private System.Windows.Forms.DataGridViewCheckBoxColumn IsSelected;
        private System.Windows.Forms.DataGridViewTextBoxColumn selected_qty;
        private System.Windows.Forms.DataGridViewTextBoxColumn selected_uom;
        private System.Windows.Forms.DataGridViewTextBoxColumn stock_qty;
        private System.Windows.Forms.DataGridViewTextBoxColumn stock_uom;
        private System.Windows.Forms.DataGridViewTextBoxColumn bin_location;
    }
}