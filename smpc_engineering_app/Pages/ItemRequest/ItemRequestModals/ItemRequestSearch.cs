using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using smpc_engineering_app.Services.Helpers;

namespace smpc_engineering_app.Pages.ItemRequest.ItemRequestModals
{
    public partial class ItemRequestSearch : Form
    {
        public string SelectedIRId { get; private set; } = null;
        private string placeHolderText = "Item Request Search...";
        private DataTable irTable;

        public ItemRequestSearch()
        {
            InitializeComponent();

            // Center the modal relative to its parent form
            this.StartPosition = FormStartPosition.CenterParent;

            dgv_ir_search.AutoGenerateColumns = false;
            InitializeSearchBox();
        }

        private void InitializeSearchBox()
        {
            txt_search = Helpers.CreateSearchBox(placeHolderText, txt_search_TextChanged);
            this.Controls.Add(txt_search);
        }

        private void txt_search_TextChanged(object sender, EventArgs e)
        {

        }

        private void ItemRequestSearch_Load(object sender, EventArgs e)
        {

        }
    }
}
