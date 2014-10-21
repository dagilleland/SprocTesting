using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using DatabaseUnitTesting.Utilities;

namespace DatabaseUnitTesting
{
    public class ResultSetTester
    {
        private readonly SqlCommand _command;

        public ResultSetTester(SqlConnection connection, string procedureName)
        {
            _command = new SqlCommand(procedureName, connection);
            _command.CommandType = CommandType.StoredProcedure;
        }

        public ResultSetTester(SqlCommand command)
        {
            _command = command;
        }

        public IEnumerable<KeyValuePair<string, object>> PrintOuputParameterValues()
        {
            Dictionary<string, object> outputParams = new Dictionary<string, object>();
            foreach(SqlParameter p in _command.Parameters)
                if(p.Direction == ParameterDirection.Output)
                    outputParams.Add(p.ParameterName,p.Value);

            return outputParams;
        }

        public bool CompareToFile(string filename)
        {
            Database procedureResults = ResultSetParser.Parse(_command);
            Database fileResults = XmlFileAdapter.Read(filename);

            if(!procedureResults.Equals(fileResults))
                return false;

            return true;
        }
        public void OutputToFile(string filename){
            
            Database results = ResultSetParser.Parse(_command);
            XmlFileAdapter.Write(filename, results);
        }

        public void SetInputParameter(string parameterName, object parameterValue)
        {
            if(_command.Parameters.Contains(parameterName))
                _command.Parameters[parameterName] =
                    new SqlParameter(parameterName, parameterValue);
            else
                _command.Parameters.AddWithValue(parameterName, parameterValue);
        }

        public void SetOutputParameter(string parameterName, SqlDbType type, int size)
        {
            if(_command.Parameters.Contains(parameterName))
                _command.Parameters.RemoveAt(parameterName);

            _command.Parameters.Add(parameterName, type);
            _command.Parameters[parameterName].Direction = ParameterDirection.Output;
            if(size != 0)
                _command.Parameters[parameterName].Size = size;
        }
    }
}