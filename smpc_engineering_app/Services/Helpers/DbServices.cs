using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smpc_engineering_app.Services.Helpers
{
    delegate void ErrorLogsHandler(string error);
    delegate void AtSavingHandler(string action, SqlConnection connection, SqlTransaction transaction, string tableName, string key, string value);

    public class DbServices
    {
        //private event ErrorLogsHandler ErrorOccurred; 

        //public DbServices()
        //{
        //    this.ErrorOccurred += LogsServices.ErrorLogs;
        //}

        readonly string connectionString = "Server=192.168.1.15;Database=erp_smpc;User Id=sa;Password=DESKTOP-QQPLJKC";

        protected string SaveChanges(string tableName, Dictionary<string, dynamic> data, string cols_key = "id")
        {
             
            string key = "";
            string value = "";
            string updateValues = "";
            int index = 0;

            using (SqlConnection connection = new SqlConnection(connectionString)) // Replace with your connection string
            {
                connection.Open();

                // Begin a transaction
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string obj_id = data[cols_key].ToString();


                        if (obj_id.Equals("''")) //INSERT DATA
                        {
                            data.Remove(cols_key);
                            foreach (var item in data)
                            {
                                key += index == 0 ? item.Key : ',' + item.Key;
                                value += index == 0 ? item.Value : ',' + item.Value;
                                index += 1;
                            }

                            // INSERT ON MAIN TABLE
                            string query = $"INSERT INTO {tableName} ({key}) VALUES ({value}); SELECT SCOPE_IDENTITY();"; // Adapt for your table and columns
                            object insertedId;
                            using (SqlCommand command = new SqlCommand(query, connection, transaction))
                            {
                                insertedId = command.ExecuteScalar();
                            }

                            key += ',' + cols_key;
                            value += ',' + insertedId.ToString();

                            SaveToAuditTrail("INSERT", connection, transaction, tableName, key, value); 

                            // Commit the transaction
                            transaction.Commit();

                            Helpers.ShowDialogMessage("success", "Saved changes!");

                            return insertedId.ToString();

                        }
                        else //UPDATE DATA
                        {
                            data.Remove(cols_key);
                            foreach (var item in data)
                            {
                                key += index == 0 ? item.Key : ',' + item.Key;
                                value += index == 0 ? item.Value : ',' + item.Value;
                                updateValues += index == 0 ? item.Key + '=' + item.Value : ',' + item.Key + '=' + item.Value;
                                index += 1;
                            }

                            // UPDATE ON MAIN TABLE
                            string query = $"UPDATE {tableName} SET {updateValues} WHERE {cols_key}={obj_id};"; // Adapt for your table and columns 
                            using (SqlCommand command = new SqlCommand(query, connection, transaction))
                            {
                                command.ExecuteNonQuery();
                            }

                            key += ',' + cols_key;
                            value += ',' + obj_id;

                            SaveToAuditTrail("UPDATE", connection, transaction, tableName, key, value);
                              
                            // Commit the transaction 
                            transaction.Commit();

                            Helpers.ShowDialogMessage("success", "Update changes!");
                            return obj_id.ToString();
                        }



                    }
                    catch (Exception)
                    {
                        // Rollback the transaction if an error occurs
                        transaction.Rollback();
                        throw; 
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }

        protected DataTable GetAll(string queryStr)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString)) // Replace with your connection string
                {
                    SqlCommand command = new SqlCommand(queryStr, connection);
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    connection.Close();
                    return dt;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        protected void GetAll(string queryStr, DataSet ds, string tableName)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString)) // Replace with your connection string
                {
                    SqlCommand command = new SqlCommand(queryStr, connection);
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable dt = new DataTable();
                    adapter.Fill(ds, tableName);
                    connection.Close();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        protected void Delete(string tableName, Dictionary<string, dynamic> data, string uniqueKey = "id")
        {

            try
            {
                string key = "";
                string value = "";
                int index = 0;

                dynamic idValue;
                Boolean isBool = data.TryGetValue(uniqueKey, out idValue);
                if (!isBool)
                {
                    return;
                }

                using (SqlConnection connection = new SqlConnection(connectionString)) // Replace with your connection string
                {

                    connection.Open();
                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            string query = $"DELETE FROM {tableName} WHERE {uniqueKey} = {idValue}";

                            using (SqlCommand command = new SqlCommand(query, connection, transaction))
                            {
                                command.ExecuteNonQuery();
                            }


                            foreach (var item in data)
                            {
                                key += index == 0 ? item.Key : ',' + item.Key;
                                value += index == 0 ? item.Value : ',' + item.Value;
                                index += 1;
                            }

                            SaveToAuditTrail("DELETE", connection, transaction, tableName, key, value);

                            // Commit the transaction
                            transaction.Commit();

                            Helpers.ShowDialogMessage("success", "Successfully deleted!");

                        }
                        catch (Exception ex)
                        {
                            // Rollback the transaction if an error occurs
                            transaction.Rollback();
                            // Optionally log the exception or handle it
                            Console.WriteLine("An error occurred: " + ex.Message);
                        }
                        finally
                        {
                            connection.Close();
                        }
                    }

                    connection.Close();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        protected void SaveToAuditTrail(string action, SqlConnection connection, SqlTransaction transaction, string tableName, string key, string value)
        {

            // INSERT ON AUDIT TABLE
            string query_at = $"INSERT INTO {"z_" + tableName + "_at"} ({key},AT_ACTION,IP_ADDRESS,AT_DATE,AT_USER_ID,AT_USER,MOTHERBOARD_SERIAL_NO,MACHINE_NAME) VALUES ({value},'{action}','{Helpers.GetLocalIPAddress()}','{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}','1','JEROME POGI','{Helpers.GetSerialNumber()}','{Environment.MachineName}')"; // Adapt for your table and columns

            using (SqlCommand command = new SqlCommand(query_at, connection, transaction))
            {
                command.ExecuteNonQuery();
            }

        }
    }
}
