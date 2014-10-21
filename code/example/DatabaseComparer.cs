using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace UnitTestingTools
{
    internal class DatabaseComparer : IDisposable
    {
        private readonly string databaseName;
        private readonly string snapshotName;
        private readonly SqlConnection connection;
        
        internal DatabaseComparer(SqlConnection connection, string databaseName, string snapshotName)
        {
            this.databaseName = databaseName;
            this.snapshotName = snapshotName;
            this.connection = connection;
            CreateSnapshot();
        }

        private void CreateSnapshot()
        {
            SqlCommand command = connection.CreateCommand();
            command.CommandText = "CREATE DATABASE " + snapshotName + " ON ( NAME = " + databaseName +
                                  ", FILENAME = 'C:\\Temp\\" + snapshotName + ".snap') AS SNAPSHOT OF " +
                                  databaseName;
            command.ExecuteNonQuery();
        }

        internal void Compare(ObjectToCompare objectToCompare, SqlTransaction transaction, DataSet results)
        {
            DataSet tempResult = new DataSet();

            string columns = GetAllColumns(objectToCompare, transaction);

            string select = "SELECT " + columns + ", ROW_NUMBER() OVER(PARTITION BY " + columns +
                            " ORDER BY @@SPID) AS 'TempRowNumber' FROM ";

            string select1 = select + databaseName + "." + objectToCompare.Qualified;
            string select2 = select + snapshotName + "." + objectToCompare.Qualified;

            SqlCommand command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = select1 + " EXCEPT " + select2 + "\n" + select2 + " EXCEPT " + select1;

            using(SqlDataAdapter da = new SqlDataAdapter(command))
            {
                da.Fill(tempResult);
            }

            tempResult.Tables[0].TableName = objectToCompare.Qualified + " in 1";
            tempResult.Tables[1].TableName = objectToCompare.Qualified + " in 2";

            results.Tables.Add(tempResult.Tables[0].Copy());
            results.Tables.Add(tempResult.Tables[1].Copy());
        }

        private string GetAllColumns(ObjectToCompare objects, SqlTransaction transaction)
        {
            SqlCommand command = connection.CreateCommand();
            command.CommandText =
                "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE table_name = @table AND" +
                " table_schema = @schema AND table_catalog = @database";
            command.Transaction = transaction;
            command.Parameters.AddWithValue("@table", objects.ObjectName);
            command.Parameters.AddWithValue("@schema", objects.SchemaName);
            command.Parameters.AddWithValue("@database", databaseName);

            List<string> allColumns = new List<string>();

            using(SqlDataReader result = command.ExecuteReader())
            {
                while(result.Read())
                    allColumns.Add(result.GetString(0));
            }

            foreach(string victim in objects.ColumnsToIgnore)
            {
                if(!allColumns.Remove(victim))
                    throw new ArgumentException("Specified column " + victim + " was not in table " +
                                                objects.Qualified);
                if(allColumns.Count == 0)
                    throw new ArgumentException("User cannot ignore all columns table " + objects.Qualified);
            }

            return String.Join(",", allColumns.ToArray());
        }

        public void Dispose()
        {
            SqlCommand command = connection.CreateCommand();
            command.CommandText = "DROP DATABASE " + snapshotName;
            command.ExecuteNonQuery();
        }
    }
}