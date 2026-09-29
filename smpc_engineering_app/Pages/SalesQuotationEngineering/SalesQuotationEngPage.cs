using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using smpc_engineering_app.Models;
using smpc_engineering_app.Services;
using smpc_engineering_app.Services.Helpers;
using smpc_engineering_app.Shared;
using smpc_sales_app.Pages.Sales;
using smpc_sales_system.Models;
using smpc_sales_system.Pages.Sales;
using smpc_sales_system.Services.Sales.Models;

namespace smpc_engineering_app.Pages.SalesQuotationEngineering
{
    // §3.2/§6.3 - the quotation detail editor half of Phase 4.1's "build the list" step.
    //
    // What an engineer may change here (spec 3.3, 5.1):
    //   * EVERYTHING on a tab - client needs, item/set description and notes, advanced
    //     conditions, Size Up, Final Selection, template, the item table and wiring (user
    //     decision 2026-09-04: sales routinely hands a quote over with those blank, and the
    //     engineer is the one who knows them);
    //   * the PROJECT MULTIPLIERS table, and adding, renaming and removing tabs, and
    //     reserving stock from the stock check (user decision 2026-09-28).
    // Not the header, the customer, the quote terms or a tab's red flag: the header is SHOWN,
    // read-only, and rides back as fetched - except its totals, which are worked out from the
    // items exactly as Sales works them out and sent with the save, so an engineer's item
    // change does not leave TOTAL AMOUNT DUE (which BPI's PENDING list reads) stale.
    //
    // Every tab is built the way Sales's own project save builds it (Quotation.cs,
    // IsProject), through the same ItemSetUC getters, so both apps produce the same payload
    // shape and the server's handling of Size Up and Final applies identically.
    //
    // Two bugs worth remembering, both now fixed, because both were silent:
    //
    //   * Enforcing the old view-only rule by reverting scalars to their fetched values
    //     made saves look successful while changing nothing - the diff compared identical
    //     before and after, sent no tab at all, and the API answered 200 with zero writes.
    //   * Hand-rolling the content payload from _fetchedData meant a missed lookup sent
    //     null, and a null content is NOT read as "unchanged": DeserializeSingleFromTab
    //     turns it into an empty object with content_id 0, which the id-keyed diff reports
    //     as "delete the real row, insert a blank one". Only the Size Up foreign key
    //     stopped it. Hence the id fallbacks in btn_save_Click.
    //
    // Deliberately NOT supported here (Phase 4.1 scope decision, confirmed with user):
    // swapping an item's MODEL. That routes through Quotation.cs's own
    // HandleModelSelectionClick, which depends on Sales's internal ItemList/BomHead/
    // BomDetails caches and several hundred more lines of BOM parent/child
    // re-expansion - a separate, much larger piece of work. Clicking a MODEL cell here
    // shows a message instead of silently doing nothing.
    public partial class SalesQuotationEngPage : UserControl
    {
        private int _quotationId;
        private SalesProjectList _fetchedData;
        private SalesQuotationModel _quotation;
        private readonly List<ItemSetUC> _tabControls = new List<ItemSetUC>();

        // Adds items / BOM trees to the item table. The same class Sales's own quotation
        // uses (see SalesItemGridEditor) rather than a second copy, so the two screens
        // cannot behave differently. Null until LoadQuotationAsync has fetched the item
        // and BOM catalogs; the CellClicked handler checks before using it, so a failed
        // catalog fetch degrades to "the picker doesn't open" instead of throwing.
        private SalesItemGridEditor _itemGridEditor;

        public SalesQuotationEngPage()
        {
            InitializeComponent();
            BuildHeaderDetails();
            BuildTabMenu();

            // The safety net behind the live socket, as Sales has: every five minutes, whatever
            // was saved meanwhile is merged in (quietly, and only when there is something).
            _pollTimer = new System.Windows.Forms.Timer { Interval = 5 * 60 * 1000 };
            _pollTimer.Tick += (s, e) => { if (_quotationId > 0 && !_loadingQuotation) QueueRemoteRefresh(); };
            _pollTimer.Start();
            Disposed += (s, e) => _pollTimer.Dispose();
        }

        private readonly System.Windows.Forms.Timer _pollTimer;

        private void SalesQuotationEngPage_Load(object sender, EventArgs e)
        {
            // Real work happens in SetQuotation once the caller (SalesQuotationList's
            // double-click handler) supplies an id - nothing to load without one yet.
        }

        // clientName/docNo/projectName/salesExecutive/status are display-only, passed
        // straight from the row the engineer just double-clicked (SalesQuotationList
        // already has them from its own scoped list endpoint). /sales/projects (fetched
        // below) has no customer-name/sales-exec-name resolution at all - only ids - so
        // re-deriving these from that fetch would mean a whole separate BPI lookup for a
        // purely cosmetic header. project_name is refreshed from the real fetch once it
        // completes, in case it's changed since the list was loaded.
        public async void SetQuotation(int quotationId, string clientName, string docNo, string projectName, string salesExecutive, string status)
        {
            _quotationId = quotationId;
            txt_client.Text = clientName;
            txt_doc_no.Text = docNo;
            txt_project_name.Text = projectName;
            txt_sales_executive.Text = salesExecutive;
            txt_status.Text = status;

            await LoadQuotationAsync();
        }

        private async void btn_refresh_Click(object sender, EventArgs e)
        {
            await LoadQuotationAsync();
        }

        // silent      - a load nobody asked for: an autosave refreshing its baseline, or a
        //               change arriving from Sales. It must not say "Loading...", must not
        //               disable the action strip, and must not pop a dialog - none of which
        //               a spreadsheet does when a collaborator types. The engineer is told
        //               what happened on the status line afterwards, by the caller.
        // rebuildUi   - false re-reads the data WITHOUT tearing down the tabs. Used right
        //               after this screen's own save, where what is on screen is already
        //               what was written, so the only thing that needs refreshing is the
        //               baseline the next diff is measured against.
        private async Task LoadQuotationAsync(bool silent = false, bool rebuildUi = true)
        {
            if (_quotationId <= 0) return;

            _loadingQuotation = true;
            try
            {
                if (!silent)
                {
                    btn_save.Enabled = false;
                    btn_refresh.Enabled = false;
                    lbl_saving_status.Text = "Loading...";
                }

                // ItemSetUC (from the Sales assembly) authenticates through Sales's own
                // CacheData, which is a different static from this app's. ApiService
                // mirrors the token when it first captures it, but that only happens once
                // per session - if this page is reached on an already-authenticated app the
                // capture path never runs again, so make sure the Sales-side store is in
                // step before building any tab. Without it ItemSetUC's own calls 401 and
                // ASSIGNED ENGR. / TEMPLATE come up empty.
                if (!string.IsNullOrEmpty(CacheData.SessionToken))
                    smpc_sales_app.Data.CacheData.SessionToken = CacheData.SessionToken;

                // Same reasoning for the signed-in user: saving from this page runs
                // Quotation.GetFullDiff -> BuildAutoHistoryEntries, which stamps the
                // CHANGE HISTORY rows from the SALES assembly's CacheData.CurrentUser.
                // Without this, every history row written here saves with a blank USER.
                CacheData.MirrorCurrentUserToSalesAssembly();

                // Item + BOM catalogs for the add-item picker. Loaded through the Sales
                // assembly because ItemService/ProjectService/CompanyService are internal
                // to it. Deliberately not fatal: if this fails the quotation still opens
                // fully, the engineer just can't add a new item until it's reloaded.
                if (_itemGridEditor == null)
                {
                    try
                    {
                        _itemGridEditor = await SalesItemGridEditor.CreateLoadedAsync(isProject: true);
                    }
                    catch (Exception catalogEx)
                    {
                        System.Diagnostics.Debug.WriteLine("Item catalog load failed: " + catalogEx.Message);
                    }
                }

                // This one quotation's tree - not /sales/projects, which is every project in
                // the database with all of its items, content and history. That was fetched
                // here on every open, every Refresh, every live update from Sales and after
                // every autosave, to use one project's worth of it. Same endpoint Sales uses.
                var service = new GeneralService<SalesProjectList>(ApiEndPoints.SALES_PROJECTS + "/detail/" + _quotationId);
                _fetchedData = await service.GetAsModel();

                if (_fetchedData == null)
                {
                    if (!silent) Helpers.ShowDialogMessage("error", "Failed to load quotation data.");
                    return;
                }

                _quotation = _fetchedData.SalesQuotation?.FirstOrDefault(q => q.id == _quotationId);
                if (_quotation == null)
                {
                    if (!silent) Helpers.ShowDialogMessage("error", "This quotation could not be found.");
                    return;
                }

                txt_project_name.Text = _quotation.project_name;

                // A full load shows the stored totals until an item edit works them out again.
                if (rebuildUi) _totalsEdited = false;
                ShowHeader();

                if (rebuildUi)
                {
                    BuildTabs();
                }
                else
                {
                    // No rebuild, but CHANGE HISTORY now holds the lines this save wrote, and it
                    // is drawn from the data just re-read - without this the engineer's own
                    // change never showed there until something else rebuilt the page.
                    RefreshMultiplierGrid();
                    RefreshHistoryGrid(CurrentItemSets());
                }
                if (!silent) lbl_saving_status.Text = "Loaded.";

                // Listen for saves on this project from here on, so a change made in Sales
                // shows up without anybody pressing Refresh. No-ops if already connected.
                EnsureSaveNotifyConnected(_quotationId);
            }
            catch (Exception ex)
            {
                // A background refresh that fails is not the engineer's problem to dismiss -
                // the next save or the next notification tries again. A load they asked for
                // still says so.
                if (!silent) Helpers.ShowDialogMessage("error", $"Failed to load quotation: {ex.Message}");
                else Console.WriteLine("Background refresh failed: " + ex.Message);
            }
            finally
            {
                _loadingQuotation = false;

                if (!silent)
                {
                    btn_save.Enabled = true;
                    btn_refresh.Enabled = true;
                }
            }
        }

        // Clear() takes a control out of its parent; it does NOT dispose it, and an
        // undisposed control keeps its window handle. Every tab here holds an ItemSetUC -
        // a 1132x1390 control tree with grids and panels - so each rebuild leaked hundreds
        // of handles. A process gets 10,000 USER handles, and once they run out WinForms
        // throws Win32Exception "Error creating window handle" from whatever control
        // happens to be next, which is nowhere near the leak.
        //
        // It survived this long because a rebuild used to mean "somebody pressed Refresh".
        // Autosave and live updates rebuild after every save from either side, which turns
        // a slow leak into minutes.
        private static void DisposeChildren(Control.ControlCollection controls)
        {
            for (int i = controls.Count - 1; i >= 0; i--)
            {
                Control child = controls[i];
                controls.RemoveAt(i);
                child.Dispose();
            }
        }

        // Painting held off across the whole teardown-and-rebuild below.
        //
        // SuspendLayout defers layout, not painting, so a rebuild still draws every
        // intermediate state - tabs disappearing one by one, then reappearing, then each
        // grid filling in. Tolerable when a rebuild meant "somebody pressed Refresh"; with
        // live updates it is a flicker every few seconds. Always paired through try/finally:
        // a control left with redraw off stays blank until something else invalidates it.
        private const int WM_SETREDRAW = 0x000B;

        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private static void SetRedraw(Control control, bool enabled)
        {
            if (control == null || !control.IsHandleCreated) return;

            SendMessage(control.Handle, WM_SETREDRAW, (IntPtr)(enabled ? 1 : 0), IntPtr.Zero);

            if (enabled)
            {
                control.Invalidate(true);
                control.Update();
            }
        }

