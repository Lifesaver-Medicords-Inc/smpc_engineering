using smpc_accounting_app.Models;
using smpc_accounting_app.Services.Helpers;
using smpc_accounting_app.Shared;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smpc_accounting_app.Services.Setup
{
    class BankService : ServiceBase<Bank>
    {
        public BankService() : base(ApiEndPoints.BANK_SETUP) {}
    } 
} 