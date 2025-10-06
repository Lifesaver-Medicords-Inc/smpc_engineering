using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace smpc_engineering_app.Pages.Components
{
    public partial class RedBoxJobOrderItem : UserControl
    {
        public event EventHandler<string> OnSalesOrderClicked;

        public RedBoxJobOrderItem()
        {
            InitializeComponent();

            this.AutoSize = false;

            lbl_doc_no.DoubleClick += RedBoxJobOrderItem_Click;
        }

        private void RedBoxJobOrderItem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(DocumentNo))
            {
                string cleanedValue = DocumentNo.StartsWith("SO#")
                    ? DocumentNo.Substring(3)
                    : DocumentNo;

                OnSalesOrderClicked?.Invoke(this, cleanedValue);
            }
        }

        public string ClientName
        {
            get => lbl_client_name.Text;
            set => lbl_client_name.Text = value;
        }

        public string DocumentNo
        {
            get => lbl_doc_no.Text;
            set => lbl_doc_no.Text = value;
        }

        public string Items
        {
            get => lbl_items.Text;
            set => lbl_items.Text = value;
        }

        public string ProjectName
        {
            get => lbl_proj_name.Text;
            set => lbl_proj_name.Text = value;
        }

        public string DueDate
        {
            get => lbl_due_date.Text;
            set => lbl_due_date.Text = value;
        }

        public string Type
        {
            get => lbl_type.Text;
            set => lbl_type.Text = value;
        }
    }
}
