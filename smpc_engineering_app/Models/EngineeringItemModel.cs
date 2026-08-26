using System.Collections.Generic;

namespace smpc_engineering_app.Models
{
    // Minimal stand-in for smpc_sales_system's own ItemModel/Items (both declared with no
    // access modifier there - implicitly internal, so not usable from this assembly even
    // via the fixed cross-project reference, Phase 4 item 4.1's detail editor). Only the
    // fields SizeUpPickerModal's own constructor actually reads
    // ("id"/"item_brand"/"item_model", pre-filtered by "item_name" == "PUMP" the same way
    // Quotation.cs's own SizeUpClicked handler does) are included - this is not meant to be
    // a general item model.
    public class EngineeringItemModel
    {
        public int id { get; set; }
        public string item_name { get; set; }
        public string item_brand { get; set; }
        public string item_model { get; set; }
    }

    public class EngineeringItems
    {
        public List<EngineeringItemModel> items { get; set; }
    }
}
