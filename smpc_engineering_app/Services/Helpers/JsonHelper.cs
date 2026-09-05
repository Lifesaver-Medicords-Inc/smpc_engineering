using System;
using System.Collections.Generic;
using System.Data; 
using Newtonsoft.Json.Linq;

namespace smpc_engineering_app.Services.Helpers
{
    class JsonHelper
    {
        public static DataTable ToDataTable(JArray jArray)
        {
            // Create a new DataTable
            DataTable dataTable = new DataTable();

            // If the JArray is not empty, get the first object to create columns
            if (jArray.Count > 0)
            {
                // Create columns based on the properties of the first object in the JArray
                foreach (JProperty property in jArray[0].ToObject<JObject>().Properties())
                {
                    dataTable.Columns.Add(property.Name);
                }

                // Add rows to the DataTable
                foreach (var item in jArray)
                {
                    var row = dataTable.NewRow();
                    var jsonObject = item.ToObject<JObject>();

                    // Add each value from the JObject to the corresponding column
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        // If the value is null, assign DBNull.Value; otherwise, convert to string
                        if (jsonObject[column.ColumnName] == null)
                        {
                            row[column] = DBNull.Value;
                        }
                        else
                        {
                            row[column] = jsonObject[column.ColumnName].ToString();
                        }
                    }

                    // Add the row to the DataTable
                    dataTable.Rows.Add(row);
                }
            } 
            return dataTable;
        } 

        public static DataTable ToDataTable<T>(List<T> items)
        {
            var dataTable = new DataTable();

            // Get all the properties of the model
            var properties = typeof(T).GetProperties();

            // Add columns to DataTable for each property
            foreach (var prop in properties)
            {
                dataTable.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }

            // Null-guard, 2026-09-05: this threw NullReferenceException whenever the API
            // call behind it came back with no payload - a stopped or restarting API, a
            // dropped connection, or any non-success response. ServiceBase.GetAsDatatable
            // hands the response body straight in, so "service unreachable" surfaced as a
            // hard crash in a formatting helper rather than as a handled error. An empty
            // table keeps the COLUMNS (already built above from T), so callers that check
            // Columns.Contains(...) still behave correctly - they just see zero rows.
            if (items == null) return dataTable;

            // Add rows to the DataTable
            foreach (var item in items)
            {
                var row = dataTable.NewRow();
                foreach (var prop in properties)
                {
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                }
                dataTable.Rows.Add(row);
            }

            return dataTable;
        }
    }
}
