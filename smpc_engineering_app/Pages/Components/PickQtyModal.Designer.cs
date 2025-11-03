
namespace smpc_engineering_app.Pages.Components
{
    partial class PickQtyModal
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
            this.dgv_pick_qty = new System.Windows.Forms.DataGridView();
            this.btn_save = new System.Windows.Forms.Button();
            this.id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ir_id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ir_details_id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.warehouse_id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.issued_qty = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.issued_uom = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.location = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_pick_qty)).BeginInit();
            this.SuspendLayout();
            // 
            // dgv_pick_qty
            // 
            this.dgv_pick_qty.AllowUserToAddRows = false;
            this.dgv_pick_qty.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgv_pick_qty.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgv_pick_qty.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_pick_qty.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.id,
            this.ir_id,
            this.ir_details_id,
            this.warehouse_id,
            this.issued_qty,
            this.issued_uom,
            this.location});
            this.dgv_pick_qty.Location = new System.Drawing.Point(-1, 24);
            this.dgv_pick_qty.Name = "dgv_pick_qty";
            this.dgv_pick_qty.Size = new System.Drawing.Size(802, 326);
            this.dgv_pick_qty.TabIndex = 4;
            this.dgv_pick_qty.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(this.dgv_pick_qty_EditingControlShowing);
            // 
            // btn_save
            // 
            this.btn_save.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_save.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(234)))), ((int)(((byte)(211)))));
            this.btn_save.Location = new System.Drawing.Point(677, 415);
            this.btn_save.Name = "btn_save";
            this.btn_save.Size = new System.Drawing.Size(111, 23);
            this.btn_save.TabIndex = 37;
            this.btn_save.Text = "SAVE";
            this.btn_save.UseVisualStyleBackColor = false;
            this.btn_save.Click += new System.EventHandler(this.btn_save_Click);
            // 
            // id
            // 
            this.id.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.id.DataPropertyName = "id";
            this.id.HeaderText = "ID";
            this.id.Name = "id";
            this.id.ReadOnly = true;
            this.id.Visible = false;
            // 
            // ir_id
            // 
            this.ir_id.DataPropertyName = "ir_id";
            this.ir_id.HeaderText = "IR ID";
            this.ir_id.Name = "ir_id";
            this.ir_id.ReadOnly = true;
            this.ir_id.Visible = false;
            // 
            // ir_details_id
            // 
            this.ir_details_id.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.ir_details_id.DataPropertyName = "ir_details_id";
            this.ir_details_id.HeaderText = "IRD ID";
            this.ir_details_id.Name = "ir_details_id";
            this.ir_details_id.ReadOnly = true;
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
            // issued_qty
            // 
            this.issued_qty.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.issued_qty.DataPropertyName = "issued_qty";
            this.issued_qty.HeaderText = "QTY";
            this.issued_qty.Name = "issued_qty";
            // 
            // issued_uom
            // 
            this.issued_uom.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.issued_uom.DataPropertyName = "issued_uom";
            this.issued_uom.HeaderText = "UOM";
            this.issued_uom.Name = "issued_uom";
            this.issued_uom.ReadOnly = true;
            // 
            // location
            // 
            this.location.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.location.DataPropertyName = "location";
            this.location.HeaderText = "BIN LOCATION";
            this.location.Name = "location";
            this.location.ReadOnly = true;
            // 
            // PickQtyModal
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btn_save);
            this.Controls.Add(this.dgv_pick_qty);
            this.Name = "PickQtyModal";
            this.Text = "PickQtyModal";
            this.Load += new System.EventHandler(this.PickQtyModal_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_pick_qty)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgv_pick_qty;
        private System.Windows.Forms.Button btn_save;
        private System.Windows.Forms.DataGridViewTextBoxColumn id;
        private System.Windows.Forms.DataGridViewTextBoxColumn ir_id;
        private System.Windows.Forms.DataGridViewTextBoxColumn ir_details_id;
        private System.Windows.Forms.DataGridViewTextBoxColumn warehouse_id;
        private System.Windows.Forms.DataGridViewTextBoxColumn issued_qty;
        private System.Windows.Forms.DataGridViewTextBoxColumn issued_uom;
        private System.Windows.Forms.DataGridViewTextBoxColumn location;
    }
}