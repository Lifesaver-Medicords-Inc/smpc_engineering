namespace smpc_engineering_app.Pages.SalesQuotationEngineering
{
    partial class SalesQuotationEngPage
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

        #region Component Designer generated code

        private void InitializeComponent()
        {
            this.panel_header = new System.Windows.Forms.Panel();
            this.btn_save = new System.Windows.Forms.Button();
            this.btn_refresh = new System.Windows.Forms.Button();
            this.lbl_title = new System.Windows.Forms.Label();
            this.lbl_status_caption = new System.Windows.Forms.Label();
            this.txt_status = new System.Windows.Forms.TextBox();
            this.lbl_sales_executive_caption = new System.Windows.Forms.Label();
            this.txt_sales_executive = new System.Windows.Forms.TextBox();
            this.lbl_project_name_caption = new System.Windows.Forms.Label();
            this.txt_project_name = new System.Windows.Forms.TextBox();
            this.lbl_doc_no_caption = new System.Windows.Forms.Label();
            this.txt_doc_no = new System.Windows.Forms.TextBox();
            this.lbl_client_caption = new System.Windows.Forms.Label();
            this.txt_client = new System.Windows.Forms.TextBox();
            this.lbl_saving_status = new System.Windows.Forms.Label();
            this.pnl_multiplier_history = new System.Windows.Forms.Panel();
            this.grp_multiplier = new System.Windows.Forms.GroupBox();
            this.grp_history = new System.Windows.Forms.GroupBox();
            this.tab_container = new System.Windows.Forms.TabControl();
            this.panel_header.SuspendLayout();
            this.pnl_multiplier_history.SuspendLayout();
            this.SuspendLayout();
            //
            // panel_header
            //
            this.panel_header.Controls.Add(this.lbl_saving_status);
            this.panel_header.Controls.Add(this.btn_refresh);
            this.panel_header.Controls.Add(this.btn_save);
            this.panel_header.Controls.Add(this.lbl_title);
            this.panel_header.Controls.Add(this.lbl_status_caption);
            this.panel_header.Controls.Add(this.txt_status);
            this.panel_header.Controls.Add(this.lbl_sales_executive_caption);
            this.panel_header.Controls.Add(this.txt_sales_executive);
            this.panel_header.Controls.Add(this.lbl_project_name_caption);
            this.panel_header.Controls.Add(this.txt_project_name);
            this.panel_header.Controls.Add(this.lbl_doc_no_caption);
            this.panel_header.Controls.Add(this.txt_doc_no);
            this.panel_header.Controls.Add(this.lbl_client_caption);
            this.panel_header.Controls.Add(this.txt_client);
            this.panel_header.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel_header.Location = new System.Drawing.Point(0, 0);
            this.panel_header.Name = "panel_header";
            this.panel_header.Size = new System.Drawing.Size(1400, 120);
            this.panel_header.TabIndex = 0;
            //
            // lbl_title
            //
            this.lbl_title.AutoSize = true;
            this.lbl_title.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Bold);
            this.lbl_title.Location = new System.Drawing.Point(14, 10);
            this.lbl_title.Name = "lbl_title";
            this.lbl_title.Size = new System.Drawing.Size(300, 21);
            this.lbl_title.TabIndex = 0;
            this.lbl_title.Text = "SALES QUOTATION - ENGINEERING";
            //
            // btn_save
            //
            this.btn_save.Location = new System.Drawing.Point(1170, 12);
            this.btn_save.Name = "btn_save";
            this.btn_save.Size = new System.Drawing.Size(100, 27);
            this.btn_save.TabIndex = 1;
            this.btn_save.Text = "Save";
            this.btn_save.UseVisualStyleBackColor = true;
            this.btn_save.Click += new System.EventHandler(this.btn_save_Click);
            //
            // btn_refresh
            //
            this.btn_refresh.Location = new System.Drawing.Point(1276, 12);
            this.btn_refresh.Name = "btn_refresh";
            this.btn_refresh.Size = new System.Drawing.Size(100, 27);
            this.btn_refresh.TabIndex = 2;
            this.btn_refresh.Text = "Refresh";
            this.btn_refresh.UseVisualStyleBackColor = true;
            this.btn_refresh.Click += new System.EventHandler(this.btn_refresh_Click);
            //
            // lbl_client_caption
            //
            this.lbl_client_caption.AutoSize = true;
            this.lbl_client_caption.Location = new System.Drawing.Point(16, 48);
            this.lbl_client_caption.Name = "lbl_client_caption";
            this.lbl_client_caption.Size = new System.Drawing.Size(37, 13);
            this.lbl_client_caption.TabIndex = 3;
            this.lbl_client_caption.Text = "CLIENT";
            //
            // txt_client
            //
            this.txt_client.Enabled = false;
            this.txt_client.Location = new System.Drawing.Point(19, 64);
            this.txt_client.Name = "txt_client";
            this.txt_client.Size = new System.Drawing.Size(220, 20);
            this.txt_client.TabIndex = 4;
            //
            // lbl_doc_no_caption
            //
            this.lbl_doc_no_caption.AutoSize = true;
            this.lbl_doc_no_caption.Location = new System.Drawing.Point(255, 48);
            this.lbl_doc_no_caption.Name = "lbl_doc_no_caption";
            this.lbl_doc_no_caption.Size = new System.Drawing.Size(46, 13);
            this.lbl_doc_no_caption.TabIndex = 5;
            this.lbl_doc_no_caption.Text = "DOC NO.";
            //
            // txt_doc_no
            //
            this.txt_doc_no.Enabled = false;
            this.txt_doc_no.Location = new System.Drawing.Point(258, 64);
            this.txt_doc_no.Name = "txt_doc_no";
            this.txt_doc_no.Size = new System.Drawing.Size(140, 20);
            this.txt_doc_no.TabIndex = 6;
            //
            // lbl_project_name_caption
            //
            this.lbl_project_name_caption.AutoSize = true;
            this.lbl_project_name_caption.Location = new System.Drawing.Point(414, 48);
            this.lbl_project_name_caption.Name = "lbl_project_name_caption";
            this.lbl_project_name_caption.Size = new System.Drawing.Size(75, 13);
            this.lbl_project_name_caption.TabIndex = 7;
            this.lbl_project_name_caption.Text = "PROJECT NAME";
            //
            // txt_project_name
            //
            this.txt_project_name.Enabled = false;
            this.txt_project_name.Location = new System.Drawing.Point(417, 64);
            this.txt_project_name.Name = "txt_project_name";
            this.txt_project_name.Size = new System.Drawing.Size(300, 20);
            this.txt_project_name.TabIndex = 8;
            //
            // lbl_sales_executive_caption
            //
            this.lbl_sales_executive_caption.AutoSize = true;
            this.lbl_sales_executive_caption.Location = new System.Drawing.Point(735, 48);
            this.lbl_sales_executive_caption.Name = "lbl_sales_executive_caption";
            this.lbl_sales_executive_caption.Size = new System.Drawing.Size(85, 13);
            this.lbl_sales_executive_caption.TabIndex = 9;
            this.lbl_sales_executive_caption.Text = "SALES EXECUTIVE";
            //
            // txt_sales_executive
            //
            this.txt_sales_executive.Enabled = false;
            this.txt_sales_executive.Location = new System.Drawing.Point(738, 64);
            this.txt_sales_executive.Name = "txt_sales_executive";
            this.txt_sales_executive.Size = new System.Drawing.Size(180, 20);
            this.txt_sales_executive.TabIndex = 10;
            //
            // lbl_status_caption
            //
            this.lbl_status_caption.AutoSize = true;
            this.lbl_status_caption.Location = new System.Drawing.Point(933, 48);
            this.lbl_status_caption.Name = "lbl_status_caption";
            this.lbl_status_caption.Size = new System.Drawing.Size(39, 13);
            this.lbl_status_caption.TabIndex = 11;
            this.lbl_status_caption.Text = "STATUS";
            //
            // txt_status
            //
            this.txt_status.Enabled = false;
            this.txt_status.Location = new System.Drawing.Point(936, 64);
            this.txt_status.Name = "txt_status";
            this.txt_status.Size = new System.Drawing.Size(180, 20);
            this.txt_status.TabIndex = 12;
            //
            // lbl_saving_status
            //
            this.lbl_saving_status.AutoSize = true;
            this.lbl_saving_status.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic);
            this.lbl_saving_status.ForeColor = System.Drawing.Color.Gray;
            this.lbl_saving_status.Location = new System.Drawing.Point(1170, 45);
            this.lbl_saving_status.Name = "lbl_saving_status";
            this.lbl_saving_status.Size = new System.Drawing.Size(0, 13);
            this.lbl_saving_status.TabIndex = 13;
            //
            // grp_multiplier
            //
            this.grp_multiplier.Dock = System.Windows.Forms.DockStyle.Left;
            this.grp_multiplier.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grp_multiplier.Location = new System.Drawing.Point(0, 0);
            this.grp_multiplier.Name = "grp_multiplier";
            this.grp_multiplier.Size = new System.Drawing.Size(690, 210);
            this.grp_multiplier.TabIndex = 0;
            this.grp_multiplier.TabStop = false;
            this.grp_multiplier.Text = "MULTIPLIER";
            //
            // grp_history
            //
            this.grp_history.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grp_history.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grp_history.Location = new System.Drawing.Point(690, 0);
            this.grp_history.Name = "grp_history";
            this.grp_history.Size = new System.Drawing.Size(710, 210);
            this.grp_history.TabIndex = 1;
            this.grp_history.TabStop = false;
            this.grp_history.Text = "CHANGE HISTORY";
            //
            // pnl_multiplier_history
            //
            // Sits above tab_container, same idea as Sales's Quotation.cs which keeps
            // dgv_project_multiplier at the quotation level (a sibling of tabControl2),
            // always visible regardless of which item-set tab is active - not one more
            // tab to click into. Per user decision, 2026-09-03.
            this.pnl_multiplier_history.Controls.Add(this.grp_history);
            this.pnl_multiplier_history.Controls.Add(this.grp_multiplier);
            this.pnl_multiplier_history.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnl_multiplier_history.Location = new System.Drawing.Point(0, 120);
            this.pnl_multiplier_history.Name = "pnl_multiplier_history";
            this.pnl_multiplier_history.Size = new System.Drawing.Size(1400, 210);
            this.pnl_multiplier_history.TabIndex = 2;
            //
            // tab_container
            //
            this.tab_container.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tab_container.Location = new System.Drawing.Point(0, 330);
            this.tab_container.Name = "tab_container";
            this.tab_container.SelectedIndex = 0;
            this.tab_container.Size = new System.Drawing.Size(1400, 620);
            this.tab_container.TabIndex = 1;
            //
            // SalesQuotationEngPage
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tab_container);
            this.Controls.Add(this.pnl_multiplier_history);
            this.Controls.Add(this.panel_header);
            this.Name = "SalesQuotationEngPage";
            this.Size = new System.Drawing.Size(1400, 950);
            this.Load += new System.EventHandler(this.SalesQuotationEngPage_Load);
            this.panel_header.ResumeLayout(false);
            this.panel_header.PerformLayout();
            this.pnl_multiplier_history.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel_header;
        private System.Windows.Forms.Label lbl_title;
        private System.Windows.Forms.Button btn_save;
        private System.Windows.Forms.Button btn_refresh;
        private System.Windows.Forms.Label lbl_client_caption;
        private System.Windows.Forms.TextBox txt_client;
        private System.Windows.Forms.Label lbl_doc_no_caption;
        private System.Windows.Forms.TextBox txt_doc_no;
        private System.Windows.Forms.Label lbl_project_name_caption;
        private System.Windows.Forms.TextBox txt_project_name;
        private System.Windows.Forms.Label lbl_sales_executive_caption;
        private System.Windows.Forms.TextBox txt_sales_executive;
        private System.Windows.Forms.Label lbl_status_caption;
        private System.Windows.Forms.TextBox txt_status;
        private System.Windows.Forms.Label lbl_saving_status;
        private System.Windows.Forms.Panel pnl_multiplier_history;
        private System.Windows.Forms.GroupBox grp_multiplier;
        private System.Windows.Forms.GroupBox grp_history;
        private System.Windows.Forms.TabControl tab_container;
    }
}
