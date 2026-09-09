namespace smpc_engineering_app.Pages.JobOrder
{
    partial class SerialNumberEntryModal
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.lbl_hint = new System.Windows.Forms.Label();
            this.dg_serials = new System.Windows.Forms.DataGridView();
            this.col_unit = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_serial = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnl_buttons = new System.Windows.Forms.Panel();
            this.btn_cancel = new System.Windows.Forms.Button();
            this.btn_ok = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dg_serials)).BeginInit();
            this.pnl_buttons.SuspendLayout();
            this.SuspendLayout();
            //
            // lbl_hint
            //
            this.lbl_hint.Dock = System.Windows.Forms.DockStyle.Top;
            this.lbl_hint.Location = new System.Drawing.Point(0, 0);
            this.lbl_hint.Name = "lbl_hint";
            this.lbl_hint.Padding = new System.Windows.Forms.Padding(12, 12, 12, 6);
            this.lbl_hint.Size = new System.Drawing.Size(444, 46);
            this.lbl_hint.TabIndex = 0;
            this.lbl_hint.Text = "One serial number per unit.";
            //
            // dg_serials
            //
            this.dg_serials.AllowUserToAddRows = false;
            this.dg_serials.AllowUserToDeleteRows = false;
            this.dg_serials.AllowUserToResizeRows = false;
            this.dg_serials.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dg_serials.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dg_serials.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.col_unit,
            this.col_serial});
            this.dg_serials.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dg_serials.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dg_serials.Location = new System.Drawing.Point(0, 46);
            this.dg_serials.Name = "dg_serials";
            this.dg_serials.RowHeadersVisible = false;
            this.dg_serials.Size = new System.Drawing.Size(444, 279);
            this.dg_serials.TabIndex = 1;
            //
            // col_unit
            //
            this.col_unit.FillWeight = 60F;
            this.col_unit.HeaderText = "UNIT";
            this.col_unit.Name = "col_unit";
            this.col_unit.ReadOnly = true;
            this.col_unit.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            //
            // col_serial
            //
            this.col_serial.FillWeight = 160F;
            this.col_serial.HeaderText = "SERIAL NUMBER";
            this.col_serial.MaxInputLength = 100;
            this.col_serial.Name = "col_serial";
            this.col_serial.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            //
            // pnl_buttons
            //
            this.pnl_buttons.Controls.Add(this.btn_cancel);
            this.pnl_buttons.Controls.Add(this.btn_ok);
            this.pnl_buttons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnl_buttons.Location = new System.Drawing.Point(0, 325);
            this.pnl_buttons.Name = "pnl_buttons";
            this.pnl_buttons.Size = new System.Drawing.Size(444, 48);
            this.pnl_buttons.TabIndex = 2;
            //
            // btn_cancel
            //
            this.btn_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_cancel.Location = new System.Drawing.Point(345, 9);
            this.btn_cancel.Name = "btn_cancel";
            this.btn_cancel.Size = new System.Drawing.Size(85, 28);
            this.btn_cancel.TabIndex = 1;
            this.btn_cancel.Text = "CANCEL";
            this.btn_cancel.UseVisualStyleBackColor = true;
            //
            // btn_ok
            //
            this.btn_ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_ok.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.btn_ok.Location = new System.Drawing.Point(254, 9);
            this.btn_ok.Name = "btn_ok";
            this.btn_ok.Size = new System.Drawing.Size(85, 28);
            this.btn_ok.TabIndex = 0;
            this.btn_ok.Text = "OK";
            this.btn_ok.UseVisualStyleBackColor = false;
            //
            // SerialNumberEntryModal
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(444, 373);
            this.Controls.Add(this.dg_serials);
            this.Controls.Add(this.pnl_buttons);
            this.Controls.Add(this.lbl_hint);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(380, 300);
            this.Name = "SerialNumberEntryModal";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Serial Numbers";
            ((System.ComponentModel.ISupportInitialize)(this.dg_serials)).EndInit();
            this.pnl_buttons.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label lbl_hint;
        private System.Windows.Forms.DataGridView dg_serials;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_unit;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_serial;
        private System.Windows.Forms.Panel pnl_buttons;
        private System.Windows.Forms.Button btn_ok;
        private System.Windows.Forms.Button btn_cancel;
    }
}
