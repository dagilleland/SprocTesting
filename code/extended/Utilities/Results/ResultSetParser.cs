using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace DatabaseUnitTesting.Utilities
{
    internal static class ResultSetParser
    {
        private const int NAME = 0;
        private const int WIDTH = 2;
        private const int PRECISION = 3;
        private const int SCALE = 4;
        private const int TYPE = 24;

        internal static Database Parse(SqlCommand command)
        {
            using(SqlDataReader sqlReader = command.ExecuteReader())
            {
                Database results = new Database();

                if(!sqlReader.HasRows)
                    return results;

                do
                {
                    Table table = new Table("Result Set");
                    List<string> fieldNames = SetFields(table, sqlReader.GetSchemaTable());
                    while(sqlReader.Read())
                    {
                        Row row = new Row("datarow");
                        for(int i = 0; i < sqlReader.FieldCount; i++)
                        {
                            object value = sqlReader.GetValue(i);
                            if(!(value is DBNull))
                                row.AddColumn(new Column(fieldNames[i], sqlReader.GetValue(i)));
                        }
                        table.AddRow(row);
                    }
                    results.AddTable(table);
                } while(sqlReader.NextResult());
                return results;
            }
        }

        internal static List<string> SetFields(Table table, DataTable schema)
        {
            List<string> fieldNames = new List<string>();
            foreach(DataRow dataRow in schema.Rows)
            {
                string name = dataRow[NAME].ToString();
                string type = dataRow[TYPE].ToString().ToLower();
                string precision = dataRow[PRECISION].ToString();
                string scale = dataRow[SCALE].ToString();
                string width = dataRow[WIDTH].ToString();

                StringBuilder typeString = new StringBuilder(type);

                if(type.Equals("decimal") || type.Equals("numeric"))
                {
                    typeString.Append("(");
                    typeString.Append(precision);
                    typeString.Append(",");
                    typeString.Append(scale);
                    typeString.Append(")");
                } else if(type.Contains("char") || type.Contains("binary"))
                {
                    if(int.Parse(width) > 8000)
                        width = "max";

                    typeString.Append("(");
                    typeString.Append(width);
                    typeString.Append(")");
                }
                
                fieldNames.Add(name);
                table.AddField(name, typeString.ToString());
            }
            return fieldNames;
        }
    }
}