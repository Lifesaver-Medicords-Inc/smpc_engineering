using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using smpc_engineering_app.Models;
using smpc_engineering_app.Shared;

namespace smpc_engineering_app.Services.Setup
{
    class EngrUsersService : ServiceBase<EngrUsersModel>
    {
        public EngrUsersService() : base(ApiEndPoints.ENGR_USERS) { }

        public async Task<List<string>> GetUsersForComboBox()
        {
            var users = await GetAsList(); // calls the base method
            return users.Select(u => $"Engr. {u.first_name} {u.last_name}").ToList();
        }
    }
}
