using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace DatabaseUnitTesting.Utilities
{
    internal class DatabaseComparer
    {
        private readonly SqlConnection _connection;
        private readonly DatabaseAdapter _databaseAdapter;
        private readonly string _databaseOne;
        private readonly string _databaseTwo;
        private readonly List<ObjectsToCompare> _objectsToCompare = new List<ObjectsToCompare>();

        internal DatabaseComparer(SqlConnection connection, string databaseOneName, string databaseTwoName)
        {
            _connection = connection;
            _databaseAdapter = new DatabaseAdapter(connection);
            _databaseAdapter.UseDatabase(databaseOneName);
            _databaseOne = databaseOneName;
            _databaseTwo = databaseTwoName;
        }

        internal void CleanUp()
        {
            _objectsToCompare.Clear();
        }

        internal Database GenerateDifferences(SqlTransaction transaction)
        {
            Database tables = new Database();

            foreach (ObjectsToCompare objects in _objectsToCompare)
            {
                Table table = RunCompare(transaction, objects);
                if(table.RowCount > 0)
                    tables.AddTable(table);
            }

            return tables;
        }

        internal string GetAllColumns(SqlTransaction transaction, ObjectsToCompare objects)
        {
            SqlCommand command = _connection.CreateCommand();
            command.CommandText =
                "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE table_name = @table AND" +
                " table_schema = @schema AND table_catalog = @database";
            command.Transaction = transaction;
            command.Parameters.AddWithValue("@table", objects.Object1Name);
            command.Parameters.AddWithValue("@schema", objects.Schema1Name);
            command.Parameters.AddWithValue("@database", _databaseOne);

            List<string> allColumns = new List<string>();

            using (SqlDataReader result = command.ExecuteReader())
            {
                while (result.Read())
                    allColumns.Add(result.GetString(0));
            }

            if (allColumns.Count == 0)
                throw new ArgumentException("Object "+objects.Qualified1+" does not exist");

            foreach (string victim in objects.ColumnsToIgnore)
            {
                if (!allColumns.Remove(victim))
                    throw new ArgumentException("Specified column " + victim + " was not in table " +
                                                objects.Qualified1);
                if (allColumns.Count == 0)
                    throw new ArgumentException("User cannot ignore all columns in a table.");
            }

            return String.Join(",", allColumns.ToArray());
        }

        internal Table RunCompare(SqlTransaction transaction, ObjectsToCompare objectComparison)
        {
            Table table =
                new Table(objectComparison.Qualified1, objectComparison.Qualified2);

            string columns = GetAllColumns(transaction, objectComparison);
            
            StringBuilder select = new StringBuilder("SELECT ", 1000);
            select.Append(columns);
            select.Append(", ROW_NUMBER() OVER(PARTITION BY ");
            select.Append(columns);
            select.Append(" ORDER BY @@SPID) AS 'TempRowNumber' FROM ");
            string select1 = select + _databaseOne + "." + objectComparison.Qualified1;
            string select2 = select + _databaseTwo + "." + objectComparison.Qualified2;
            string commandText = "--Data Comparison\n" + select1 + "\nEXCEPT\n" + select2 + "\n" +
                                 select2 + "\nEXCEPT\n" + select1;
            SqlCommand command = _connection.CreateCommand();
            command.CommandText = commandText;
            command.Transaction = transaction;

            using(SqlDataReader reader = command.ExecuteReader())
            {
                string type = "In First";
                string[] columnNames = columns.Split(',');
                do
                {
                    while(reader.Read())
                    {
                        Row row = new Row(type);
                        for(int i = 0; i < (reader.FieldCount - 1); i++)
                        {
                            object value = reader.GetValue(i);
                            if(!(value is DBNull))
                                row.AddColumn(new Column(columnNames[i].ToLower(), value));
                        }
                        table.AddRow(row);
                    }
                    type = "In Second";
                } while(reader.NextResult());
            }

            return table;
        }

        internal void AddObjectComparison(string schema1, string object1, string schema2, string object2)
        {
            _objectsToCompare.Add(new ObjectsToCompare(schema1, object1, schema2, object2));
        }

        internal void AddColumnToIgnore(string schemaName, string objectName, string columnName)
        {
            _objectsToCompare.Find(
                delegate(ObjectsToCompare item) { return item.Schema1Name == schemaName && item.Object1Name == objectName; }
                ).AddColumnToIgnore(columnName);
        }

        internal void AddColumnsToIgnore(string schemaName, string tableName, List<string> columnNames)
        {
            foreach(string columnName in columnNames)
                AddColumnToIgnore(schemaName, tableName, columnName);
        }
    }
}