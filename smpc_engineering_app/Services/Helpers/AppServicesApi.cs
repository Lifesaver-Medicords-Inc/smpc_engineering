using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smpc_accounting_app.Services.Helpers
{
    class AppServices : DbServices
    {
        private string TableName { get; set; }

        public AppServices(string tableName)
        {
            this.TableName = tableName;
        }

        public  virtual DataTable Get()
        {
            return GetAll("select * from " + this.TableName);
        }

        public virtual AppServices Save(Dictionary<string, dynamic> data)
        {
            SaveChanges(this.TableName, data);
            return this;
        }

        public virtual void Delete(Dictionary<string, dynamic> data)
        {
            Delete(this.TableName, data);
        }

    }
}