        private void BuildTabs()
        {
            // The item set the engineer is on, keyed by itemset_id rather than by position:
            // a change from Sales can add or remove a tab, and an index would then land on a
            // different item set than the one being worked on. Without this, every live
            // update snaps the engineer back to the first tab.
            object selectedTabKey = tab_container.SelectedTab?.Tag;

            // The rest of the engineer's place: scroll positions, the tab's own scroll (each
            // item set scrolls inside its tab here), the items grid, and which field or cell
            // had focus. Disposing the old tabs hands focus to some other control and WinForms
            // scrolls the page to it - the jump to the middle of the page (2026-09-24).
            LiveViewSnapshot view = LiveViewSnapshot.Capture(tab_container);

            SetRedraw(tab_container, false);
            try
            {
            BuildTabsCore();
            RestoreSelectedTab(selectedTabKey);
            view.Restore(tab_container);
            }
            finally
            {
                SetRedraw(tab_container, true);
            }

            // Once more after the message queue drains, for focus and layout that settle late.
            if (IsHandleCreated)
                BeginInvoke(new Action(() => { if (!IsDisposed) view.RestoreScroll(); }));
        }

        private static void FitItemSetToTabWidth(TabPage tab, ItemSetUC uc)
        {
            int natural = uc.Width;

            void Fit()
            {
                if (tab.IsDisposed || uc.IsDisposed) return;

                // ClientSize already excludes a vertical scrollbar that is showing; one that
                // is about to appear (the item set is taller than the tab) is allowed for up
                // front, or the item set would be 17px too wide and grow a sideways scrollbar.
                int width = tab.ClientSize.Width;
                if (!tab.VerticalScroll.Visible && uc.Height > tab.ClientSize.Height)
                    width -= SystemInformation.VerticalScrollBarWidth;

                if (width > 0) uc.Width = Math.Max(natural, width);
            }

            tab.ClientSizeChanged += (s, e) => Fit();
            Fit();
        }

        // Back on the item set the user was on; otherwise the FIRST tab, as in Sales (user
        // request 2026-09-26). WinForms keeps the old SelectedIndex across a rebuild, so with
        // no match the screen used to open on whatever index the previous quotation was left on.
        private void RestoreSelectedTab(object selectedTabKey)
        {
            if (tab_container.TabPages.Count == 0) return;

            if (selectedTabKey != null)
            {
                string key = selectedTabKey.ToString();

                foreach (TabPage page in tab_container.TabPages)
                {
                    if (page.Tag != null && page.Tag.ToString() == key)
                    {
                        tab_container.SelectedTab = page;
                        return;
                    }
                }
            }

            tab_container.SelectedIndex = 0;
        }

        private void BuildTabsCore()
        {
            for (int i = tab_container.TabPages.Count - 1; i >= 0; i--)
            {
                TabPage page = tab_container.TabPages[i];
                tab_container.TabPages.RemoveAt(i);
                DisposeChildren(page.Controls);
                page.Dispose();
            }

            tab_container.TabPages.Clear();
            _tabControls.Clear();

            // A rebuild shows what is stored: a tab added or removed here and saved is in it or
            // gone from it, and one not saved is dropped with the rest of the page.
            _newTabs.Clear();
            _removedItemSetIds.Clear();

            var tabs = (_fetchedData.sales_project_item_set ?? new List<SalesProjectItemSet>())
                .Where(t => t.based_id == _quotationId)
                .ToList();

            // First, so each tab's MULTIPLIER dropdown is filled from the grid the engineer can
            // now edit (see RefreshMultiplierGrid).
            RefreshMultiplierGrid();

            foreach (var tab in tabs)
            {
                // ItemSetUC is a fixed 1132x1390 UserControl (set on itself in its own
                // designer, not something this page controls) - well taller than
                // tab_container's client area. Docking it Fill (as before) forced it down
                // to the tab's own smaller size, silently clipping everything past the
                // visible fold instead of scrolling to it - no scrollbar ever appeared
                // because a Filled child can never be bigger than its parent. AutoScroll on
                // the TabPage plus leaving the child at its natural size is what actually
                // lets the tab scroll to reach the rest of it (items grid included).
                TabPage newTab = new TabPage(tab.tab_number) { Tag = tab.itemset_id, AutoScroll = true };

                ItemSetUC uc = new ItemSetUC { Location = new System.Drawing.Point(0, 0) };

                // Every tab this page builds is an EXISTING quote, never a blank one, so
                // say so before anything else touches the control. ItemSetUC_Load is async
                // and clears the items grid as soon as its fetches return unless this flag
                // is already set. Setting it here rather than relying on SetFetchedItemData
                // (called further down, after Controls.Add has already started the load) is
                // what makes that deterministic instead of a race.
                //
                // This no longer freezes the TEMPLATE dropdown - the load-time restore has
                // its own transient flag inside ItemSetUC now, so an engineer picking a
                // different template on a saved quote actually gets it (2026-09-05).
                uc.MarkAsExistingRecord();

                WireItemSet(uc, newTab);

                var content = _fetchedData.sales_project_content?
                    .FirstOrDefault(c => c.based_id == tab.itemset_id);
                var contentTable = Helpers.ToDataTable(content == null
                    ? new List<SalesProjectContent>()
                    : new List<SalesProjectContent> { content });

                var conditionsTable = Helpers.ToDataTable((_fetchedData.sales_project_content_advanced_condition ?? new List<SalesProjectAdvancedConditions>())
                    .Where(c => c.based_id == tab.itemset_id)
                    .ToList());

                uc.SetAdvancedPanelData(conditionsTable);
                uc.SetContentsPanelData(contentTable);

                // Size Up / Final Selection (user-reported gap, 2026-09-03): both were
                // left blank here because this page never bound them - not because they
                // aren't saved. They ARE: tbl_trans_sales_project_size_up and
                // tbl_trans_sales_project_content_final both hold real rows, the API
                // preloads them onto each content row (GetSalesProjectContent's
                // DbGetWithPreloads), and Sales's own Quotation.cs binds them the same
                // way (SetFinalData/SetSizeUpData, ~L1426). Only this page skipped it.
                // No row filtering needed, unlike Sales: these arrive already nested
                // under their own content row rather than as one flat table per project.
                uc.SetSizeUpData(Helpers.ToDataTable((content?.sales_project_size_up ?? new SalesProjectSizeUp[0]).ToList()));
                uc.SetFinalData(Helpers.ToDataTable((content?.sales_project_content_final ?? new SalesProjectContentFinal[0]).ToList()));

                uc.SetTemplateName(content != null ? content.template_project_id.ToString() : "0");

                newTab.Controls.Add(uc);
                tab_container.TabPages.Add(newTab);

                // As wide as the tab, however wide that is (user, 2026-09-24) - the item set
                // was pinned at its designed 1132px and left an empty band down the right.
                // Width only: the height stays natural so the tab still scrolls down to the
                // rest of it (the reason this is not docked - see above). ItemSetUC spreads
                // its own contents across whatever width it is given.
                FitItemSetToTabWidth(newTab, uc);

                var itemsTable = Helpers.ToDataTable((_fetchedData.sales_project_items ?? new List<SalesProjectItems>())
                    .Where(i => i.based_id == tab.itemset_id)
                    .ToList());
                var wiringTable = Helpers.ToDataTable((_fetchedData.sales_project_wiring ?? new List<SalesWiringModel>())
                    .Where(w => w.based_id == tab.itemset_id)
                    .ToList());

                uc.SetFetchedItemData(itemsTable);
                uc.SetProjectWiring(wiringTable);

                // SetWiring moved to AFTER SetFetchedItemData - same fix and same reason
                // as Sales's own fetchSalesProject (2026-09-05, user-reported: "wiring is
                // not working, it didn't add [to] the item gridview"). Calling it before
                // the items grid has any data bound fires checkBox_Wiring_CheckedChanged
                // -> AddWiringRowsComponent against a null DataSource, which silently
                // does nothing - no exception, the wiring block just never appears. Now
                // it fires against the real, just-loaded items, so it correctly finds no
                // existing wiring block and adds one (or correctly finds one and leaves
                // it alone, on a record this bug didn't affect).
                uc.SetWiring(content != null ? content.is_wiring.ToString() : "false");

                // Per the type-level comment above: fully interactive, not gated to just
                // the 3 areas - the save path is what actually restricts what takes effect.
                uc.SetEditable(true);

                _tabControls.Add(uc);
            }

            // The "+" tab that adds an item set, last, as in Sales. It holds no item set; every
            // loop over the tabs skips it because it has no ItemSetUC.
            tab_container.TabPages.Add(new TabPage(AddTabText));

            // Multiplier and Change History sit in pnl_multiplier_history above tab_container
            // (user decision 2026-09-03) - same idea as Sales's Quotation.cs, where
            // dgv_project_multiplier is a sibling of tabControl2, always visible whichever
            // item-set tab is selected.
            RefreshHistoryGrid(tabs);
        }

        // ---- One item set's wiring ------------------------------------------------------
        //
        // Every tab - a stored one built by BuildTabsCore and one added here with "+" - listens
        // to the same events, so a new tab is not a second-class tab.
        private void WireItemSet(ItemSetUC uc, TabPage page)
        {
            // What makes this screen autosave: the same change events Sales listens to.
            // Each one only restarts the idle timer - the save itself happens once the
            // engineer stops.
            uc.DataChangedContent += (s2, e2) => QueueAutoSave();
            uc.CellChangedProject += (s2, e2) => QueueAutoSave();

            // Advanced Conditions and the wiring table are saved from this screen too (see
            // BuildSaveChanges), but nothing queued a save for either - a condition or a
            // wiring edit waited for the next item edit, or for Save (user-reported
            // 2026-09-26: "update the advance conditions ... it will not update").
            uc.DataChangedConditions += (s2, e2) => QueueAutoSave();
            uc.CellChangedWiring += (s2, e2) => QueueAutoSave();

            // An item line changed: the quotation's totals, as Sales's Cell_EditedUC ->
            // RecomputeParentTotals works them out on the same event.
            uc.CellEdited += (s2, e2) => RecomputeTotals();

            uc.SizeUpClicked += async (s, e) => await OnSizeUpClicked(uc);
            uc.FinalTxtBoxClicked += async (s, e) => await OnFinalClicked(uc);

            // Add an item / accessory / BOM to the item table (user-reported gap,
            // 2026-09-04: "can't add accessories like the project quotation can").
            // Sales subscribes this same event to the same logic; this page simply
            // never subscribed, so clicking the add cell did nothing at all.
            uc.CellClicked += async (s, e) => await OnAddItemClicked(uc);

            // The rest of what Sales does with an item set, from the same shared code
            // (user, 2026-09-26: "all the function had in sales project quotation suppose
            // on the engineering"): the MODEL swap, the TEMPLATE button, and the stock
            // check - with RESERVE since 2026-09-28 (spec 3.3, 10.4.1).
            uc.CellClickedModel += async (s, e) => await OnModelClicked(uc);
            uc.ButtonClicked += async (s, e) => await OnTemplateClicked(uc);
            uc.CellClickedStock += async (s, e) => await OnStockCheckClicked(uc, page);

            // Price lines with Company Setup's multiplier and VAT, as Sales does, not the
            // control's built-in 1.186 and 12% - the two screens must show the same prices.
            if (_itemGridEditor != null)
            {
                uc.MarkUpMultiplier = _itemGridEditor.CompanyMarkupMultiplier;
                uc.VatRate = _itemGridEditor.CompanyVatRate;
                uc.ImageList = _itemGridEditor.ImageList;
            }

            uc.setMultiplier(MultiplierChoices());
        }

