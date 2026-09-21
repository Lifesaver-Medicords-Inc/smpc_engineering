using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using smpc_engineering_app.Shared;

namespace smpc_engineering_app.Models
{
    // Hides the sidebar entries this user's position has no access to. Same rules as the
    // inventory app's NavigationAccess, against the same grants: tbl_position_access, seeded
    // per position from the access-level workbook (ERP_API's SeedPositionAccess) and
    // maintained afterwards through Admin's Access Control screen.
    //
    // An entry with no code below stays visible, and so does the whole sidebar for a position
    // carrying no grants at all - a configuration gap should not blank the app.
    internal static class NavigationAccess
    {
        // Sidebar node name (Layout.Designer.cs TreeNode.Name) -> the access codes that open
        // it. BOM and BOQ are catalogued under the inventory app, which is where the screens
        // came from; a code is granted to a position, not to an app, so it reads the same
        // either way.
        private static readonly Dictionary<string, string[]> NodeCodes =
            new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                { "Job Order", new[] { "Job Order.Job Order" } },
                { "Sales Order", new[] { "Sales Order.Sales Order (Engineering)" } },
                { "Item Request", new[] { "Item Request.Item Request", "Item Request.Item Request (v2)" } },
                { "Pick Activity", new[] { "Pick Activity.Pick Activity", "Pick Activity.Pick Activity (v2)" } },
                { "Sales Quotation List", new[] { "Transactions.Sales Quotation List (view)" } },
                { "BOM", new[] { "Engineering.BOM" } },
                { "BOQ", new[] { "Engineering.BOQ" } },
            };

        private static HashSet<string> GrantedCodes()
        {
            var granted = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var access = CacheData.CurrentUser?.position?.access;

            if (access == null)
                return granted;

            foreach (var row in access)
            {
                if (!string.IsNullOrWhiteSpace(row?.code))
                    granted.Add(row.code.Trim());
            }

            return granted;
        }

        public static bool HasAccess(params string[] codes)
        {
            if (codes == null || codes.Length == 0)
                return true;

            var granted = GrantedCodes();

            if (granted.Count == 0)
                return true;

            return codes.Any(code => granted.Contains(code.Trim()));
        }

        public static void Apply(TreeView sidebar)
        {
            if (sidebar == null)
                return;

            var granted = GrantedCodes();
            if (granted.Count == 0)
                return;

            RemoveUngranted(sidebar.Nodes, granted);
        }

        private static void RemoveUngranted(TreeNodeCollection nodes, HashSet<string> granted)
        {
            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                TreeNode node = nodes[i];

                RemoveUngranted(node.Nodes, granted);

                string[] codes;
                if (NodeCodes.TryGetValue(node.Name ?? "", out codes))
                {
                    if (!codes.Any(code => granted.Contains(code.Trim())))
                        nodes.RemoveAt(i);

                    continue;
                }

                // A heading left with nothing under it goes too; a leaf with no code stays.
                if (string.Equals(node.Name, "parent", StringComparison.OrdinalIgnoreCase)
                    && node.Nodes.Count == 0)
                {
                    nodes.RemoveAt(i);
                }
            }
        }
    }
}
