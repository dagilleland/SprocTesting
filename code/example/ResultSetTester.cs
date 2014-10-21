using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace UnitTestingTools
{
    public class ResultSetTester
    {
        private readonly DataSet results = new DataSet();
        private readonly SqlCommand storedProcedure;

        public ResultSetTester(SqlConnection connection, string procedureName)
        {
            storedProcedure = new SqlCommand(procedureName, connection);
            storedProcedure.CommandType = CommandType.StoredProcedure;
        }

        public bool CompareToFile(string filename)
        {
            RunProcedure();

            DataSet other = new DataSet();
            other.ReadXml(filename);
            other.AcceptChanges();

            return DataSetComparer.Compare(results, other);
        }

        public void OutputToFile(string filename)
        {
            RunProcedure();
            results.WriteXml(filename, XmlWriteMode.WriteSchema);
        }

        public IEnumerable<KeyValuePair<string, object>> OuputParameters
        {
            get
            {
                Dictionary<string, object> outputParams = new Dictionary<string, object>();
                foreach(SqlParameter p in storedProcedure.Parameters)
                    if(p.Direction == ParameterDirection.Output)
                        outputParams.Add(p.ParameterName, p.Value);

                return outputParams;
            }
        }

        public void SetInputParameter(string parameterName, object parameterValue)
        {
            SqlParameterCollection parameters = storedProcedure.Parameters;
            if(parameters.Contains(parameterName))
                parameters[parameterName] = new SqlParameter(parameterName, parameterValue);
            else
                parameters.AddWithValue(parameterName, parameterValue);
        }

        public void SetOutputParameter(string parameterName, SqlDbType type, int size)
        {
            SqlParameterCollection parameters = storedProcedure.Parameters;
            if(parameters.Contains(parameterName))
                parameters.RemoveAt(parameterName);
            
            parameters.Add(parameterName, type);
            parameters[parameterName].Direction = ParameterDirection.Output;
            if(size > 0)
                parameters[parameterName].Size = size;
        }

        private void RunProcedure()
        {
            results.Reset();

            using(SqlDataAdapter da = new SqlDataAdapter(storedProcedure))
            {
                da.Fill(results);
            }

            for(int i = 0; i < results.Tables.Count; i++)
                results.Tables[i].TableName = "Result Set " + i;

            results.AcceptChanges();
        }
    }
}