        // Read-only DataGridView shared by both tabs below - AllowUserToAddRows/Delete off
        // and ReadOnly on so nothing here is mistaken for one of the three actually-editable
        // areas (Size Up, item table, wiring).
        private static DataGridView BuildReadOnlyGrid()
        {
            return new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToOrderColumns = false,
                AllowUserToResizeRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                BackgroundColor = System.Drawing.SystemColors.Window
            };
        }

        // The multiplier SETUP grid (brand/component/description/multiplier% - what a
        // line item's own MULTIPLIER dropdown, wired via setMultiplier above, actually
        // picks from) lives at the quotation level in Sales's Quotation.cs
        // (dgv_project_multiplier, a sibling of tabControl2 - not part of ItemSetUC at
        // all), which is why nothing on this page ever showed it: this page only ever
        // reused ItemSetUC's own per-item dropdown-choices wiring, never built the
        // equivalent of that separate quotation-level grid. Rebuilt into grp_multiplier
        // fresh on every load/refresh, same as the per-item-set tabs.
        private List<SalesProjectItemSet> CurrentItemSets()
        {
            return (_fetchedData?.sales_project_item_set ?? new List<SalesProjectItemSet>())
                .Where(t => t.based_id == _quotationId)
                .ToList();
        }

        // One grid per box, created once and rebound after that. Two things were wrong with
        // building a new one each time: every refresh (now every few seconds, with live sync)
        // threw a grid's worth of window handles away; and the grid was configured before it
        // was on the form, when it has no columns yet - so "hide multiplier_id", "call it OLD
        // DATA" and the rest found nothing to act on, and the raw column names showed.
        private DataGridView _multiplierGrid;
        private DataGridView _historyGrid;

        private static DataGridView EnsureSideGrid(ref DataGridView grid, Control box)
        {
            if (grid == null || grid.IsDisposed)
            {
                DisposeChildren(box.Controls);
                grid = BuildReadOnlyGrid();
                box.Controls.Add(grid);
            }
            return grid;
        }

        // The PROJECT MULTIPLIERS table - editable here since 2026-09-28 (user decision; spec
        // 3.3, 5.1.1): the engineer the quote was sent to adds, edits and removes rows as Sales
        // does. Bound to the stored rows in the order the API returns them, which is the order
        // the save's DiffByIndex compares them in, so the ids never need to be on screen.
        private DataGridView EnsureMultiplierGrid()
        {
            if (_multiplierGrid == null || _multiplierGrid.IsDisposed)
            {
                DisposeChildren(grp_multiplier.Controls);
                _multiplierGrid = new DataGridView
                {
                    Dock = DockStyle.Fill,
                    AllowUserToAddRows = true,
                    AllowUserToDeleteRows = true,
                    AllowUserToOrderColumns = false,
                    AllowUserToResizeRows = false,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    MultiSelect = false,
                    BackgroundColor = System.Drawing.SystemColors.Window
                };
                _multiplierGrid.CellEndEdit += (s, e) => OnMultipliersEdited();
                _multiplierGrid.UserDeletedRow += (s, e) => OnMultipliersEdited();
                grp_multiplier.Controls.Add(_multiplierGrid);
            }
            return _multiplierGrid;
        }

        private void RefreshMultiplierGrid()
        {
            var table = new DataTable();
            foreach (string column in new[] { "brand", "component", "description", "multiplier" })
                table.Columns.Add(column, typeof(string));

            foreach (var m in ProjectLiveMerge.StoredMultipliers(_fetchedData, _quotationId))
                table.Rows.Add(m.brand, m.component, m.description, m.multiplier);

            var dgv = EnsureMultiplierGrid();
            dgv.DataSource = table;

            dgv.Columns["brand"].HeaderText = "BRAND";
            dgv.Columns["component"].HeaderText = "COMPONENT";
            dgv.Columns["description"].HeaderText = "DESCRIPTION";
            dgv.Columns["multiplier"].HeaderText = "MULTIPLIER";
        }

        // The multipliers as a save sends them: every row with anything in it, in grid order.
        private List<SalesProjectMultiplier> ScreenMultipliers()
        {
            var rows = new List<SalesProjectMultiplier>();
            if (_multiplierGrid == null || _multiplierGrid.IsDisposed) return rows;

            foreach (DataGridViewRow row in _multiplierGrid.Rows)
            {
                if (row.IsNewRow) continue;

                string Cell(string name) => row.Cells[name].Value?.ToString()?.Trim() ?? string.Empty;
                var m = new SalesProjectMultiplier
                {
                    based_id = (uint)_quotationId,
                    brand = Cell("brand"),
                    component = Cell("component"),
                    description = Cell("description"),
                    multiplier = Cell("multiplier")
                };

                if (m.brand.Length + m.component.Length + m.description.Length + m.multiplier.Length == 0) continue;
                rows.Add(m);
            }
            return rows;
        }

        // What each item line's MULTIPLIER dropdown offers: a blank first (a line may have no
        // multiplier), then the table's multipliers - Sales's fetchMultiplierData.
        private List<string> MultiplierChoices()
        {
            var choices = new List<string> { string.Empty };
            choices.AddRange(ScreenMultipliers()
                .Select(m => m.multiplier)
                .Where(m => !string.IsNullOrWhiteSpace(m))
                .Distinct());
            return choices;
        }

        private void OnMultipliersEdited()
        {
            List<string> choices = MultiplierChoices();
            foreach (var uc in _tabControls)
            {
                if (uc != null && !uc.IsDisposed) uc.setMultiplier(choices);
            }
            QueueAutoSave();
        }

        // History is stored per item-set (based_id = itemset_id, same as items/wiring - see
        // the save method's own history filter above), not per-quotation like Multiplier.
        // Combined into one grid tagged by which item-set tab each entry belongs to, rather
        // than splitting it across every per-item-set tab - simpler to scan, and most
        // quotations only have one item-set anyway. GetHistoryList() in ItemSetUC.cs
        // (Sales's own side) is a dead stub that was never finished, so there's no existing
        // pattern to match here - this is a new viewer, not a port of one. Rebuilt into
        // grp_history fresh on every load/refresh, same as grp_multiplier above.
        private void RefreshHistoryGrid(List<SalesProjectItemSet> tabs)
        {
            var tabNumberByItemSetId = tabs.ToDictionary(t => t.itemset_id, t => t.tab_number);

            // Every tab AND the project header, as one list - the same list Sales shows. It was
            // tab rows only, so a change to the project name, the multipliers, REQUEST FOR ENGR.
            // or a removed tab never appeared here at all. Classified by label as well as id:
            // see ProjectHistoryRows for why based_id alone mixes quotations up.
            var tabIds = new HashSet<int>(tabNumberByItemSetId.Keys);

            var history = (_fetchedData.sales_project_history ?? new List<SalesProjectHistory>())
                .Where(h => ProjectHistoryRows.IsTabRow(h, tabIds) || ProjectHistoryRows.IsProjectRow(h, _quotationId))
                .OrderByDescending(h => h.history_id)
                .Select(h => new
                {
                    TAB = ProjectHistoryRows.IsTabRow(h, tabIds) ? tabNumberByItemSetId[(int)h.based_id] : "PROJECT",
                    USER = h.user,
                    DATE = h.date,
                    TIME = h.time,
                    OLD_DATA = h.old_data,
                    NEW_DATA = h.new_data
                })
                .ToList();

            var dgv = EnsureSideGrid(ref _historyGrid, grp_history);
            dgv.DataSource = Helpers.ToDataTable(history);

            if (dgv.Columns["OLD_DATA"] != null) dgv.Columns["OLD_DATA"].HeaderText = "CHANGED";
            if (dgv.Columns["NEW_DATA"] != null) dgv.Columns["NEW_DATA"].HeaderText = "OLD => NEW";
        }

        // Reads an id out of one of the ItemSetUC payload dictionaries. The values come
        // from panel controls, so an id can arrive as int, long, string or be missing
        // entirely; anything unparseable counts as 0 ("no id supplied").
        private static int ToId(Dictionary<string, dynamic> data, string key)
        {
            if (data == null || !data.ContainsKey(key) || data[key] == null) return 0;
            return int.TryParse(data[key].ToString(), out int id) ? id : 0;
        }

        // Opens the item/BOM picker for the clicked row and inserts the choice.
        //
        // Mirrors Sales's Cell_ClickedUC: read the clicked row off the control, reset the
        // per-tab parent counter, and hand the control's own grid to the shared editor.
        // The counter reset matters because reference numbering is per item set - without
        // it, adding on one tab then another continues the first tab's numbering.
        private async Task OnAddItemClicked(ItemSetUC uc)
        {
            if (uc == null) return;

            if (_itemGridEditor == null)
            {
                Helpers.ShowDialogMessage("error",
                    "The item list hasn't loaded yet. Use Refresh, then try again.");
                return;
            }

            _itemGridEditor.CounterParent = 1;

            // A picked item goes into the grid's table directly, which raises no cell-change
            // event - so without this the autosave never heard of it.
            int rowsBefore = uc.DgvProjectItems.Rows.Count;
            await _itemGridEditor.HandleItemSelectionClickAsync(uc.GetIndex(), uc.DgvProjectItems);
            if (uc.DgvProjectItems.Rows.Count != rowsBefore)
            {
                RecomputeTotals();
                QueueAutoSave();
            }
        }

        // The MODEL cell on a row that did not come from a template - the same swap Sales
        // does (SalesItemGridEditor). This screen used to answer it with "isn't available
        // from this screen yet" (QA, 2026-09-26: the model could not be changed here).
        // Template rows pick their model inside ItemSetUC itself, as in Sales.
        private async Task OnModelClicked(ItemSetUC uc)
        {
            if (uc == null) return;

            if (_itemGridEditor == null)
            {
                Helpers.ShowDialogMessage("error",
                    "The item list hasn't loaded yet. Use Refresh, then try again.");
                return;
            }

            await _itemGridEditor.HandleModelSelectionClickAsync(uc.GetIndex(), uc.DgvProjectItems);
            RecomputeTotals();
            QueueAutoSave();
        }

        // The TEMPLATE button - the same template picker Sales opens.
        private async Task OnTemplateClicked(ItemSetUC uc)
        {
            if (uc == null) return;

            await ProjectItemSetActions.ApplyTemplateAsync(uc);
            RecomputeTotals();
            QueueAutoSave();
        }

        // ---- Stock check, with RESERVE --------------------------------------------------
        //
        // The engineer the quote was sent to may reserve stock too (user decision 2026-09-28;
        // spec 3.3, 10.4.1) - through the same stock check and the same reservation calls as
        // Sales. What is ticked is kept per item set (reference codes repeat from tab to tab),
        // applied at once to lines already saved, and to the rest after the next save. A tab
        // added here and not saved yet has no lines to point a reservation at: its check is
        // view-only until it saves.
        private readonly Dictionary<int, Dictionary<string, bool>> _pendingReservations = new Dictionary<int, Dictionary<string, bool>>();

        private async Task OnStockCheckClicked(ItemSetUC uc, TabPage page)
        {
            if (uc == null) return;

            int itemSetId = ItemSetIdOf(page);
            if (itemSetId <= 0 || _newTabs.Contains(page))
            {
                await ProjectItemSetActions.ShowStockCheckReadOnlyAsync(uc, _quotationId);
                return;
            }

            if (!_pendingReservations.TryGetValue(itemSetId, out var pending))
                _pendingReservations[itemSetId] = pending = new Dictionary<string, bool>();

            var affected = new HashSet<int>();
            await ProjectItemSetActions.ShowStockCheckWithReserveAsync(uc, _quotationId, ValidUntil(), pending, affected);
            RefreshStockOnOtherTabs(affected, uc);
        }

