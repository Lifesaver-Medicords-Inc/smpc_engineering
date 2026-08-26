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
    // Per user direction: editing is enforced at SAVE time, not on screen. Every
    // ItemSetUC tab is fully interactive (SetEditable(true), same as Sales's own Edit
    // mode) rather than hand-gating a dozen+ individual controls that all key off one
    // shared _isEditable flag with no existing seam for "these three areas only" - see
    // the phase4 memory file for the full reasoning. What actually enforces §3.2's "only
    // Size Up/item table/wiring take effect" is this page's own save-payload
    // construction: header fields, multipliers, tab content, and advanced conditions are
    // always sent back EXACTLY as fetched (never read from the UI), so nothing typed
    // into a locked-in-spirit field is ever included in the diff this sends - only real
    // item table and wiring edits (and Size Up, in the sense Sales itself already
    // supports it - see below) can actually change anything.
    //
    // Deliberately NOT supported here (Phase 4.1 scope decision, confirmed with user):
    // swapping an item's MODEL. That routes through Quotation.cs's own
    // HandleModelSelectionClick, which depends on Sales's internal ItemList/BomHead/
    // BomDetails caches and several hundred more lines of BOM parent/child
    // re-expansion - a separate, much larger piece of work. Clicking a MODEL cell here
    // shows a message instead of silently doing nothing.
    //
    // Known, pre-existing limitation inherited from Sales, not introduced here: Size Up
    // (ItemSetUC.AddSizeUpRow/GetSizeUpData) has no save path anywhere in
    // smpc_sales_system either - GetSizeUpData() is dead code, never called from
    // Quotation.cs's own save path. Size Up rows work as an in-session picker that
    // filters Final Selection (exactly as §5.1.4 describes) but are not currently
    // persisted by Sales itself, so they won't persist here either - this page matches
    // Sales's actual behavior rather than inventing a save path Sales itself lacks.
    public partial class SalesQuotationEngPage : UserControl
    {
        private int _quotationId;
        private SalesProjectList _fetchedData;
        private SalesQuotationModel _quotation;
        private readonly List<ItemSetUC> _tabControls = new List<ItemSetUC>();
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
                TabPage newTab = new TabPage(tab.tab_number) { Tag = tab.itemset_id };

                ItemSetUC uc = new ItemSetUC { Dock = DockStyle.Fill };

                uc.SizeUpClicked += async (s, e) => await OnSizeUpClicked(uc);
                uc.FinalTxtBoxClicked += async (s, e) => await OnFinalClicked(uc);
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
                uc.SetTemplateName(content != null ? content.template_project_id.ToString() : "0");
                uc.SetWiring(content != null ? content.is_wiring.ToString() : "false");

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

                // Per the type-level comment above: fully interactive, not gated to just
                // the 3 areas - the save path is what actually restricts what takes effect.
                uc.SetEditable(true);

                _tabControls.Add(uc);
            }
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

                    var tabData = new Dictionary<string, object>
                    {
                        ["sales_project_item_set"] = new Dictionary<string, object>
                        {
                            { "based_id", _quotationId },
                            { "tab_number", tabPage.Text },
                            { "itemset_id", itemSetId },
                            { "is_new_tab", false }
                        },
                        // History/content/advanced-conditions sent back EXACTLY as fetched
                        // (never read from the UI) - see the type-level comment. This is
                        // what guarantees those areas can never actually change, regardless
                        // of what's technically typeable on screen.
                        ["sales_project_history"] = (_fetchedData.sales_project_history ?? new List<SalesProjectHistory>())
                            .Where(h => h.based_id == itemSetId)
                            .ToList(),
                        ["sales_project_content"] = _fetchedData.sales_project_content?
                            .FirstOrDefault(c => c.based_id == itemSetId),
                        ["sales_project_content_advanced_condition"] = _fetchedData.sales_project_content_advanced_condition?
                            .FirstOrDefault(c => c.based_id == itemSetId),
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
