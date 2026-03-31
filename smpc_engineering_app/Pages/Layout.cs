using smpc_engineering_app.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using smpc_engineering_app.Pages;
using smpc_engineering_app.Shared;
using System.Windows.Forms;
using smpc_engineering_app.Models;
using smpc_engineering_app.Services.Helpers;
using smpc_engineering_app.Pages.Components;

namespace smpc_engineering_app
{
    public partial class SMPC : Form
    {
        private int tabCount = 0;

        private WebSocketService productionListSocket;
        private WebSocketService quotationSocket;

        public SMPC()
        {
            InitializeComponent();

            tabContainer.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabContainer.SizeMode = TabSizeMode.Fixed;
            tabContainer.ItemSize = new Size(150, 20);

            flowPanelQuotationRedBox.Padding = new Padding(8);
            flowPanelQuotationRedBox.Margin = new Padding(0);

            flowPanelJobOrderRedBox.Padding = new Padding(8);
            flowPanelJobOrderRedBox.Margin = new Padding(0);

            // Hook events for Quotation
            flowPanelQuotationRedBox.SizeChanged += (s, e) => ResizeFlowChildren(flowPanelQuotationRedBox);
            flowPanelQuotationRedBox.ControlAdded += (s, e) => SizeChild(flowPanelQuotationRedBox, e.Control);

            // Hook events for JobOrder
            flowPanelJobOrderRedBox.SizeChanged += (s, e) => ResizeFlowChildren(flowPanelJobOrderRedBox);
            flowPanelJobOrderRedBox.ControlAdded += (s, e) => SizeChild(flowPanelJobOrderRedBox, e.Control);
        }

        void ResizeFlowChildren(FlowLayoutPanel flowPanel)
        {
            foreach (Control c in flowPanel.Controls)
                SizeChild(flowPanel, c);
        }

        void SizeChild(FlowLayoutPanel flowPanel, Control c)
        {
            // Width that fills the flow panel, respecting padding & the child’s margin
            int w = flowPanel.ClientSize.Width
                    - flowPanel.Padding.Horizontal
                    - c.Margin.Horizontal;

            c.Width = Math.Max(0, w);
        }

