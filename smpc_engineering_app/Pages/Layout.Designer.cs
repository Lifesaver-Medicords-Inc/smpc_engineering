
namespace smpc_engineering_app
{
    partial class SMPC
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
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Job Order");
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("Sales Order");
            System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("Item Request");
            System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("Pick Activity");
            // Sales Quotation List was registered in RoutesService.cs (Phase 4 item 4.1)
            // but never actually given a sidebar node - engineers had no way to reach it
            // at all. Found and fixed alongside adding BOM/BOQ below.
            System.Windows.Forms.TreeNode treeNode5 = new System.Windows.Forms.TreeNode("Sales Quotation List");
            // BOM/BOQ (Phase 4 item 4.3-adjacent) - already fully built in the Inventory
            // app (Pages/Engineering/Bom, Pages/Engineering/Boq); reused here rather than
            // rebuilt, same "one implementation, two entry points" precedent as BPI.
            System.Windows.Forms.TreeNode treeNode6 = new System.Windows.Forms.TreeNode("BOM");
            System.Windows.Forms.TreeNode treeNode7 = new System.Windows.Forms.TreeNode("BOQ");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SMPC));
            this.container = new System.Windows.Forms.Panel();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.lbl_name = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel3 = new System.Windows.Forms.ToolStripStatusLabel();
            this.lbl_position = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel6 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel7 = new System.Windows.Forms.ToolStripStatusLabel();
            this.lbl_department = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel4 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel5 = new System.Windows.Forms.ToolStripStatusLabel();
            this.lbl_date_time = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel8 = new System.Windows.Forms.ToolStripStatusLabel();
            this.lbl_status = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.panel5 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.flowPanelJobOrderRedBox = new System.Windows.Forms.FlowLayoutPanel();
            this.flowPanelQuotationRedBox = new System.Windows.Forms.FlowLayoutPanel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.tabContainer = new System.Windows.Forms.TabControl();
            this.pnl_content_capped = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.Sidebar = new System.Windows.Forms.TreeView();
            this.statusStrip1.SuspendLayout();
            this.panel5.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel2.SuspendLayout();
            this.pnl_content_capped.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // container
            // 
            this.container.Dock = System.Windows.Forms.DockStyle.Fill;
            this.container.Location = new System.Drawing.Point(0, 0);
            this.container.Name = "container";
            this.container.Size = new System.Drawing.Size(1288, 637);
            this.container.TabIndex = 3;
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(33, 17);
            this.toolStripStatusLabel1.Text = "User:";
            // 
            // lbl_name
            // 
            this.lbl_name.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_name.Name = "lbl_name";
            this.lbl_name.Size = new System.Drawing.Size(0, 17);
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(55, 17);
            this.toolStripStatusLabel2.Text = "                ";
            // 
            // toolStripStatusLabel3
            // 
            this.toolStripStatusLabel3.Name = "toolStripStatusLabel3";
            this.toolStripStatusLabel3.Size = new System.Drawing.Size(53, 17);
            this.toolStripStatusLabel3.Text = "Position:";
            // 
            // lbl_position
            // 
            this.lbl_position.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_position.Name = "lbl_position";
            this.lbl_position.Size = new System.Drawing.Size(0, 17);
            // 
            // toolStripStatusLabel6
            // 
            this.toolStripStatusLabel6.Name = "toolStripStatusLabel6";
            this.toolStripStatusLabel6.Size = new System.Drawing.Size(55, 17);
            this.toolStripStatusLabel6.Text = "                ";
            // 
            // toolStripStatusLabel7
            // 
            this.toolStripStatusLabel7.Name = "toolStripStatusLabel7";
            this.toolStripStatusLabel7.Size = new System.Drawing.Size(73, 17);
            this.toolStripStatusLabel7.Text = "Department:";
            // 
            // lbl_department
            // 
            this.lbl_department.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_department.Name = "lbl_department";
            this.lbl_department.Size = new System.Drawing.Size(0, 17);
            // 
            // toolStripStatusLabel4
            // 
            this.toolStripStatusLabel4.Name = "toolStripStatusLabel4";
            this.toolStripStatusLabel4.Size = new System.Drawing.Size(55, 17);
            this.toolStripStatusLabel4.Text = "                ";
            // 
            // toolStripStatusLabel5
            // 
            this.toolStripStatusLabel5.Name = "toolStripStatusLabel5";
            this.toolStripStatusLabel5.Size = new System.Drawing.Size(84, 17);
            this.toolStripStatusLabel5.Text = "Date and time:";
            this.toolStripStatusLabel5.Visible = false;
            // 
            // lbl_date_time
            // 
            this.lbl_date_time.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_date_time.Name = "lbl_date_time";
            this.lbl_date_time.Size = new System.Drawing.Size(124, 17);
            this.lbl_date_time.Text = "Feb 7, 2025 11:02AM";
            this.lbl_date_time.Visible = false;
            // 
            // toolStripStatusLabel8
            // 
            this.toolStripStatusLabel8.Name = "toolStripStatusLabel8";
            this.toolStripStatusLabel8.Size = new System.Drawing.Size(42, 17);
            this.toolStripStatusLabel8.Text = "Status:";
            // 
            // lbl_status
            // 
            this.lbl_status.Name = "lbl_status";
            this.lbl_status.Size = new System.Drawing.Size(0, 17);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.lbl_name,
            this.toolStripStatusLabel2,
            this.toolStripStatusLabel3,
            this.lbl_position,
            this.toolStripStatusLabel6,
            this.toolStripStatusLabel7,
            this.lbl_department,
            this.toolStripStatusLabel4,
            this.toolStripStatusLabel5,
            this.lbl_date_time,
            this.toolStripStatusLabel8,
            this.lbl_status});
            this.statusStrip1.Location = new System.Drawing.Point(0, 615);
            this.statusStrip1.MaximumSize = new System.Drawing.Size(1273, 0);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1273, 22);
            this.statusStrip1.TabIndex = 7;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // panel5
            // 
            this.panel5.BackColor = System.Drawing.Color.LightCoral;
            this.panel5.Controls.Add(this.panel4);
            this.panel5.Controls.Add(this.panel2);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel5.Location = new System.Drawing.Point(965, 0);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(323, 615);
            this.panel5.TabIndex = 8;
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.LightCoral;
            this.panel4.Controls.Add(this.flowPanelJobOrderRedBox);
            this.panel4.Controls.Add(this.flowPanelQuotationRedBox);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(0, 58);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(323, 557);
            this.panel4.TabIndex = 15;
            this.panel4.Resize += new System.EventHandler(this.panel4_Resize);
            // 
            // flowPanelJobOrderRedBox
            // 
            this.flowPanelJobOrderRedBox.AutoScroll = true;
            this.flowPanelJobOrderRedBox.BackColor = System.Drawing.Color.LightCoral;
            this.flowPanelJobOrderRedBox.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flowPanelJobOrderRedBox.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowPanelJobOrderRedBox.Location = new System.Drawing.Point(0, 252);
            this.flowPanelJobOrderRedBox.Name = "flowPanelJobOrderRedBox";
            this.flowPanelJobOrderRedBox.Padding = new System.Windows.Forms.Padding(10);
            this.flowPanelJobOrderRedBox.Size = new System.Drawing.Size(323, 305);
            this.flowPanelJobOrderRedBox.TabIndex = 18;
            this.flowPanelJobOrderRedBox.WrapContents = false;
            // 
            // flowPanelQuotationRedBox
            // 
            this.flowPanelQuotationRedBox.AutoScroll = true;
            this.flowPanelQuotationRedBox.BackColor = System.Drawing.Color.LightCoral;
            this.flowPanelQuotationRedBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowPanelQuotationRedBox.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowPanelQuotationRedBox.Location = new System.Drawing.Point(0, 0);
            this.flowPanelQuotationRedBox.Name = "flowPanelQuotationRedBox";
            this.flowPanelQuotationRedBox.Padding = new System.Windows.Forms.Padding(10);
            this.flowPanelQuotationRedBox.Size = new System.Drawing.Size(323, 249);
            this.flowPanelQuotationRedBox.TabIndex = 15;
            this.flowPanelQuotationRedBox.WrapContents = false;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.LightCoral;
            this.panel2.Controls.Add(this.label2);
            this.panel2.Controls.Add(this.panel3);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(323, 58);
            this.panel2.TabIndex = 13;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(101, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(113, 25);
            this.label2.TabIndex = 4;
            this.label2.Text = "RED BOX";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.DarkRed;
            this.panel3.Location = new System.Drawing.Point(0, 53);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(313, 1);
            this.panel3.TabIndex = 3;
            //
            // tabContainer
            //
            // Phase 4.6 (UI uniformity): no longer Dock=Fill - pnl_content_capped now
            // owns that, and sizes/centers this manually (see Layout.cs's
            // RecalculateContentWidth) so the work area caps at 1280px on wide/ultrawide
            // monitors instead of stretching edge to edge, matching all 6 apps' new
            // standard.
            this.tabContainer.Location = new System.Drawing.Point(200, 0);
            this.tabContainer.Name = "tabContainer";
            this.tabContainer.SelectedIndex = 0;
            this.tabContainer.Size = new System.Drawing.Size(765, 615);
            this.tabContainer.TabIndex = 11;
            this.tabContainer.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.tabContainer_DrawItem);
            this.tabContainer.MouseDown += new System.Windows.Forms.MouseEventHandler(this.tabContainer_MouseDown);
            //
            // pnl_content_capped
            //
            this.pnl_content_capped.Controls.Add(this.tabContainer);
            this.pnl_content_capped.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnl_content_capped.AutoScroll = true;
            this.pnl_content_capped.Location = new System.Drawing.Point(200, 0);
            this.pnl_content_capped.Name = "pnl_content_capped";
            this.pnl_content_capped.Size = new System.Drawing.Size(765, 615);
            this.pnl_content_capped.TabIndex = 12;
            this.pnl_content_capped.Resize += new System.EventHandler(this.pnl_content_capped_Resize);
            //
            // panel1
            // 
            this.panel1.Controls.Add(this.Sidebar);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(200, 615);
            this.panel1.TabIndex = 10;
            // 
            // Sidebar
            // 
            this.Sidebar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Sidebar.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Sidebar.Location = new System.Drawing.Point(0, 0);
            this.Sidebar.Name = "Sidebar";
            treeNode1.Name = "Job Order";
            treeNode1.Text = "Job Order";
            treeNode2.Name = "Sales Order";
            treeNode2.Text = "Sales Order";
            treeNode3.Name = "Item Request";
            treeNode3.Text = "Item Request";
            treeNode4.Name = "Pick Activity";
            treeNode4.Text = "Pick Activity";
            treeNode5.Name = "Sales Quotation List";
            treeNode5.Text = "Sales Quotation List";
            treeNode6.Name = "BOM";
            treeNode6.Text = "BOM";
            treeNode7.Name = "BOQ";
            treeNode7.Text = "BOQ";
            this.Sidebar.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode2,
            treeNode3,
            treeNode4,
            treeNode5,
            treeNode6,
            treeNode7});
            this.Sidebar.Size = new System.Drawing.Size(200, 615);
            this.Sidebar.TabIndex = 0;
            this.Sidebar.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.Sidebar_NodeMouseClick);
            // 
            // SMPC
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1288, 637);
            this.Controls.Add(this.pnl_content_capped);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel5);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.container);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SMPC";
            this.Text = "SMPC - ENGINEERING";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.SMPC_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.panel5.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.pnl_content_capped.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel container;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel lbl_name;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel3;
        private System.Windows.Forms.ToolStripStatusLabel lbl_position;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel6;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel7;
        private System.Windows.Forms.ToolStripStatusLabel lbl_department;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel4;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel5;
        private System.Windows.Forms.ToolStripStatusLabel lbl_date_time;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel8;
        private System.Windows.Forms.ToolStripStatusLabel lbl_status;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.FlowLayoutPanel flowPanelJobOrderRedBox;
        private System.Windows.Forms.FlowLayoutPanel flowPanelQuotationRedBox;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panel3;
        public System.Windows.Forms.TabControl tabContainer;
        private System.Windows.Forms.Panel pnl_content_capped;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TreeView Sidebar;
    }
}

