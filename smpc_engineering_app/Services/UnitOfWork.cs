using smpc_accounting_app.Models;
using smpc_accounting_app.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smpc_accounting_app.Services
{
    class UnitOfWork
    {

        public ServicesDbBase<Bank> BankRepository = new ServicesDbBase<Bank>(ApiEndPoints.CHART_CLASS_SETUP); 
    }
}