        // After a save: lines that had no id when their RESERVE was ticked have one now.
        private async Task ApplyPendingReservationsAsync()
        {
            var failures = new List<string>();
            var affected = new HashSet<int>();

            foreach (TabPage page in tab_container.TabPages)
            {
                if (!(page.Controls.Count > 0 && page.Controls[0] is ItemSetUC uc)) continue;
                if (!_pendingReservations.TryGetValue(ItemSetIdOf(page), out var pending) || pending.Count == 0) continue;

                failures.AddRange(await ProjectItemSetActions.ApplyPendingReservationsAsync(uc, pending, _quotationId, ValidUntil(), affected));
            }

            RefreshStockOnOtherTabs(affected, null);

            if (failures.Count > 0)
                Helpers.ShowDialogMessage("error", "The quotation saved, but some reservation changes could not be applied:\n" + string.Join("\n", failures));
        }

        // A reservation changes the stock every tab showing that item reads, not only the tab it
        // was raised on (which the shared apply has already refreshed - `done`).
        private void RefreshStockOnOtherTabs(ICollection<int> itemIds, ItemSetUC done)
        {
            if (itemIds == null || itemIds.Count == 0) return;

            foreach (var uc in _tabControls)
            {
                if (uc == null || uc.IsDisposed || uc == done) continue;

                foreach (int itemId in itemIds) uc.ClearStockCache(itemId);
                uc.RefreshAllStockIndicators();
            }
        }

        // The quotation's VALID UNTIL - what a reservation expires with (spec 10.4.5).
        private DateTime? ValidUntil()
        {
            return DateTime.TryParse(_quotation?.valid_until, out DateTime date) ? date.Date : (DateTime?)null;
        }

        private static int ItemSetIdOf(TabPage page)
        {
            return page != null && int.TryParse(page.Tag?.ToString(), out int id) ? id : 0;
        }

        // ---- The quotation header, read-only ----------------------------------------------
        //
        // Engineers see the whole header (user decision 2026-09-28; spec 5.1) - it was the
        // client, the number, the project, the sales executive and the status only. Read-only:
        // none of it is theirs to change, and the save sends it back as fetched, bar the totals.
        private readonly Dictionary<string, TextBox> _headerFields = new Dictionary<string, TextBox>();

        private void BuildHeaderDetails()
        {
            var details = new FlowLayoutPanel
            {
                Location = new Point(14, 92),
                Size = new Size(panel_header.ClientSize.Width - 28, 98),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                WrapContents = true
            };

            void Field(string key, string caption, int width)
            {
                var cell = new Panel { Size = new Size(width + 4, 42), Margin = new Padding(0, 0, 12, 4) };
                cell.Controls.Add(new Label { Text = caption, AutoSize = true, Location = new Point(2, 0) });
                var box = new TextBox { Enabled = false, Location = new Point(3, 16), Width = width };
                cell.Controls.Add(box);
                details.Controls.Add(cell);
                _headerFields[key] = box;
            }

            Field("date", "DATE", 90);
            Field("valid_until", "VALID UNTIL", 90);
            Field("purpose", "PURPOSE", 100);
            Field("application", "APPLICATION", 120);
            Field("payment_terms", "PAYMENT TERMS", 140);
            Field("ship_type", "SHIP TYPE", 100);
            Field("warranty", "WARRANTY", 90);
            Field("address_to", "ADDRESSED TO", 150);
            Field("thru", "THRU", 150);
            Field("ship_to", "SHIP TO", 260);
            Field("bill_to", "BILL TO", 260);
            Field("gross_sales", "GROSS SALES", 110);
            Field("vat_amount", "VAT", 100);
            Field("net_sales", "NET SALES", 110);
            Field("cash_discount", "CASH DISCOUNT", 100);
            Field("total_amount_due", "TOTAL AMOUNT DUE", 120);

            panel_header.Height = 196;
            panel_header.Controls.Add(details);
        }

        private void ShowHeader()
        {
            if (_quotation == null) return;
            SalesProjectHeaderNames names = _fetchedData?.header_names;

            SetHeader("date", AsDate(_quotation.date));
            SetHeader("valid_until", AsDate(_quotation.valid_until));
            SetHeader("purpose", _quotation.purpose);
            SetHeader("application", names?.application);
            SetHeader("payment_terms", names?.payment_terms);
            SetHeader("ship_type", names?.ship_type);
            SetHeader("warranty", _quotation.warranty);
            SetHeader("address_to", _quotation.address_to);
            SetHeader("thru", _quotation.thru);
            SetHeader("ship_to", names?.ship_to);
            SetHeader("bill_to", names?.bill_to);
            SetHeader("cash_discount", ((decimal)_quotation.cash_discount).ToString("N2"));

            // The stored totals, until an item edit here works them out afresh.
            if (!_totalsEdited)
                ShowTotals((decimal)_quotation.gross_sales, (decimal)_quotation.vat_amount,
                    (decimal)_quotation.net_sales, (decimal)_quotation.total_amount_due);
        }

        private void SetHeader(string key, string value)
        {
            if (_headerFields.TryGetValue(key, out TextBox box)) box.Text = value ?? string.Empty;
        }

        private void ShowTotals(decimal gross, decimal vat, decimal net, decimal total)
        {
            SetHeader("gross_sales", gross.ToString("N2"));
            SetHeader("vat_amount", vat.ToString("N2"));
            SetHeader("net_sales", net.ToString("N2"));
            SetHeader("total_amount_due", total.ToString("N2"));
        }

        private static string AsDate(string value)
        {
            return DateTime.TryParse(value, out DateTime date) ? date.ToString("MM/dd/yyyy") : (value ?? string.Empty);
        }

        // ---- The quotation's totals ----------------------------------------------------------
        //
        // Worked out as Sales's RecomputeParentTotals works them out - every tab that is not
        // red-flagged, summed, then the one project-wide cash discount - on the same event (an
        // item line edited) plus a line added, a model or template applied, a tab removed; and
        // sent with the save. Before 2026-09-28 this screen sent them back as fetched, so an
        // engineer's item change left TOTAL AMOUNT DUE - which BPI's PENDING list reads - as
        // Sales had last saved it. Only once lines are priced with Company Setup's rates
        // (_itemGridEditor loaded): a total worked out with the control's built-in rates is
        // never sent.
        private bool _totalsEdited;
        private decimal _gross, _vat, _net, _percent, _netAmountDue, _totalAmountDue;

        private void RecomputeTotals()
        {
            if (_quotation == null || _itemGridEditor == null) return;

            decimal gross = 0, vat = 0, net = 0;
            foreach (TabPage page in tab_container.TabPages)
            {
                if (!(page.Controls.Count > 0 && page.Controls[0] is ItemSetUC uc) || uc.IsDisposed) continue;
                if (IsExcludedTab(page)) continue;

                Dictionary<string, dynamic> data;
                try
                {
                    data = uc.ProjectComputationLoop();
                }
                catch (Exception ex)
                {
                    // A line mid-edit with something unparseable in it: keep the last good
                    // figures rather than send a wrong one.
                    Console.WriteLine("Totals not worked out: " + ex.Message);
                    return;
                }
                if (data == null) continue;

                gross += Amount(data, "gross_sales");
                vat += Amount(data, "vat_amount");
                net += Amount(data, "net_sales");
            }

            decimal cash = (decimal)_quotation.cash_discount;
            _gross = gross;
            _vat = vat;
            _net = net;
            _percent = gross != 0 ? ((gross - net) / gross) * 100 : 0;
            _netAmountDue = net - cash;
            _totalAmountDue = _netAmountDue + vat;
            _totalsEdited = true;

            ShowTotals(_gross, _vat, _net, _totalAmountDue);
        }

        private static decimal Amount(Dictionary<string, dynamic> data, string key)
        {
            if (!data.TryGetValue(key, out dynamic value) || value == null) return 0m;
            string text = new string(((object)value).ToString().Where(ch => char.IsDigit(ch) || ch == '.' || ch == '-').ToArray());
            return decimal.TryParse(text, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out decimal amount) ? amount : 0m;
        }

        // A red-flagged tab (Sales's right-click RedFlag) is left out of the totals, as there.
        private bool IsExcludedTab(TabPage page)
        {
            int itemSetId = ItemSetIdOf(page);
            if (itemSetId <= 0) return false;
            return _fetchedData?.sales_project_content?.FirstOrDefault(c => c.based_id == itemSetId)?.IsExcluded == true;
        }

        // ---- Tabs: add, rename, remove ----------------------------------------------------
        //
        // As Sales does them (user decision 2026-09-28; spec 3.3): "+" adds an item set, and a
        // tab's right-click menu renames or removes it. The red flag stays Sales's - leaving a
        // tab out of the totals is pricing. A saved tab that is removed is sent as a deliberate
        // removal (removed_item_set_ids), the only kind the API and the autosave guard accept.
        private const string AddTabText = "+";
        private readonly HashSet<TabPage> _newTabs = new HashSet<TabPage>();
        private readonly HashSet<int> _removedItemSetIds = new HashSet<int>();

        private static bool IsAddTab(TabPage page)
        {
            return page != null && page.Text == AddTabText && page.Controls.Count == 0;
        }

        private void BuildTabMenu()
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add("Rename Tab", null, (s, e) => RenameSelectedTab());
            menu.Items.Add("Remove Tab", null, (s, e) => RemoveSelectedTab());
            menu.Opening += (s, e) =>
            {
                // Only on a tab's own header - a right-click inside the item set bubbles up here.
                TabPage page = TabAt(tab_container.PointToClient(Cursor.Position));
                if (page == null || IsAddTab(page) || _loadingQuotation || _quotation == null)
                {
                    e.Cancel = true;
                    return;
                }
                tab_container.SelectedTab = page;
            };
            tab_container.ContextMenuStrip = menu;

