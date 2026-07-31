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
    public partial class RedBoxQuotationItem : UserControl
    {
        public RedBoxQuotationItem()
        {
            InitializeComponent();

            this.AutoSize = false;
        }

        public string ClientName
        {
            get => lbl_client_name.Text;
            set => lbl_client_name.Text = value;
        }

        public string SalesQuotation
        {
            get => lbl_doc_no.Text;
            set => lbl_doc_no.Text = value;
        }

        public string Status
        {
            get => lbl_status.Text;
            set => lbl_status.Text = value;
        }

        public string ProjectName
        {
            get => lbl_project_name.Text;
            set => lbl_project_name.Text = value;
        }

        public string SalesExecutive
        {
            get => lbl_sales_exec.Text;
            set => lbl_sales_exec.Text = value;
        }

        public string Remark
        {
            get => lbl_remark.Text;
            set => lbl_remark.Text = value;
        }
    }
}
