using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace DatabaseUnitTesting.Utilities
{
    internal class DatabaseAdapter
    {
        private readonly SqlConnection _connection;

        internal DatabaseAdapter(SqlConnection connection)
        {
            _connection = connection;

            if (_connection.State == ConnectionState.Closed)
                _connection.Open();
        }

        internal bool IsSnapshot(string name)
        {
            SqlCommand command = _connection.CreateCommand();
            command.CommandText = 
                "--Testing Existence Type\nSELECT source_database_id FROM sys.databases WHERE name = @name";

            command.Parameters.AddWithValue("@name", name);

            object objResult = command.ExecuteScalar();

            if(objResult != null && !(objResult is DBNull) )
                return true;

            return false;
        }

        internal void UseDatabase(string databaseName)
        {
            SqlCommand command = _connection.CreateCommand();
            command.CommandText = "USE " + databaseName;
            command.ExecuteNonQuery();
        }

        internal void CreateSnapshot(string databaseName, string snapshotName)
        {
            SqlCommand command = _connection.CreateCommand();
            
            StringBuilder stringBuilder = new StringBuilder("-- Creating Snapshot \nCREATE DATABASE ");
            stringBuilder.Append(snapshotName);
            stringBuilder.Append(" ON ( NAME = ");
            stringBuilder.Append(databaseName);
            stringBuilder.Append(", FILENAME = 'C:\\Temp\\");
            stringBuilder.Append(snapshotName);
            stringBuilder.Append("') AS SNAPSHOT OF ");
            stringBuilder.Append(databaseName);
            command.CommandText = stringBuilder.ToString();
            command.ExecuteNonQuery();
        }
     
        internal void DropSnapshot(string snapshotName)
        {
            if(! IsSnapshot(snapshotName) )
                throw new ArgumentException("A snapshot of name " + snapshotName + " does not exist.");

            SqlCommand command = _connection.CreateCommand();
            command.CommandText = "DROP DATABASE " + snapshotName;
            command.ExecuteNonQuery();
        }
    }
}