            // "+" is a button, not a place: clicking it adds an item set, and it is never selected.
            tab_container.Selecting += (s, e) =>
            {
                if (IsAddTab(e.TabPage)) e.Cancel = true;
            };
            tab_container.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left && IsAddTab(TabAt(e.Location))) AddNewTab();
            };
        }

        private TabPage TabAt(Point clientPoint)
        {
            for (int i = 0; i < tab_container.TabCount; i++)
            {
                if (tab_container.GetTabRect(i).Contains(clientPoint)) return tab_container.TabPages[i];
            }
            return null;
        }

        // Sales's own tab-name prompt. null when cancelled, "" when left blank.
        private static string AskTabName()
        {
            using (var modal = new ItemSetModal())
            {
                if (modal.ShowDialog() != DialogResult.OK) return null;
                return modal.GetResult()?.Trim() ?? string.Empty;
            }
        }

        private void AddNewTab()
        {
            if (_fetchedData == null || _quotation == null || _loadingQuotation) return;

            string name = AskTabName();
            if (name == null) return;

            int realTabs = tab_container.TabPages.Cast<TabPage>().Count(p => !IsAddTab(p));
            if (name.Length == 0) name = "New Project " + (realTabs + 1);

            // Tag 0 until the save gives it an item set id; _newTabs is what marks it new.
            var page = new TabPage(name) { Tag = 0, AutoScroll = true };
            var uc = new ItemSetUC { Location = new Point(0, 0) };
            WireItemSet(uc, page);
            page.Controls.Add(uc);

            int at = tab_container.TabCount > 0 && IsAddTab(tab_container.TabPages[tab_container.TabCount - 1])
                ? tab_container.TabCount - 1
                : tab_container.TabCount;
            tab_container.TabPages.Insert(at, page);
            FitItemSetToTabWidth(page, uc);
            uc.SetEditable(true);

            _newTabs.Add(page);
            _tabControls.Add(uc);
            tab_container.SelectedTab = page;

            QueueAutoSave();
        }

        private void RenameSelectedTab()
        {
            TabPage page = tab_container.SelectedTab;
            if (page == null || IsAddTab(page)) return;

            string name = AskTabName();
            if (string.IsNullOrEmpty(name) || name == page.Text) return;

            page.Text = name;
            QueueAutoSave();
        }

        private void RemoveSelectedTab()
        {
            TabPage page = tab_container.SelectedTab;
            if (page == null || IsAddTab(page)) return;

            // A project quotation needs somewhere for its items to live.
            if (tab_container.TabPages.Cast<TabPage>().Count(p => !IsAddTab(p)) <= 1)
            {
                Helpers.ShowDialogMessage("error", "A project quotation needs at least one tab - add another before removing this one.");
                return;
            }

            if (MessageBox.Show($"Remove tab \"{page.Text}\" and all its items? This can't be undone.", "Remove Tab",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            int itemSetId = ItemSetIdOf(page);
            if (!_newTabs.Contains(page) && itemSetId > 0)
                _removedItemSetIds.Add(itemSetId);

            _newTabs.Remove(page);
            _pendingReservations.Remove(itemSetId);
            if (page.Controls.Count > 0 && page.Controls[0] is ItemSetUC uc)
                _tabControls.Remove(uc);

            int index = tab_container.TabPages.IndexOf(page);
            tab_container.TabPages.Remove(page);
            DisposeChildren(page.Controls);
            page.Dispose();

            int realTabs = tab_container.TabPages.Cast<TabPage>().Count(p => !IsAddTab(p));
            tab_container.SelectedIndex = Math.Max(0, Math.Min(index, realTabs - 1));

            RecomputeTotals();
            QueueAutoSave();
        }

        // One pump picker at a time, across BOTH grids (user-reported 2026-09-05: rapid
        // clicking stacked several, and SIZE UP's picker could open on top of FINAL's).
        //
        // Both handlers await the pump list before reaching ShowDialog, and the page
        // stays live during that await, so clicks queue up behind it. ShowDialog then runs
        // its own message loop, which lets a pending continuation resume while the first
        // modal is already up and open a second one over it - being modal does not stop it.
        // The flag is claimed before the await for exactly that reason, and is shared by
        // SIZE UP and FINAL so that while either is open the other stays shut.
        private bool _pumpPickerOpen = false;

        private async Task OnSizeUpClicked(ItemSetUC uc)
        {
            if (_pumpPickerOpen) return;
            _pumpPickerOpen = true;

            // The flag stops two pickers being open together; it cannot stop a click that
            // queued up while this one was being fetched from opening another as soon as
            // this one closes. Disabling the grids discards those clicks instead of
            // queueing them. See ItemSetUC.SetPumpPickerBusy.
            uc?.SetPumpPickerBusy(true);

            try
            {
                await OnSizeUpClickedCore(uc);
            }
            finally
            {
                uc?.SetPumpPickerBusy(false);
                _pumpPickerOpen = false;
            }
        }

        // The picker is Sales's own (ProjectItemSetActions): pump rows from the server, not
        // the whole item catalogue this used to download to find them.
        private async Task OnSizeUpClickedCore(ItemSetUC uc)
        {
            await ProjectItemSetActions.PickSizeUpAsync(uc);

            // A plain repaint after changing a grid inside an AutoScroll'd tab.
            uc.Refresh();
        }

        private async Task OnFinalClicked(ItemSetUC uc)
        {
            if (_pumpPickerOpen) return;
            _pumpPickerOpen = true;

            // Same reasoning as OnSizeUpClicked, and this is the path the report was about:
            // FINAL fetches the pump list before it can show anything, so the wait between
            // the click and the modal is the one people click through.
            uc?.SetPumpPickerBusy(true);

            try
            {
                await OnFinalClickedCore(uc);
            }
            finally
            {
                uc?.SetPumpPickerBusy(false);
                _pumpPickerOpen = false;
            }
        }

        // Sales's own FINAL picker (ProjectItemSetActions): several pumps at once, and each
        // with the FLA and VOLTAGE from its Item Entry specs. This screen's copy passed both
        // as "", so an engineer's FINAL could never feed the wiring amp formulas (spec 8.4).
        private async Task OnFinalClickedCore(ItemSetUC uc)
        {
            await ProjectItemSetActions.PickFinalAsync(uc);

            // A plain repaint after changing a grid inside an AutoScroll'd tab.
            uc.Refresh();
        }

        // ---- Live updates and autosave ------------------------------------------
        //
        // The client asked for this screen and the Sales project quotation to behave "like
        // Gsheet" (2026-09-24): what you type saves on its own, you are told it saved, you
        // stay where you are, and the other side sees it even while that side is editing.
        //
        // The mechanism is the save itself. The API already broadcasts
        // { "event": "quotation_saved" } on a project's own channel whenever anyone saves it
        // (BroadcastToProject), so turning "typing" into "saving" is all that is needed for
        // the two screens to track each other - there is no second live-edit protocol.
        //
        // Sales has had the listening half for a while; this page had neither half, which is
        // why an engineer saw nothing until they pressed Refresh.
        private System.Net.WebSockets.ClientWebSocket _saveNotifyWebSocket;
        private System.Threading.CancellationTokenSource _saveNotifyCts;
        private int _saveNotifyProjectId;

        private System.Windows.Forms.Timer _autoSaveTimer;
        private bool _autoSaving;
        private bool _applyingRemoteUpdate;

        // With the item-set control's own debounce in front of it (ItemSetUC's
        // timer_send_message_*), an edit reached the other screen after ~2 + 2 + 2.5 s. Both
        // shortened 2026-09-26 ("the possible other that will not update quickly").
        private const int AutoSaveIdleMs = 1000;

        // One save this screen made, still waiting for its own broadcast to come back. See
        // HandleQuotationSaved for why the echo has to be recognised at all. Time-boxed
        // so a broadcast that never arrives - dropped socket, or a save whose notification
        // was lost - cannot leave this screen permanently ignoring the first real update
        // from Sales that follows.
        private DateTime _ownSaveAt = DateTime.MinValue;
        private bool _ownSaveEchoPending;

        private static readonly TimeSpan OwnSaveEchoWindow = TimeSpan.FromSeconds(10);

        // ---- Live updates from Sales, collapsed -----------------------------------
        //
        // Deliberately not a straight "notification arrives -> reload now". Sales's saves are
        // keystroke-driven autosaves, so the notifications come in bursts. The timer restarts
        // on each one and only fires once they stop.
        private System.Windows.Forms.Timer _remoteRefreshTimer;

        // Short now that the other app's change is merged in place (ProjectLiveMerge) rather
        // than rebuilding every tab - it only has to collapse a burst of notifications.
        private const int RemoteRefreshDebounceMs = 600;

        private void QueueRemoteRefresh()
        {
            if (_remoteRefreshTimer == null)
            {
                _remoteRefreshTimer = new System.Windows.Forms.Timer { Interval = RemoteRefreshDebounceMs };
                _remoteRefreshTimer.Tick += RemoteRefreshTimer_Tick;
            }

            _remoteRefreshTimer.Stop();
            _remoteRefreshTimer.Start();
        }

        private async void RemoteRefreshTimer_Tick(object sender, EventArgs e)
        {
            _remoteRefreshTimer.Stop();

            try
            {
                if (_quotationId <= 0) return;

                // Rebinding on top of our own in-flight save would read from data that does
                // not include it yet. Wait for the next gap instead.
                if (_autoSaving || _loadingQuotation)
                {
                    QueueRemoteRefresh();
                    return;
                }

                // In place, around whatever the engineer is doing (ProjectLiveMerge). Only a
                // change of shape still rebuilds every tab, and that still waits for a pause.
                if (!await MergeRemoteChangesAsync(_fetchedData))
                {
                    if (IsUserMidEdit())
                    {
                        QueueRemoteRefresh();
                        return;
                    }

                    _applyingRemoteUpdate = true;
                    try
                    {
                        await LoadQuotationAsync(silent: true);
                    }
                    finally
                    {
                        _applyingRemoteUpdate = false;
                    }
                }

                if (_lastMergeChangedScreen)
                    lbl_saving_status.Text = "Updated by Sales " + DateTime.Now.ToString("HH:mm");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Live refresh failed: " + ex.Message);
            }
        }

        // Whether this diff would delete an item set - i.e. remove a whole tab and everything
        // filed under it. Read off the built diff rather than guessed at, so it catches the
        // deletion however it was arrived at. The shape is GetFullDiff's own: result["Tabs"]
        // is a list of serialized TabDiff objects, so the nested values are JObject/JArray.
        //
        // `requested` - the tabs removed here on purpose (right-click > Remove Tab, since
        // 2026-09-28). Any other removal is the save being wrong, as before.
        private static bool DiffRemovesUnrequestedItemSet(Dictionary<string, dynamic> changes, ICollection<int> requested)
        {
            if (changes == null || !changes.ContainsKey("Tabs")) return false;

            if (!(changes["Tabs"] is List<Dictionary<string, dynamic>> tabs)) return false;

            foreach (var tab in tabs)
            {
                if (tab == null || !tab.ContainsKey("SalesProjectItemSet")) continue;

                var itemSet = tab["SalesProjectItemSet"] as JObject;
                if (itemSet == null) continue;

                var removed = itemSet["Removed"] as JArray;
                if (removed == null) continue;

                foreach (var row in removed)
                {
                    int id = row["itemset_id"]?.ToObject<int>() ?? 0;
                    if (!requested.Contains(id)) return true;
                }
            }

            return false;
        }

        // ---- Is the user in the middle of something? -------------------------------
        //
        // Three signals, any one of which means "not now":
        //   * an autosave is queued - a change has been made and not yet saved;
        //   * a key was pressed in the last few seconds - they are typing, possibly in a
        //     field whose change event has not fired yet;
        //   * a grid cell is open for editing - its text is not in the grid until they leave.
        private static DateTime _lastKeyUtc = DateTime.MinValue;
        private static bool _keyFilterInstalled;

        private static readonly TimeSpan TypingPause = TimeSpan.FromSeconds(3);

        // Watches key presses application-wide, without consuming any. One per process.
        private sealed class KeyActivityFilter : IMessageFilter
        {
            public bool PreFilterMessage(ref Message m)
            {
                // WM_KEYDOWN, WM_CHAR, WM_SYSKEYDOWN
                if (m.Msg == 0x0100 || m.Msg == 0x0102 || m.Msg == 0x0104)
                    _lastKeyUtc = DateTime.UtcNow;
                return false;
            }
        }

        private static void EnsureKeyActivityFilter()
        {
            if (_keyFilterInstalled) return;
            Application.AddMessageFilter(new KeyActivityFilter());
            _keyFilterInstalled = true;
        }

        private bool IsUserMidEdit()
        {
            EnsureKeyActivityFilter();

            if (_autoSaveTimer != null && _autoSaveTimer.Enabled) return true;
            if (DateTime.UtcNow - _lastKeyUtc < TypingPause) return true;

            for (Control c = LiveViewSnapshot.FocusedControl(FindForm()); c != null; c = c.Parent)
            {
                if (c is DataGridView grid && grid.IsCurrentCellInEditMode) return true;
            }

            return false;
        }

        // ---- Keeping the user's place across a rebuild ------------------------------
        //
        // What the user is looking at, captured before the item-set tabs are torn down and
        // put back after they are rebuilt: the scroll position of every scrolling container
        // above the tabs, the selected tab's own scroll, the items grid's scroll, and which
        // field or cell had focus. Controls are matched by Name inside the rebuilt tab, since
        // the old ones no longer exist.
        private sealed class LiveViewSnapshot
        {
            private readonly List<KeyValuePair<ScrollableControl, Point>> _scrolls = new List<KeyValuePair<ScrollableControl, Point>>();
            private Point _tabScroll;
            private string _focusName;
            private Control _focusOutside;
            private int _gridRow = -1;
            private string _gridColumn;
            private int _gridFirstRow = -1;
            private int _gridHOffset;

            public static LiveViewSnapshot Capture(TabControl tabs)
            {
                var snap = new LiveViewSnapshot();
                if (tabs == null) return snap;

                for (Control c = tabs.Parent; c != null; c = c.Parent)
                {
                    if (c is ScrollableControl sc && sc.AutoScroll)
                        snap._scrolls.Add(new KeyValuePair<ScrollableControl, Point>(sc, sc.AutoScrollPosition));
                }

                TabPage tab = tabs.SelectedTab;
                if (tab != null)
                {
                    snap._tabScroll = tab.AutoScrollPosition;

                    DataGridView items = ItemsGridOf(tab);
                    if (items != null)
                    {
                        snap._gridFirstRow = items.FirstDisplayedScrollingRowIndex;
                        snap._gridHOffset = items.HorizontalScrollingOffset;
                    }
                }

                Control focused = FocusedControl(tabs.FindForm());
                if (focused == null) return snap;

                if (tab != null && IsInside(focused, tab))
                {
                    DataGridView grid = GridOf(focused);
                    if (grid != null)
                    {
                        snap._focusName = grid.Name;
                        if (grid.CurrentCell != null)
                        {
                            snap._gridRow = grid.CurrentCell.RowIndex;
                            snap._gridColumn = grid.CurrentCell.OwningColumn?.Name;
                        }
                    }
                    else
                    {
                        snap._focusName = focused.Name;
                    }
                }
                else if (!IsInside(focused, tabs))
                {
                    // Outside the tabs - it survives the rebuild, so it can simply be refocused.
                    snap._focusOutside = focused;
                }

                return snap;
            }

            public void Restore(TabControl tabs)
            {
                TabPage tab = tabs?.SelectedTab;
                if (tab != null)
                {
                    if (tab.AutoScroll)
                        tab.AutoScrollPosition = new Point(-_tabScroll.X, -_tabScroll.Y);

                    if (!string.IsNullOrEmpty(_focusName))
                    {
                        Control target = tab.Controls.Find(_focusName, true).FirstOrDefault();

                        if (target is DataGridView grid && _gridRow >= 0 && _gridColumn != null
                            && _gridRow < grid.Rows.Count && grid.Columns.Contains(_gridColumn)
                            && grid.Columns[_gridColumn].Visible)
                        {
                            try { grid.CurrentCell = grid.Rows[_gridRow].Cells[_gridColumn]; }
                            catch (InvalidOperationException) { }
                        }

                        if (target != null && target.CanFocus) target.Focus();
                    }

                    // After the focus, which may have scrolled the grid to its current cell.
                    DataGridView items = ItemsGridOf(tab);
                    if (items != null)
                    {
                        if (_gridFirstRow >= 0 && _gridFirstRow < items.Rows.Count)
                        {
                            try { items.FirstDisplayedScrollingRowIndex = _gridFirstRow; }
                            catch (InvalidOperationException) { }
                        }
                        try { items.HorizontalScrollingOffset = _gridHOffset; }
                        catch (InvalidOperationException) { }
                        catch (ArgumentOutOfRangeException) { }
                    }
                }

                if (_focusOutside != null && !_focusOutside.IsDisposed && _focusOutside.CanFocus && !_focusOutside.Focused)
                    _focusOutside.Focus();

                // Last: every focus change above is allowed to scroll, and this undoes it.
                RestoreScroll();
            }

            public void RestoreScroll()
            {
                foreach (var entry in _scrolls)
                {
                    if (!entry.Key.IsDisposed)
                        entry.Key.AutoScrollPosition = new Point(-entry.Value.X, -entry.Value.Y);
                }
            }

            private static DataGridView ItemsGridOf(TabPage tab)
            {
                return tab.Controls.Count > 0 && tab.Controls[0] is ItemSetUC uc ? uc.DgvProjectItems : null;
            }

            private static DataGridView GridOf(Control c)
            {
                for (; c != null; c = c.Parent)
                {
                    if (c is DataGridView grid) return grid;
                }
                return null;
            }

            private static bool IsInside(Control c, Control ancestor)
            {
                for (; c != null; c = c.Parent)
                {
                    if (c == ancestor) return true;
                }
                return false;
            }

            // The control that actually holds focus, following ActiveControl down through
            // nested containers (the page, the item-set control, their panels).
            public static Control FocusedControl(Form form)
            {
                Control active = form?.ActiveControl;
                while (active is ContainerControl container && container.ActiveControl != null)
                    active = container.ActiveControl;
                return active;
            }
        }

        private void NoteOwnSave()
        {
            _ownSaveAt = DateTime.UtcNow;
            _ownSaveEchoPending = true;
        }

        private void ForgetOwnSave()
        {
            _ownSaveEchoPending = false;
        }

        private bool ConsumeOwnSaveEcho()
        {
            if (!_ownSaveEchoPending) return false;

            _ownSaveEchoPending = false;

            // Outside the window this is somebody else's save that merely arrived after ours
            // went missing, so let it through rather than swallowing it.
            return DateTime.UtcNow - _ownSaveAt <= OwnSaveEchoWindow;
        }

        // Derived from the API url rather than read from WssBaseUrl: this page also runs
        // with the engineering assembly hosted inside Sales, where only the API url is
        // mirrored across and WssBaseUrl is left null.
        private static string GetWebSocketBaseUrl()
        {
            string apiBaseUrl = Program.ApiBaseUrl;
            if (string.IsNullOrWhiteSpace(apiBaseUrl)) return null;

            if (apiBaseUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return "wss://" + apiBaseUrl.Substring("https://".Length);
            if (apiBaseUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                return "ws://" + apiBaseUrl.Substring("http://".Length);
            return apiBaseUrl;
        }

        // Safe to call on every load: it no-ops when already listening to the same project,
        // and never throws out to its caller - a socket that cannot connect just means this
        // screen keeps working the way it did before, on Refresh.
        private async void EnsureSaveNotifyConnected(int projectId)
        {
            await ConnectSaveNotifyAsync(projectId);
        }

        // True when the channel for this quotation is open afterwards.
        private async Task<bool> ConnectSaveNotifyAsync(int projectId)
        {
            if (projectId <= 0) return false;
            if (_saveNotifyProjectId == projectId
                && _saveNotifyWebSocket?.State == System.Net.WebSockets.WebSocketState.Open) return true;

            DisconnectSaveNotify();

            string wsBase = GetWebSocketBaseUrl();
            if (string.IsNullOrWhiteSpace(wsBase)) return false;

            _saveNotifyProjectId = projectId;
            var cts = new System.Threading.CancellationTokenSource();
            var socket = new System.Net.WebSockets.ClientWebSocket();
            _saveNotifyCts = cts;
            _saveNotifyWebSocket = socket;

            try
            {
                // /ws is behind login and reads the token from ?Authorization= on the
                // upgrade, the same way Sales connects to this channel.
                string token = Uri.EscapeDataString(CacheData.SessionToken ?? "");
                var uri = new Uri(wsBase + "/ws/setup/test?branch=Sales&projectid=" + projectId + "&Authorization=" + token);
                await socket.ConnectAsync(uri, cts.Token);
                _ = ListenForSaveNotificationsAsync(socket, cts, projectId);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Live update channel unavailable: " + ex.Message);
                return false;
            }
        }

        private async Task ListenForSaveNotificationsAsync(System.Net.WebSockets.ClientWebSocket socket,
                                                           System.Threading.CancellationTokenSource cts,
                                                           int projectId)
        {
            byte[] buffer = new byte[64 * 1024];
            try
            {
                while (socket.State == System.Net.WebSockets.WebSocketState.Open)
                {
                    var message = new List<byte>();
                    System.Net.WebSockets.WebSocketReceiveResult result;
                    do
                    {
                        result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
                        message.AddRange(buffer.Take(result.Count));
                    } while (!result.EndOfMessage);

                    if (result.MessageType != System.Net.WebSockets.WebSocketMessageType.Text) continue;

                    JToken token;
                    try { token = JToken.Parse(System.Text.Encoding.UTF8.GetString(message.ToArray())); }
                    catch { continue; }

                    string evt = token["event"]?.ToString();

                    // Sales pressed EDIT: the quotation now continues on a new sub-version.
                    if (evt == "quotation_superseded")
                    {
                        int newId = token["new_id"]?.ToObject<int>() ?? 0;
                        string code = token["code"]?.ToString();
                        if (IsHandleCreated)
                            Invoke(new Action(() => HandleQuotationSuperseded(projectId, newId, code)));
                        continue;
                    }

                    if (evt != "quotation_saved") continue;

                    if (IsHandleCreated)
                        Invoke(new Action(() => HandleQuotationSaved(projectId)));
                }
            }
            catch (Exception)
            {
                // Dropped connection - reconnected below.
            }

            // Closed or dropped, but not by this page (DisconnectSaveNotify cancels cts when the
            // quotation changes or the page goes): get it back.
            if (!cts.IsCancellationRequested && !IsDisposed)
                ReconnectSaveNotifyLater(projectId);
        }

        // The channel dropped on its own - the API restarted, the network blinked, a proxy closed
        // a connection it took for idle. It used to stay down until this page loaded again - on
        // Refresh, or after one of its own saves - so an engineer only looking at a quotation
        // stopped seeing Sales's changes without being told (2026-09-29 sync audit). Reconnects
        // with a growing pause (2 s up to 30 s) while this quotation stays open, then catches up
        // on what was saved meanwhile.
        private bool _saveNotifyReconnecting;

        private async void ReconnectSaveNotifyLater(int projectId)
        {
            if (_saveNotifyReconnecting) return;
            _saveNotifyReconnecting = true;
            try
            {
                for (int attempt = 1; ; attempt++)
                {
                    await Task.Delay(Math.Min(30000, 2000 * attempt));
                    if (IsDisposed || _quotationId != projectId) return;
                    if (await ConnectSaveNotifyAsync(projectId))
                    {
                        QueueRemoteRefresh();
                        return;
                    }
                }
            }
            finally
            {
                _saveNotifyReconnecting = false;
            }
        }

        // ---- Versioning (user decision 2026-09-25) -------------------------------------------
        //
        // EDIT in Sales starts the quotation's next sub-version: a copy, Q#2026-0001.2.0 ->
        // Q#2026-0001.2.1, which is where all work continues. The copy this page has open
        // becomes history and the API refuses saves to it, so this page FOLLOWS the new copy
        // (the user's choice: "Follow to the newest") - told either by the socket, or by a save
        // of its own being refused because the copy was replaced.

        private async void HandleQuotationSuperseded(int projectId, int newId, string code)
        {
            try
            {
                if (projectId != _quotationId) return;
                await FollowNewerCopyAsync(newId, code);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Following the newer copy failed: " + ex.Message);
            }
        }

        private async Task FollowNewerCopyAsync(int newId, string code)
        {
            if (newId <= 0 || newId == _quotationId) return;

            _autoSaveTimer?.Stop();
            _quotationId = newId;
            if (!string.IsNullOrEmpty(code)) txt_doc_no.Text = code;

            _applyingRemoteUpdate = true;
            try { await LoadQuotationAsync(silent: true); }
            finally { _applyingRemoteUpdate = false; }

            if (_quotation != null && !string.IsNullOrEmpty(_quotation.quote_code))
                txt_doc_no.Text = _quotation.quote_code;

            lbl_saving_status.Text = "Sales started " + (string.IsNullOrEmpty(txt_doc_no.Text) ? "a new sub-version" : txt_doc_no.Text) + " - now showing it";
        }

        // "Q#2026-0001.2.0 has been replaced by Q#2026-0001.2.1 (id 57) - ..." - the API's
        // refusal of a save to a copy that is no longer the newest.
        private static bool TryParseReplacement(string message, out int id, out string code)
        {
            id = 0;
            code = null;
            if (string.IsNullOrEmpty(message)) return false;

            var match = System.Text.RegularExpressions.Regex.Match(message, @"replaced by (\S+) \(id (\d+)\)");
            if (!match.Success || !int.TryParse(match.Groups[2].Value, out id) || id <= 0) return false;

            code = match.Groups[1].Value;
            return true;
        }

        private void HandleQuotationSaved(int projectId)
        {
            try
            {
                if (projectId != _quotationId) return;

                // Our own save's echo. The API broadcasts to every socket on the project
                // channel including the one that saved, so without this, typing here means:
                // autosave -> broadcast -> tear down and rebuild every tab from data we just
                // sent ourselves, every couple of seconds. That is the reload the user sees
                // with both apps open, and a spreadsheet plainly does not do it.
                //
                // The message carries no sender, so this screen recognises its own echo by
                // having just produced it - one notification per save it made, inside a
                // short window. If Sales saved in that same instant we consume theirs
                // instead; their next save, or the next load, brings it in.
                if (ConsumeOwnSaveEcho()) return;

                // Collapsed into one refresh once Sales pauses, rather than one per
                // notification. Sales autosaves every two seconds while somebody types, so
                // acting on each notification meant rebuilding every tab on this screen every
                // two seconds for one piece of work on theirs.
                QueueRemoteRefresh();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Live update failed: " + ex.Message);
            }
        }

        private void DisconnectSaveNotify()
        {
            try { _saveNotifyCts?.Cancel(); } catch { }
            try { _saveNotifyWebSocket?.Dispose(); } catch { }
            _saveNotifyWebSocket = null;
            _saveNotifyCts = null;
            _saveNotifyProjectId = 0;
        }

        // Restarted on every change, so the save lands once the engineer stops, not per
        // keystroke. Never while a remote update is being applied - those rebinds raise the
        // same change events, and autosaving them would echo the other side's work straight
        // back at them.
        private bool CanAutoSave()
        {
            if (_applyingRemoteUpdate || _loadingQuotation) return false;
            if (_quotationId <= 0 || _fetchedData == null || _quotation == null) return false;

            if (tab_container == null) return false;

            // The item-set tabs - every tab but "+".
            List<TabPage> pages = tab_container.TabPages.Cast<TabPage>().Where(p => !IsAddTab(p)).ToList();
            if (pages.Count == 0) return false;

            // One tab per item set the fetch returned - less the ones removed here on purpose -
            // each holding its control. Fewer means the page is part-built - a load that threw
            // after the fetch but before or during BuildTabs - and saving that sends tabs with
            // nothing in them, which the id-keyed diff reads as "delete the real rows".
            var storedIds = new HashSet<int>((_fetchedData.sales_project_item_set ?? new List<SalesProjectItemSet>())
                .Where(x => x.based_id == _quotationId)
                .Select(x => (int)x.itemset_id));

            int expected = storedIds.Count - _removedItemSetIds.Count(storedIds.Contains);
            int existing = pages.Count(p => !_newTabs.Contains(p));

            if (storedIds.Count == 0 || existing != expected) return false;

            foreach (TabPage tab in pages)
            {
                if (!(tab.Controls.Count > 0 && tab.Controls[0] is ItemSetUC uc)) return false;

                // A tab whose items grid has no columns at all has not bound its data yet.
                // Counting tabs alone cannot tell "the engineer emptied this tab" from "this
                // tab never loaded", and the id-keyed diff reads the second as delete-the-real
                // -rows. Same guard Sales uses (ProjectTabsLookLoaded).
                if (!uc.HasLoadedItems()) return false;
            }

            return true;
        }

        // Set for the whole of a load, so an autosave cannot fire into the middle of one -
        // including a load that threw part-way.
        private bool _loadingQuotation;

        private void QueueAutoSave()
        {
            if (!CanAutoSave()) return;

            if (_autoSaveTimer == null)
            {
                _autoSaveTimer = new System.Windows.Forms.Timer { Interval = AutoSaveIdleMs };
                _autoSaveTimer.Tick += AutoSaveTimer_Tick;
            }

            _autoSaveTimer.Stop();
            _autoSaveTimer.Start();
        }

        private async void AutoSaveTimer_Tick(object sender, EventArgs e)
        {
            await RunAutoSaveAsync();
        }

        // Whether this page holds edits not saved yet: a save queued, running or waiting to be
        // retried, or a change still inside an Item/Set control's short debounce. The main
        // window asks before it closes (FlushAutoSaveAsync).
        public bool HasUnsavedEdits =>
            CanAutoSave() && ((_autoSaveTimer?.Enabled ?? false) || _autoSaving || _autoSaveRetries > 0
                || _tabControls.Any(uc => uc != null && !uc.IsDisposed && uc.HasPendingChangeEvents));

        // Saves what is waiting, now, and returns once it is done. Closing the app used to drop
        // the last second or two of edits and any save waiting to be retried (2026-09-29).
        public async Task FlushAutoSaveAsync()
        {
            for (int i = 0; i < 100 && _autoSaving; i++)
                await Task.Delay(100);
            if (!CanAutoSave()) return;
            await RunAutoSaveAsync();
        }

        private async Task RunAutoSaveAsync()
        {
            _autoSaveTimer?.Stop();

            if (_autoSaving) return;
            if (!CanAutoSave())
            {
                // A tab just added with "+" is still loading its lists; its save waits for it
                // rather than being dropped.
                if (!_loadingQuotation && _newTabs.Count > 0) _autoSaveTimer.Start();
                return;
            }

            _autoSaving = true;
            try
            {
                lbl_saving_status.Text = "Saving...";

                // What this save is measured against - the merge that follows it needs the same.
                SalesProjectList savedFrom = _fetchedData;

                bool hasInserts;
                Dictionary<string, dynamic> changes = BuildSaveChanges(out hasInserts);

                // Nothing to send: no save, and no "look again" for Sales either.
                if (Quotation.DiffIsEmpty(changes))
                {
                    lbl_saving_status.Text = "";
                    return;
                }

                // Marked BEFORE the request, not after. The API commits, broadcasts, and only
                // then answers, so our own echo can reach the socket before this await
                // returns - and an echo that arrives before it has been marked is not
                // recognised as ours, which is exactly the rebuild this is here to stop.
                NoteOwnSave();

                // Last line of defence. Removing a tab is something a person does on purpose
                // (right-click > Remove Tab); there is no circumstance in which a save nobody
                // asked for should delete one, so if the diff says it would, the save is wrong -
                // abandon it and reload rather than let it through. Costs one autosave; the
                // alternative cost is the item set and everything keyed to it.
                if (DiffRemovesUnrequestedItemSet(changes, _removedItemSetIds))
                {
                    ForgetOwnSave();
                    lbl_saving_status.Text = "Not saved - refreshing";
                    Console.WriteLine("Autosave aborted: the diff would have removed an item set.");
                    QueueRemoteRefresh();
                    return;
                }

                var service = new GeneralService<SalesProjectList>(ApiEndPoints.SALES_PROJECTS);
                var response = await service.Update(changes);

                if (response != null && response.success)
                {
                    lbl_saving_status.Text = "Saved " + DateTime.Now.ToString("HH:mm");
                    _autoSaveRetries = 0;

                    // Written; nothing left to request.
                    _removedItemSetIds.Clear();

                    // Not simply "take the stored project": it can hold a change Sales saved while
                    // this one was on its way, which this page does not show yet - and a baseline
                    // claiming otherwise made the NEXT save undo it. Merged instead, against what
                    // this save was diffed from; new rows take the ids the server gave them. A
                    // tab added or removed changes the shape, and that is a rebuild.
                    if (!await MergeRemoteChangesAsync(savedFrom, adoptNewRowIds: true))
                    {
                        _applyingRemoteUpdate = true;
                        try { await LoadQuotationAsync(silent: true); }
                        finally { _applyingRemoteUpdate = false; }
                    }

                    // Lines saved just now can take the reservations ticked on them before.
                    await ApplyPendingReservationsAsync();
                }
                else
                {
                    // Nothing was written, so nothing will be broadcast: stop waiting for an
                    // echo that is never coming, or it would swallow the next real update.
                    ForgetOwnSave();
                    lbl_saving_status.Text = "Not saved - it will try again";

                    // Refused for good: Sales made a newer copy. Retrying would fail every time.
                    if (TryParseReplacement(response?.message, out int newerId, out string newerCode))
                        await FollowNewerCopyAsync(newerId, newerCode);
                    else
                        ScheduleAutoSaveRetry();
                }
            }
            catch (Exception ex)
            {
                ForgetOwnSave();
                lbl_saving_status.Text = "Not saved - it will try again";
                Console.WriteLine("Autosave failed: " + ex.Message);
                ScheduleAutoSaveRetry();
            }
            finally
            {
                _autoSaving = false;
            }
        }

        // A failed autosave is tried again on its own, sooner at first and then every minute
        // (2026-09-29 sync audit). It used to wait for the engineer's next edit, so the last
        // change made stayed unsaved and never reached Sales while the status said "it will try
        // again". Each retry sends everything still unsaved: the baseline only moves on success.
        private int _autoSaveRetries;

        private async void ScheduleAutoSaveRetry()
        {
            int attempt = ++_autoSaveRetries;
            await Task.Delay(Math.Min(60000, 5000 * attempt));
            // Superseded by a newer failure's retry, or a save has since gone through.
            if (IsDisposed || attempt != _autoSaveRetries) return;
            QueueAutoSave();
        }

        // The save payload, built once and used by both the Save button and the autosave.
        // Extracted rather than copied: two builds of this that drift apart is exactly how a
        // save starts writing something different from what the screen shows.
        private Dictionary<string, dynamic> BuildSaveChanges()
        {
            bool ignored;
            return BuildSaveChanges(out ignored);
        }

        // hasInserts - the payload carries at least one row with no primary key yet, so the
        // server is about to issue ids this screen does not have. That is the one case where
        // a save has to be followed by a real rebuild: leave the grid holding 0 and the next
        // diff reads the same row as new again and inserts a duplicate. Everything else - a
        // changed value, a deleted row - leaves the screen already correct, so it can be
        // refreshed without being torn down.
        private Dictionary<string, dynamic> BuildSaveChanges(out bool hasInserts)
        {
            Dictionary<string, object> pnlQuotation = BuildSavePayload(out hasInserts);

            // Reuses Quotation.cs's own diff engine rather than re-implementing it -
            // GetFullDiff is a public method with no dependency beyond its two
            // parameters (confirmed by reading its body and every helper it calls
            // before relying on this), so a throwaway, immediately-disposed instance
            // is the safest way to guarantee this page's save produces byte-for-byte
            // the same diff shape Sales's own save would for an identical before/after
            // state - not a hand-rolled approximation of it.
            Dictionary<string, dynamic> changes;
            using (var diffEngine = new Quotation())
            {
                changes = diffEngine.GetFullDiff(_fetchedData, pnlQuotation);
            }

            changes["id"] = _quotationId;
            return changes;
        }

        // What a save from this page sends, before it is diffed: the header and multipliers as
        // fetched, every tab as it is on screen. Also where ProjectLiveMerge reads "what the
        // engineer has changed and not saved yet" from.
        // includeTotals: false for the live merge, which reads only the tabs from this.
        private Dictionary<string, object> BuildSavePayload(out bool hasInserts, bool includeTotals = true)
        {
            hasInserts = false;

                        var pnlQuotation = JObject.FromObject(_quotation).ToObject<Dictionary<string, object>>();

                        // The totals worked out from the items here (see RecomputeTotals) - the one
                        // part of the header this screen sends changed.
                        if (includeTotals && _totalsEdited)
                        {
                            pnlQuotation["gross_sales"] = (double)Math.Round(_gross, 2);
                            pnlQuotation["vat_amount"] = (double)Math.Round(_vat, 2);
                            pnlQuotation["net_sales"] = (double)Math.Round(_net, 2);
                            pnlQuotation["percent_discount"] = (double)Math.Round(_percent, 2);
                            pnlQuotation["net_amount_due"] = (double)Math.Round(_netAmountDue, 2);
                            pnlQuotation["total_amount_due"] = (double)Math.Round(_totalAmountDue, 2);
                        }

                        // As the grid holds them - editable here since 2026-09-28.
                        pnlQuotation["sales_project_multiplier"] = ScreenMultipliers();

                        // The tabs removed here on purpose - the only item sets this save may delete.
                        pnlQuotation["removed_item_set_ids"] = _removedItemSetIds.ToList();

                        var allTabsData = new List<Dictionary<string, object>>();

                        foreach (TabPage tabPage in tab_container.TabPages)
                        {
                            if (!(tabPage.Controls.Count > 0 && tabPage.Controls[0] is ItemSetUC uc)) continue;

                            int itemSetId = ItemSetIdOf(tabPage);
                            bool isNewTab = _newTabs.Contains(tabPage);
                            if (isNewTab) hasInserts = true;

                            // Built exactly the way Sales's own project save builds it
                            // (Quotation.cs, IsProject): every tab collection is read from the
                            // tab's own controls. Only the HEADER differs - quotation fields and
                            // multipliers still ride back as fetched, since this screen has no UI
                            // for them.
                            //
                            // User decision, 2026-09-04: an engineer may edit ALL of it, not just
                            // Size Up / item table / wiring. The reason is practical - sales
                            // regularly hands the quote over with client-needs fields left blank,
                            // and the engineer is the one who knows them. Spec §3.2/§5.1 updated to
                            // match; this is a deliberate override, not a drift.
                            //
                            // The previous version reverted every non-editable scalar to its
                            // fetched value to enforce §3.2. That is what made a save look like it
                            // worked while changing nothing: the diff compared identical before and
                            // after, found no changes, sent no tab, and the API answered 200
                            // without a single write.
                            var contentForSave = uc.GetProjectContentsData();
                            var conditionsForSave = uc.GetAdvancedConditionsData();

                            // Identity safety net, kept from the FK-error fix: these two ids decide
                            // whether the server UPDATES the existing row or treats it as a new one,
                            // and an id-keyed diff reads a 0 as "delete the real row, insert a
                            // blank". If the panel didn't supply one, fall back to the record that
                            // actually exists rather than letting that happen.
                            // A new tab has no stored rows to fall back on.
                            var fetchedContent = isNewTab ? null : _fetchedData.sales_project_content?
                                .FirstOrDefault(c => c.based_id == itemSetId);
                            if (fetchedContent != null && contentForSave != null
                                && ToId(contentForSave, "content_id") == 0)
                                contentForSave["content_id"] = fetchedContent.content_id;

                            var fetchedCondition = isNewTab ? null : _fetchedData.sales_project_content_advanced_condition?
                                .FirstOrDefault(c => c.based_id == itemSetId);
                            if (fetchedCondition != null && conditionsForSave != null
                                && ToId(conditionsForSave, "conditions_id") == 0)
                                conditionsForSave["conditions_id"] = fetchedCondition.conditions_id;

                            var itemsForSave = uc.GetProjectItems()["sales_project_items"] as List<SalesProjectItems>;
                            var wiringForSave = uc.GetProjectWiringData()["sales_project_wiring"] as List<SalesWiringModel>;

                            if (itemsForSave != null && itemsForSave.Any(x => x.items_id == 0)) hasInserts = true;
                            if (wiringForSave != null && wiringForSave.Any(x => x.id == 0)) hasInserts = true;

                            var tabData = new Dictionary<string, object>
                            {
                                // A new tab goes as Sales sends one: placeholder id 0, flagged new.
                                ["sales_project_item_set"] = new Dictionary<string, object>
                                {
                                    { "based_id", _quotationId },
                                    { "tab_number", tabPage.Text },
                                    { "itemset_id", isNewTab ? 0 : itemSetId },
                                    { "is_new_tab", isNewTab }
                                },
                                // History is the one thing still echoed back untouched - it is an
                                // audit trail, generated from the diff itself, never typed.
                                ["sales_project_history"] = isNewTab
                                    ? new List<SalesProjectHistory>()
                                    : (_fetchedData.sales_project_history ?? new List<SalesProjectHistory>())
                                        .Where(h => h.based_id == itemSetId)
                                        .ToList(),
                                ["sales_project_content"] = contentForSave,
                                ["sales_project_content_advanced_condition"] = conditionsForSave,
                                // These two are the real, intended edits - read from the UI.
                                ["sales_project_items"] = (object)itemsForSave ?? uc.GetProjectItems()["sales_project_items"],
                                ["sales_project_wiring"] = (object)wiringForSave ?? uc.GetProjectWiringData()["sales_project_wiring"]
                            };

                            allTabsData.Add(tabData);
                        }

                        pnlQuotation["sales_project_all_tabs"] = allTabsData;
                        pnlQuotation["id"] = _quotationId;

            return pnlQuotation;
        }

        // Reads the stored project and puts it on screen in place (ProjectLiveMerge), with
        // `baseline` as what the screen was built from: what Sales changed lands field by field
        // and row by row, around whatever the engineer is doing, with no reload. False when the
        // shape changed (a tab added, removed or renamed) and the page has to be rebuilt.
        // Whether the last in-place merge put anything on screen - a poll or a catch-up after a
        // reconnect usually finds nothing, and "Updated by Sales" then is false.
        private bool _lastMergeChangedScreen;

        private async Task<bool> MergeRemoteChangesAsync(SalesProjectList baseline, bool adoptNewRowIds = false)
        {
            _lastMergeChangedScreen = false;
            int id = _quotationId;
            if (id <= 0 || baseline == null || _quotation == null) return false;

            var service = new GeneralService<SalesProjectList>(ApiEndPoints.SALES_PROJECTS + "/detail/" + id);
            SalesProjectList fresh = await service.GetAsModel();
            if (fresh?.SalesQuotation == null || !fresh.SalesQuotation.Any()) return true;

            // Another quotation opened, or a load started, while the request was out.
            if (_quotationId != id || _loadingQuotation) return true;

            var tabs = new Dictionary<int, ItemSetUC>();
            foreach (TabPage page in tab_container.TabPages)
            {
                if (page.Controls.Count > 0 && page.Controls[0] is ItemSetUC uc
                    && int.TryParse(page.Tag?.ToString(), out int itemSetId) && itemSetId > 0)
                    tabs[itemSetId] = uc;
            }

            // Rows this page created carry id 0 until it is told the ids the server issued;
            // left at 0, the next save would insert them a second time.
            if (adoptNewRowIds)
            {
                foreach (var tab in tabs)
                {
                    tab.Value.AdoptServerIds(
                        (fresh.sales_project_items ?? new List<SalesProjectItems>()).Where(i => i.based_id == tab.Key),
                        (fresh.sales_project_wiring ?? new List<SalesWiringModel>()).Where(w => w.based_id == tab.Key));
                }
            }

            ProjectLiveMerge.Outcome outcome;
            bool multipliersUntouched;
            _applyingRemoteUpdate = true;
            try
            {
                // Has the engineer edited the multipliers since the baseline? A cell still open
                // counts - it is committed first.
                if (_multiplierGrid != null && !_multiplierGrid.IsDisposed && _multiplierGrid.IsCurrentCellInEditMode)
                    _multiplierGrid.EndEdit();
                multipliersUntouched = ProjectLiveMerge.SameMultipliers(ScreenMultipliers(),
                    ProjectLiveMerge.StoredMultipliers(baseline, id));

                Dictionary<string, object> payload = BuildSavePayload(out _, includeTotals: false);
                using (var diffEngine = new Quotation())
                {
                    outcome = ProjectLiveMerge.Apply(diffEngine, baseline, fresh, payload, id, tabs);
                }
            }
            finally
            {
                _applyingRemoteUpdate = false;
            }

            if (outcome.NeedsRebuild) return false;

            _fetchedData = outcome.Baseline;

            // The header this page saves from has to be the stored one - see BuildSavePayload.
            // Sales edits it; this page shows it.
            if (outcome.HeaderChanged)
            {
                _quotation = fresh.SalesQuotation.FirstOrDefault(q => q.id == id) ?? _quotation;
                txt_project_name.Text = _quotation.project_name;
                ShowHeader();
            }

            // The other app's multipliers, unless the engineer has changed them here and not
            // saved - then theirs stay, and the next save sends them (last to save wins, as
            // with a field).
            if (outcome.MultipliersChanged && multipliersUntouched)
            {
                RefreshMultiplierGrid();
                List<string> choices = MultiplierChoices();
                foreach (var uc in _tabControls)
                {
                    if (uc != null && !uc.IsDisposed) uc.setMultiplier(choices);
                }
            }

            // Item lines changed there: this screen's own totals, if it is working them out.
            if (outcome.Applied > 0 && _totalsEdited) RecomputeTotals();

            _lastMergeChangedScreen = outcome.Applied > 0 || outcome.HeaderChanged
                || (outcome.MultipliersChanged && multipliersUntouched);

            RefreshHistoryGrid(CurrentItemSets());

            // A grid was open for typing: its part waits for the next pass.
            if (outcome.Deferred.Count > 0) QueueRemoteRefresh();

            return true;
        }

        private async void btn_save_Click(object sender, EventArgs e)
        {
            if (_fetchedData == null || _quotation == null) return;

            Helpers.Loading.ShowLoading(this);
            try
            {
                try
                {
                    btn_save.Enabled = false;
                    lbl_saving_status.Text = "Saving...";

                    Dictionary<string, dynamic> changes = BuildSaveChanges();

                    // The reload below already brings this screen up to date, so the
                    // broadcast this save causes would only rebuild every tab a second time.
                    // Marked before the request for the same reason as the autosave's: the
                    // echo can reach the socket before this await returns.
                    NoteOwnSave();

                    var service = new GeneralService<SalesProjectList>(ApiEndPoints.SALES_PROJECTS);
                    var response = await service.Update(changes);

                    if (response == null || !response.success) ForgetOwnSave();

                    if (response != null && response.success)
                    {
                        lbl_saving_status.Text = "Saved.";
                        Helpers.ShowDialogMessage("success", "Quotation updated successfully.");
                        await LoadQuotationAsync();
                        await ApplyPendingReservationsAsync();
                    }
                    else if (TryParseReplacement(response?.message, out int newerId, out string newerCode))
                    {
                        lbl_saving_status.Text = "Save failed.";
                        Helpers.ShowDialogMessage("error", response.message +
                            "\n\nThe page now shows the current copy - make the change there.");
                        await FollowNewerCopyAsync(newerId, newerCode);
                    }
                    else
                    {
                        lbl_saving_status.Text = "Save failed.";
                        Helpers.ShowDialogMessage("error", string.IsNullOrWhiteSpace(response?.message)
                            ? "Failed to save the quotation."
                            : response.message);
                    }
                }
                catch (Exception ex)
                {
                    lbl_saving_status.Text = "Save failed.";
                    Helpers.ShowDialogMessage("error", $"Failed to save: {ex.Message}");
                }
                finally
                {
                    btn_save.Enabled = true;
                }
            }
            finally
            {
                Helpers.Loading.HideLoading(this);
            }
        }
    }
}
