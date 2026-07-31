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

            // Style the document number like a clickable link so it reads as
            // "click here to open the sales order" instead of a plain label.
            lbl_doc_no.ForeColor = Color.FromArgb(30, 144, 255); // link blue
            lbl_doc_no.Font = new Font(lbl_doc_no.Font, lbl_doc_no.Font.Style | FontStyle.Underline);
            lbl_doc_no.Cursor = Cursors.Hand;

            lbl_doc_no.Click += RedBoxJobOrderItem_Click;
        }

        private void RedBoxJobOrderItem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Id))
            {
                OnSalesOrderClicked?.Invoke(this, Id);
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

        public string Id
        {
            get => lbl_id.Text;
            set => lbl_id.Text = value;
        }
    }
}
