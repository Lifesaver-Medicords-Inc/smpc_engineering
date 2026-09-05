using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using smpc_engineering_app.Models;

namespace smpc_engineering_app.Shared
{
    static class CacheData
    {
        //Setup
        public static DataTable ChartOfAccountClass { get; set; } = new DataTable();
        public static DataTable ChartOfAccountGroup { get; set; } = new DataTable();
        public static DataTable ChartOfAccountClassification { get; set; } = new DataTable();
        public static DataTable TaxClassification { get; set; } = new DataTable();
        public static DataTable PaymentTerms { get; set; } = new DataTable();


        public static String SessionToken { get; set; } = "";
        public static CurrentUserModel CurrentUser { get; set; } = null;

        // Copies CurrentUser into the SALES assembly's own CacheData (added 2026-09-05:
        // "the user the edit is not seeing on the record").
        //
        // Saving a quotation from the Sales Quotation page runs Quotation.GetFullDiff,
        // which calls BuildAutoHistoryEntries to write the CHANGE HISTORY rows - and that
        // stamps them from smpc_sales_app.Data.CacheData.CurrentUser, a different static
        // from this one. This app only ever populates its own, so the Sales-side one
        // stayed null and every history row written from here saved with a blank USER
        // column. The edit itself saved correctly; it just wasn't attributed to anyone.
        //
        // Same problem, and same remedy, as the SessionToken mirror in ApiService - see
        // MirrorTokenToSalesAssembly there for the fuller explanation of why the two
        // assemblies each keep their own copy of this state.
        //
        // The two CurrentUserModel classes are field-for-field identical but declared in
        // different assemblies, so this copies field by field rather than assigning.
        // Engineering already references smpc_inventory_app (see the .csproj), which is
        // where the Sales-side type is declared.
        public static void MirrorCurrentUserToSalesAssembly()
        {
            try
            {
                if (CurrentUser == null)
                    return;

                smpc_sales_app.Data.CacheData.CurrentUser = new smpc_inventory_app.Model.CurrentUserModel
                {
                    id = CurrentUser.id,
                    employee_id = CurrentUser.employee_id,
                    first_name = CurrentUser.first_name,
                    last_name = CurrentUser.last_name,
                    department = CurrentUser.department,
                    position_id = CurrentUser.position_id,
                };
            }
            catch
            {
                // Non-fatal: worst case the history row is attributed to nobody, exactly
                // as it was before this existed.
            }
        }
    }
}
