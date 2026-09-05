using System;
using System.Collections.Generic;
using System.Data;
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
    // What an engineer may change here (user decision, 2026-09-04): EVERYTHING on a tab -
    // client needs, item/set description and notes, advanced conditions, Size Up, Final
    // Selection, the item table and wiring. The reason is practical: sales routinely hands
    // a quote over with those fields blank, and the engineer is the one who knows them.
    // Spec §3.2/§5.1 were updated to match - this is a deliberate override of the old
    // "view-only outside three areas" rule, not drift.
    //
    // The only thing this screen does NOT send is the HEADER - quotation fields and
    // multipliers ride back exactly as fetched, because there is no UI for them here.
    // Everything else is built the same way Sales's own project save builds it
    // (Quotation.cs, IsProject), through the same ItemSetUC getters, so both apps produce
    // the same payload shape and the server's dedupe/prune of Size Up and Final applies
    // identically.
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
        private DataTable _pumpItemsCache;

        public SalesQuotationEngPage()
        {
            InitializeComponent();
        }

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

        private async Task LoadQuotationAsync()
        {
            if (_quotationId <= 0) return;

            try
            {
                btn_save.Enabled = false;
                btn_refresh.Enabled = false;
                lbl_saving_status.Text = "Loading...";

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

                var service = new GeneralService<SalesProjectList>(ApiEndPoints.SALES_PROJECTS);
                _fetchedData = await service.GetAsModel();

                if (_fetchedData == null)
                {
                    Helpers.ShowDialogMessage("error", "Failed to load quotation data.");
                    return;
                }

                _quotation = _fetchedData.SalesQuotation?.FirstOrDefault(q => q.id == _quotationId);
                if (_quotation == null)
                {
                    Helpers.ShowDialogMessage("error", "This quotation could not be found.");
                    return;
                }

                txt_project_name.Text = _quotation.project_name;

                BuildTabs();
                lbl_saving_status.Text = "Loaded.";
            }
            catch (Exception ex)
            {
                Helpers.ShowDialogMessage("error", $"Failed to load quotation: {ex.Message}");
            }
            finally
            {
                btn_save.Enabled = true;
                btn_refresh.Enabled = true;
            }
        }

        private void BuildTabs()
        {
            tab_container.TabPages.Clear();
            _tabControls.Clear();

            var tabs = (_fetchedData.sales_project_item_set ?? new List<SalesProjectItemSet>())
                .Where(t => t.based_id == _quotationId)
                .ToList();

            var multiplierChoices = (_fetchedData.sales_project_multiplier ?? new List<SalesProjectMultiplier>())
                .Where(m => m.based_id == _quotationId)
                .Select(m => m.multiplier)
                .Where(m => !string.IsNullOrWhiteSpace(m))
                .Distinct()
                .ToList();

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
                // and re-applies the saved template as soon as its fetches return - which
                // clears the items grid and appends another wiring block - unless this
                // flag is already set. Setting it here rather than relying on
                // SetFetchedItemData (called further down, after Controls.Add has already
                // started the load) is what makes that deterministic instead of a race.
                uc.MarkAsExistingRecord();

                uc.SizeUpClicked += async (s, e) => await OnSizeUpClicked(uc);
                uc.FinalTxtBoxClicked += async (s, e) => await OnFinalClicked(uc);

                // Add an item / accessory / BOM to the item table (user-reported gap,
                // 2026-09-04: "can't add accessories like the project quotation can").
                // Sales subscribes this same event to the same logic; this page simply
                // never subscribed, so clicking the add cell did nothing at all.
                uc.CellClicked += (s, e) => OnAddItemClicked(uc);
                uc.CellClickedModel += (s, e) => Helpers.ShowDialogMessage(
                    "info",
                    "Swapping an item's model isn't available from this screen yet - use the Sales app if a substitution is genuinely needed.");

                uc.setMultiplier(multiplierChoices);

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

            // Multiplier and Change History (user-reported gap, 2026-09-03): spec says
            // "pricing structure, multipliers, quote terms" stay view-only to an engineer -
            // that means visible-but-locked, not absent. Neither was ever wired into this
            // page before. Per user decision, these sit in pnl_multiplier_history above
            // tab_container instead of being tabs themselves - same idea as Sales's own
            // Quotation.cs, where dgv_project_multiplier is a sibling of tabControl2, always
            // visible regardless of which item-set tab is selected.
            RefreshMultiplierGrid();
            RefreshHistoryGrid(tabs);
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
        private void RefreshMultiplierGrid()
        {
            grp_multiplier.Controls.Clear();

            var multipliers = (_fetchedData.sales_project_multiplier ?? new List<SalesProjectMultiplier>())
                .Where(m => m.based_id == _quotationId)
                .ToList();

            var dgv = BuildReadOnlyGrid();
            dgv.DataSource = Helpers.ToDataTable(multipliers);

            if (dgv.Columns["multiplier_id"] != null) dgv.Columns["multiplier_id"].Visible = false;
            if (dgv.Columns["based_id"] != null) dgv.Columns["based_id"].Visible = false;
            if (dgv.Columns["brand"] != null) dgv.Columns["brand"].HeaderText = "BRAND";
            if (dgv.Columns["component"] != null) dgv.Columns["component"].HeaderText = "COMPONENT";
            if (dgv.Columns["description"] != null) dgv.Columns["description"].HeaderText = "DESCRIPTION";
            if (dgv.Columns["multiplier"] != null) dgv.Columns["multiplier"].HeaderText = "MULTIPLIER";

            grp_multiplier.Controls.Add(dgv);
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
            grp_history.Controls.Clear();

            var tabNumberByItemSetId = tabs.ToDictionary(t => t.itemset_id, t => t.tab_number);

            var history = (_fetchedData.sales_project_history ?? new List<SalesProjectHistory>())
                .Where(h => tabNumberByItemSetId.ContainsKey((int)h.based_id))
                .OrderByDescending(h => h.history_id)
                .Select(h => new
                {
                    TAB = tabNumberByItemSetId[(int)h.based_id],
                    USER = h.user,
                    DATE = h.date,
                    TIME = h.time,
                    OLD_DATA = h.old_data,
                    NEW_DATA = h.new_data
                })
                .ToList();

            var dgv = BuildReadOnlyGrid();
            dgv.DataSource = Helpers.ToDataTable(history);

            if (dgv.Columns["OLD_DATA"] != null) dgv.Columns["OLD_DATA"].HeaderText = "OLD DATA";
            if (dgv.Columns["NEW_DATA"] != null) dgv.Columns["NEW_DATA"].HeaderText = "NEW DATA";

            grp_history.Controls.Add(dgv);
        }

        // Reads an id out of one of the ItemSetUC payload dictionaries. The values come
        // from panel controls, so an id can arrive as int, long, string or be missing
        // entirely; anything unparseable counts as 0 ("no id supplied").
        private static int ToId(Dictionary<string, dynamic> data, string key)
        {
            if (data == null || !data.ContainsKey(key) || data[key] == null) return 0;
            return int.TryParse(data[key].ToString(), out int id) ? id : 0;
        }

        private async Task<DataTable> GetPumpItemsAsync()
        {
            if (_pumpItemsCache != null) return _pumpItemsCache;

            var service = new GeneralService<EngineeringItems>(ApiEndPoints.SETUP_ITEM_FULL);
            var data = await service.GetAsModel();
            var pumpItems = (data?.items ?? new List<EngineeringItemModel>())
                .Where(i => string.Equals(i.item_name, "PUMP", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var table = new DataTable();
            table.Columns.Add("id", typeof(int));
            table.Columns.Add("item_brand", typeof(string));
            table.Columns.Add("item_model", typeof(string));
            foreach (var item in pumpItems)
                table.Rows.Add(item.id, item.item_brand ?? string.Empty, item.item_model ?? string.Empty);

            _pumpItemsCache = table;
            return table;
        }

        // Opens the item/BOM picker for the clicked row and inserts the choice.
        //
        // Mirrors Sales's Cell_ClickedUC: read the clicked row off the control, reset the
        // per-tab parent counter, and hand the control's own grid to the shared editor.
        // The counter reset matters because reference numbering is per item set - without
        // it, adding on one tab then another continues the first tab's numbering.
        private void OnAddItemClicked(ItemSetUC uc)
        {
            if (uc == null) return;

            if (_itemGridEditor == null)
            {
                Helpers.ShowDialogMessage("error",
                    "The item list hasn't loaded yet. Use Refresh, then try again.");
                return;
            }

            _itemGridEditor.CounterParent = 1;
            _itemGridEditor.HandleItemSelectionClick(uc.GetIndex(), uc.DgvProjectItems);
        }

        private async Task OnSizeUpClicked(ItemSetUC uc)
        {
            var pumpItems = await GetPumpItemsAsync();
            if (pumpItems.Rows.Count == 0)
            {
                Helpers.ShowDialogMessage("error", "No pump items are set up yet.");
                return;
            }

            using (var modal = new SizeUpPickerModal(pumpItems, uc.GetSizeUpItemIds()))
            {
                // SizeUpPickerModal is multi-select internally (checkboxes per row) but
                // exposes its picks the same way Quotation.cs consumes them - reflectively,
                // since its own selection surface isn't a simple public property (matches
                // how Quotation.cs's own SizeUpClicked handler reads it back, one
                // AddSizeUpRow call per row the user checked).
                if (modal.ShowDialog() != DialogResult.OK) return;

                foreach (var selection in modal.GetSelectedItems())
                    uc.AddSizeUpRow(selection.ItemId.ToString(), selection.Model);

                // Kept as a plain repaint after mutating the grid inside an AutoScroll'd
                // tab. It was originally added on the theory that the duplicate rows being
                // reported were a stale-paint artifact - that theory was WRONG: the
                // duplicates were real rows, caused by finals not persisting item_id, which
                // disabled SetFinalPumpData's own duplicate guard for reloaded rows (fixed
                // 2026-09-03; see SalesProjectContentFinal.item_id). Size Up was never
                // affected - it has always carried item_id.
                uc.Refresh();
            }
        }

        private async Task OnFinalClicked(ItemSetUC uc)
        {
            var pumpItems = await GetPumpItemsAsync();
            var sizeUpIds = uc.GetSizeUpItemIds();
            if (sizeUpIds.Count == 0)
            {
                Helpers.ShowDialogMessage("error", "Add at least one pump to Size Up first.");
                return;
            }

            var filtered = pumpItems.Clone();
            foreach (DataRow row in pumpItems.Rows)
                if (sizeUpIds.Contains(Convert.ToInt32(row["id"])))
                    filtered.ImportRow(row);

            using (var modal = new SizeUpPickerModal(filtered, new List<int>(), "Select Final Pump"))
            {
                if (modal.ShowDialog() != DialogResult.OK) return;

                var picked = modal.GetSelectedItems().FirstOrDefault();
                if (picked == null) return;

                uc.SetFinalPumpData("", "", picked.Model, picked.ItemId.ToString());

                // Same plain repaint as OnSizeUpClicked above. Note SetFinalPumpData's
                // final_item_id guard only actually works now that item_id is persisted -
                // before that fix it was dead for any row loaded from the database, which
                // is what produced the real (not repainted) duplicate finals.
                uc.Refresh();
            }
        }

        private async void btn_save_Click(object sender, EventArgs e)
        {
            if (_fetchedData == null || _quotation == null) return;

            try
            {
                btn_save.Enabled = false;
                lbl_saving_status.Text = "Saving...";

                var pnlQuotation = JObject.FromObject(_quotation).ToObject<Dictionary<string, object>>();

                pnlQuotation["sales_project_multiplier"] = (_fetchedData.sales_project_multiplier ?? new List<SalesProjectMultiplier>())
                    .Where(m => m.based_id == _quotationId)
                    .ToList();

                var allTabsData = new List<Dictionary<string, object>>();

                foreach (TabPage tabPage in tab_container.TabPages)
                {
                    if (!(tabPage.Controls.Count > 0 && tabPage.Controls[0] is ItemSetUC uc)) continue;

                    int itemSetId = Convert.ToInt32(tabPage.Tag);

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
                    var fetchedContent = _fetchedData.sales_project_content?
                        .FirstOrDefault(c => c.based_id == itemSetId);
                    if (fetchedContent != null && contentForSave != null
                        && ToId(contentForSave, "content_id") == 0)
                        contentForSave["content_id"] = fetchedContent.content_id;

                    var fetchedCondition = _fetchedData.sales_project_content_advanced_condition?
                        .FirstOrDefault(c => c.based_id == itemSetId);
                    if (fetchedCondition != null && conditionsForSave != null
                        && ToId(conditionsForSave, "conditions_id") == 0)
                        conditionsForSave["conditions_id"] = fetchedCondition.conditions_id;

                    var tabData = new Dictionary<string, object>
                    {
                        ["sales_project_item_set"] = new Dictionary<string, object>
                        {
                            { "based_id", _quotationId },
                            { "tab_number", tabPage.Text },
                            { "itemset_id", itemSetId },
                            { "is_new_tab", false }
                        },
                        // History is the one thing still echoed back untouched - it is an
                        // audit trail, generated from the diff itself, never typed.
                        ["sales_project_history"] = (_fetchedData.sales_project_history ?? new List<SalesProjectHistory>())
                            .Where(h => h.based_id == itemSetId)
                            .ToList(),
                        ["sales_project_content"] = contentForSave,
                        ["sales_project_content_advanced_condition"] = conditionsForSave,
                        // These two are the real, intended edits - read from the UI.
                        ["sales_project_items"] = uc.GetProjectItems()["sales_project_items"],
                        ["sales_project_wiring"] = uc.GetProjectWiringData()["sales_project_wiring"]
                    };

                    allTabsData.Add(tabData);
                }

                pnlQuotation["sales_project_all_tabs"] = allTabsData;
                pnlQuotation["id"] = _quotationId;

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

                var service = new GeneralService<SalesProjectList>(ApiEndPoints.SALES_PROJECTS);
                var response = await service.Update(changes);

                if (response != null && response.success)
                {
                    lbl_saving_status.Text = "Saved.";
                    Helpers.ShowDialogMessage("success", "Quotation updated successfully.");
                    await LoadQuotationAsync();
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
    }
}