        private void Sidebar_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            try
            {
                if (!e.Node.Name.Contains("parent"))
                {
                    RoutesService route = new RoutesService(e.Node.Name);
                    ShowForm(route.GetTitle(), route.GetForm());
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void OpenRoute(string routeName)
        {
            try
            {
                RoutesService route = new RoutesService(routeName);
                ShowForm(route.GetTitle(), route.GetForm());
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void ShowForm(string tabTitle, Control control)
        {
            try
            {
                tabCount++;

                TabPage newTab = new TabPage(tabTitle);

                //control.Width = this.Width - 235; 
                container.Height = this.Height * 2;
                //control.Height = this.Height;
                control.Width = this.Width - 570;
                newTab.Controls.Add(control);
                newTab.AutoScroll = true;
                tabContainer.TabPages.Add(newTab);
                tabContainer.SelectTab(newTab);
            }
            catch (Exception)
            {

                throw;
            }
        }
        private void removeTab(object sender, EventArgs e)
        {
            try
            {
                tabContainer.TabPages.Remove(tabContainer.SelectedTab);
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void tabContainer_DrawItem(object sender, DrawItemEventArgs e)
        {
            var tabPage = tabContainer.TabPages[e.Index];
            var tabRect = tabContainer.GetTabRect(e.Index);
            bool isSelected = (e.Index == tabContainer.SelectedIndex);

            // Draw the tab title
            string title = tabPage.Text;
            Font font = isSelected ? new Font(e.Font, FontStyle.Bold) : e.Font;
            using (Brush textBrush = new SolidBrush(tabPage.ForeColor))
            {
                e.Graphics.DrawString(title, font, textBrush, tabRect.X + 2, tabRect.Y + 4);
            }

            // Define close button size and position
            int closeButtonSize = 16;
            Rectangle closeButton = new Rectangle(
                tabRect.Right - closeButtonSize - 5,
                tabRect.Top + (tabRect.Height - 16) / 2,
                closeButtonSize,
                closeButtonSize
            );

            // Draw a border box (optional)
            // e.Graphics.DrawRectangle(Pens.Gray, closeButton);
            // Draw "X" centered inside the rectangle
            using (Font closeFont = new Font("Arial", 9, FontStyle.Bold))
            {
                StringFormat sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                e.Graphics.DrawString("x", closeFont, Brushes.Black, closeButton, sf);
            }
        }

        private void tabContainer_MouseDown(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < tabContainer.TabPages.Count; i++)
            {
                Rectangle tabRect = tabContainer.GetTabRect(i);
                int closeButtonSize = 16;
                Rectangle closeButton = new Rectangle(
                    tabRect.Right - closeButtonSize - 5,
                    tabRect.Top + (tabRect.Height - 16) / 2,
                    closeButtonSize,
                    closeButtonSize
                );

                bool isSelected = (i == tabContainer.SelectedIndex);
                if (isSelected && closeButton.Contains(e.Location))
                {
                    TabPage tabToRemove = tabContainer.TabPages[i];
                    tabContainer.TabPages.Remove(tabToRemove);
                    break;
                }
            }
            return;
        }

        private void SMPC_Load(object sender, EventArgs e)
        {
            Login login = new Login();
            if (DialogResult.OK == login.ShowDialog())
            {
                lbl_name.Text = CacheData.CurrentUser.first_name + " " + CacheData.CurrentUser.last_name;
                lbl_position.Text = CacheData.CurrentUser.position_id;
                lbl_department.Text = CacheData.CurrentUser.department;
                this.Enabled = true;


                ConnectWebSockets();
            }
            else
            {
                Application.Exit();
            }
        }

        private async void ConnectWebSockets()
        {
            // --- Quotation Socket ---
            quotationSocket = new WebSocketService();

            quotationSocket.OnConnected += () =>
            {
                Invoke((Action)(() => lbl_status.Text = "Quotation Connected"));
            };

            quotationSocket.OnError += (msg) =>
            {
                Invoke((Action)(() => MessageBox.Show("Quotation WS Error: " + msg)));
            };

            quotationSocket.OnDisconnected += () =>
            {
                Invoke((Action)(() => lbl_status.Text = "Quotation Disconnected"));
            };

            await quotationSocket.ConnectAndDeserialize<RedboxQuotationList>(
                ApiEndPoints.WSQUOTATIONREDBOXLIST,
                (data) => Invoke((Action)(() => LoadQuotationRedBox(data)))
            );

            // --- Production List Socket ---
            productionListSocket = new WebSocketService();

            productionListSocket.OnConnected += () =>
            {
                Invoke((Action)(() => lbl_status.Text = "Production List Connected"));
            };

            productionListSocket.OnError += (msg) =>
            {
                Invoke((Action)(() => MessageBox.Show("Production List WS Error: " + msg)));
            };

            productionListSocket.OnDisconnected += () =>
            {
                Invoke((Action)(() => lbl_status.Text = "Production List Disconnected"));
            };

            await productionListSocket.ConnectAndDeserialize<RedboxJobOrder>(
                ApiEndPoints.WSJOBORDERREDBOXLIST,
                (data) => Invoke((Action)(() => LoadJobOrderRedBox(data)))
            );
        }

        private void LoadQuotationRedBox(RedboxQuotationList data)
        {
            if (data?.quotationlist == null || data.quotationlist.Count == 0)
            {
                // Optionally clear or show "No job orders"
                flowPanelQuotationRedBox.Controls.Clear();
                return;
            }

            flowPanelQuotationRedBox.Controls.Clear();

            flowPanelQuotationRedBox.HorizontalScroll.Enabled = false;
            flowPanelQuotationRedBox.HorizontalScroll.Visible = false;

            foreach (var item in data.quotationlist)
            {
                var redBox = new RedBoxQuotationItem
                {
                    ClientName = item.client_name,
                    SalesQuotation = item.sales_quotation,
                    Status = item.status,
                    ProjectName = item.project_name,
                    SalesExecutive = item.sales_executive,
                    Remark = item.remark,
                };

                flowPanelQuotationRedBox.Controls.Add(redBox);
            }
        }

        private void LoadJobOrderRedBox(RedboxJobOrder data)
        {
            if (data?.joborder == null || data.joborder.Count == 0)
            {
                // Optionally clear or show "No job orders"
                flowPanelJobOrderRedBox.Controls.Clear();
                return;
            }

            flowPanelJobOrderRedBox.Controls.Clear();

            flowPanelJobOrderRedBox.HorizontalScroll.Enabled = false;
            flowPanelJobOrderRedBox.HorizontalScroll.Visible = false;

            foreach (var item in data.joborder)
            {
                var redBox = new RedBoxJobOrderItem
                {
                    ClientName = item.client_name,
                    DocumentNo = item.document_no,
                    Items = item.items.ToString(),
                    ProjectName = item.project_name,
                    DueDate = item.due_date,
                    Type = item.type,
                    Id = item.id.ToString(),
                };

                redBox.OnSalesOrderClicked += (s, cleanedValue) =>
                {
                    PassSalesOrder(cleanedValue);
                };

                flowPanelJobOrderRedBox.Controls.Add(redBox);
            }
        }

        private void panel4_Resize(object sender, EventArgs e)
        {
            flowPanelQuotationRedBox.Height = panel4.Height / 2;
            flowPanelJobOrderRedBox.Height = panel4.Height / 2;
        }

        public void PassSalesOrder(string id)
        {
            TabPage salesOrderTab = null;

            // Check if tab already exists
            foreach (TabPage tab in tabContainer.TabPages)
            {
                if (tab.Name == "SalesOrderEng")
                {
                    salesOrderTab = tab;
                    break;
                }
            }

            // If not found, create it
            if (salesOrderTab == null)
            {
                salesOrderTab = new TabPage("Sales Order Engineering");
                salesOrderTab.Name = "SalesOrderEng";

                var page = new smpc_engineering_app.Pages.SalesOrderEngineering.SalesOrderEngPage();
                page.Dock = DockStyle.Fill;
                salesOrderTab.Controls.Add(page);
                salesOrderTab.AutoScroll = true;

                tabContainer.TabPages.Add(salesOrderTab);
            }

            // Find the SalesOrderEngPage control and set the ID
            foreach (Control ctrl in salesOrderTab.Controls)
            {
                if (ctrl is smpc_engineering_app.Pages.SalesOrderEngineering.SalesOrderEngPage engPage)
                {
                    engPage.SetSalesOrder(id);
                    break;
                }
            }

            // Switch to the tab
            tabContainer.SelectedTab = salesOrderTab;
        }
    }
}
