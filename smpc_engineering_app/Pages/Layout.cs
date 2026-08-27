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
        GeneralService<ClearCacheModel> cacheServiceSetup;
        private ClearCacheModel _cachedata;

        // Red Box section sidebar (QUOTATIONS / PRODUCTION labels + divider)
        private Label lblQuotationsSide;
        private Label lblProductionSide;
        private Panel pnlRedBoxDivider;
        private const int RedBoxSideLabelWidth = 24;
        private const int RedBoxDividerHeight = 4;

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
            flowPanelQuotationRedBox.Layout += (s, e) => SuppressHorizontalScroll(flowPanelQuotationRedBox);

            // Hook events for JobOrder
            flowPanelJobOrderRedBox.SizeChanged += (s, e) => ResizeFlowChildren(flowPanelJobOrderRedBox);
            flowPanelJobOrderRedBox.ControlAdded += (s, e) => SizeChild(flowPanelJobOrderRedBox, e.Control);
            flowPanelJobOrderRedBox.Layout += (s, e) => SuppressHorizontalScroll(flowPanelJobOrderRedBox);

            BuildRedBoxSideLabels();

            tabContainer.SelectedIndexChanged += (s, e) => RecalculateContentWidth();
            // Phase 4.6 (UI uniformity): set the initial capped/centered width before
            // the form is ever shown - the Resize event alone would leave tabContainer
            // at its Designer-time placeholder size for one frame on startup.
            RecalculateContentWidth();
        }

        // Phase 4.6 (UI uniformity): the main content area (tabContainer, left of the
        // sidebar/panel1 and right of RedBox's panel5) caps at 1280px and stays
        // centered on wide/ultrawide monitors. RedBox's own panel (panel5) is left
        // uncapped/full-width on purpose - it's persistent utility chrome, not the
        // "page" being viewed.
        //
        // Individual pages hardcode their own size in their own code and are never
        // resized to fit whatever tabContainer happens to be (same as
        // smpc_sales_system's Quotation.cs - see that app's Layout.cs for the full
        // history of what was tried and why this shape won). tabContainer never
        // shrinks narrower than the ACTIVE tab's own page needs; pnl_content_capped's
        // own AutoScroll (Designer) scrolls the whole work area - tab strip included -
        // into view when it doesn't fit, rather than the page clipping inside a
        // too-small TabPage.
        private const int MaxContentWidth = 1280;

        private void pnl_content_capped_Resize(object sender, EventArgs e)
        {
            RecalculateContentWidth();
        }

        private Control GetActiveTabPageControl()
        {
            if (tabContainer == null) return null;
            TabPage selected = tabContainer.SelectedTab;
            return selected != null && selected.Controls.Count > 0 ? selected.Controls[0] : null;
        }

        // Live crash found in smpc_sales_system: NullReferenceException on
        // tabContainer.SelectedTab. pnl_content_capped's Resize event can fire mid-
        // InitializeComponent() - e.g. the moment it's docked into its own parent -
        // which is *before* every field this method touches is necessarily assigned
        // yet, regardless of how early each one's own "new" line appears in the
        // Designer file. Guard against both being null rather than relying on Designer
        // code-generation order to save us.
        private void RecalculateContentWidth()
        {
            if (pnl_content_capped == null || tabContainer == null) return;

            int availableWidth = pnl_content_capped.ClientSize.Width;
            int cappedWidth = Math.Min(MaxContentWidth, availableWidth);

            Control activePage = GetActiveTabPageControl();
            int neededWidth = activePage != null ? Math.Max(cappedWidth, activePage.Width) : cappedWidth;

            tabContainer.Width = neededWidth;
            tabContainer.Height = pnl_content_capped.ClientSize.Height;
            // Centers only when everything actually fits (neededWidth == cappedWidth);
            // once the active page needs more room than's available, flush-left is the
            // only position that makes sense for something you're about to scroll to
            // see the rest of.
            tabContainer.Left = neededWidth <= availableWidth ? (availableWidth - neededWidth) / 2 : 0;
            tabContainer.Top = 0;
        }

        private void BuildRedBoxSideLabels()
        {
            lblQuotationsSide = new Label
            {
                Text = "Q\nU\nO\nT\nA\nT\nI\nO\nN\nS",
                Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold),
                ForeColor = Color.FromArgb(139, 0, 0),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                Width = RedBoxSideLabelWidth
            };

            lblProductionSide = new Label
            {
                Text = "P\nR\nO\nD\nU\nC\nT\nI\nO\nN",
                Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold),
                ForeColor = Color.FromArgb(139, 0, 0),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                Width = RedBoxSideLabelWidth
            };

            pnlRedBoxDivider = new Panel
            {
                BackColor = Color.FromArgb(139, 0, 0),
                Height = RedBoxDividerHeight
            };

            // The designer docks these Top/Bottom; switch to explicit bounds so we
            // can carve out a left-side column for the section labels.
            flowPanelQuotationRedBox.Dock = DockStyle.None;
            flowPanelJobOrderRedBox.Dock = DockStyle.None;

            panel4.Controls.Add(lblQuotationsSide);
            panel4.Controls.Add(lblProductionSide);
            panel4.Controls.Add(pnlRedBoxDivider);

            LayoutRedBoxSections();
        }

        private void LayoutRedBoxSections()
        {
            if (panel4.Height <= 0 || panel4.Width <= 0) return;

            int half = panel4.Height / 2;

            // Quotations section (top half)
            lblQuotationsSide.SetBounds(0, 0, RedBoxSideLabelWidth, half);
            flowPanelQuotationRedBox.SetBounds(RedBoxSideLabelWidth, 0, panel4.Width - RedBoxSideLabelWidth, half);

            // Bold divider between the two sections
            pnlRedBoxDivider.SetBounds(0, half, panel4.Width, RedBoxDividerHeight);

            // Production section (bottom half)
            int bottomTop = half + RedBoxDividerHeight;
            int bottomHeight = panel4.Height - bottomTop;
            lblProductionSide.SetBounds(0, bottomTop, RedBoxSideLabelWidth, bottomHeight);
            flowPanelJobOrderRedBox.SetBounds(RedBoxSideLabelWidth, bottomTop, panel4.Width - RedBoxSideLabelWidth, bottomHeight);
        }

        void ResizeFlowChildren(FlowLayoutPanel flowPanel)
        {
            foreach (Control c in flowPanel.Controls)
                SizeChild(flowPanel, c);

            SuppressHorizontalScroll(flowPanel);
        }

        void SizeChild(FlowLayoutPanel flowPanel, Control c)
        {
            // Width that fills the flow panel, respecting padding & the child's margin.
            // Reserve room for the vertical scrollbar even when it isn't showing yet -
            // otherwise a card sized to the full width now can overflow by a few pixels
            // once enough cards are added to trigger the vertical scrollbar, which in
            // turn pops an unwanted horizontal scrollbar at the bottom of the box.
            int w = flowPanel.ClientSize.Width
                    - flowPanel.Padding.Horizontal
                    - c.Margin.Horizontal
                    - SystemInformation.VerticalScrollBarWidth;

            c.Width = Math.Max(0, w);

            SuppressHorizontalScroll(flowPanel);
        }

        // FlowLayoutPanel re-decides its scrollbars on every layout pass, so setting
        // HorizontalScroll.Enabled/Visible = false once (e.g. right after Controls.Clear())
        // doesn't stick - it needs to be reasserted whenever cards are added/resized.
        void SuppressHorizontalScroll(FlowLayoutPanel flowPanel)
        {
            if (flowPanel.HorizontalScroll.Visible || flowPanel.HorizontalScroll.Enabled)
            {
                flowPanel.HorizontalScroll.Enabled = false;
                flowPanel.HorizontalScroll.Visible = false;
            }
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
                // Phase 4.6 (UI uniformity): was "control.Width = this.Width - 570" (a
                // magic-number approximation of the available content width) - removed
                // entirely, along with newTab.AutoScroll. The page keeps its own
                // Designer-authored/hardcoded size; pnl_content_capped's own AutoScroll
                // (Designer) and RecalculateContentWidth (above) handle showing all of
                // it, scrolled if needed, instead of clipping it to a forced width.
                newTab.Controls.Add(control);
                tabContainer.TabPages.Add(newTab);
                tabContainer.SelectTab(newTab);
                // SelectTab above should already raise SelectedIndexChanged and trigger
                // this, but calling it directly here too is cheap and removes any doubt
                // that a freshly-added tab's own width need is accounted for immediately.
                RecalculateContentWidth();
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
                RecalculateContentWidth();
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
                    RecalculateContentWidth();
                    break;
                }
            }
            return;
        }

        private async void SMPC_Load(object sender, EventArgs e)
        {
            Login login = new Login();
            if (DialogResult.OK == login.ShowDialog())
            {
                lbl_name.Text = CacheData.CurrentUser.first_name + " " + CacheData.CurrentUser.last_name;
                lbl_position.Text = CacheData.CurrentUser.position_id;
                lbl_department.Text = CacheData.CurrentUser.department;
                this.Enabled = true;

                try
                {
                    cacheServiceSetup = new GeneralService<ClearCacheModel>(ApiEndPoints.CLEAR_CACHE);
                    _cachedata = await cacheServiceSetup.GetAsModel();
                }
                catch (Exception)
                {
                    return; // Skip ConnectWebSockets if cache fetch fails
                }

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
            LayoutRedBoxSections();
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
