using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace UnitTestingTools
{
    public class DatabaseModificationTester : IDisposable
    {
        private readonly DatabaseComparer _dataComparer;
        private readonly List<ObjectToCompare> _objectsToCompare = new List<ObjectToCompare>();
        private readonly SqlConnection _connection;
        private readonly DataSet _results = new DataSet();
        private SqlTransaction _transaction;

        public DatabaseModificationTester(SqlConnection connection, string databaseName, string snapshotName)
        {
            _connection = connection;
            _dataComparer = new DatabaseComparer(connection, databaseName, snapshotName);
        }

        public SqlTransaction BeginTestTransaction()
        {
            if(_transaction != null)
                EndTestTransaction();

            return (_transaction = _connection.BeginTransaction());
        }

        public void EndTestTransaction()
        {
            if(_transaction == null)
                throw new InvalidOperationException("Transaction does not exist");

            _objectsToCompare.Clear();
            _transaction.Rollback();
            _transaction.Dispose();
            _transaction = null;
        }

        public bool AreEqual()
        {
            RunComparison();

            foreach(DataTable table in _results.Tables)
                if(table.Rows.Count > 0)
                    return false;

            return true;
        }

        public bool CompareToFile(string filename)
        {
            RunComparison();

            DataSet other = new DataSet();
            other.ReadXml(filename);
            other.AcceptChanges();

            return DataSetComparer.Compare(_results, other);
        }

        public void SaveToFile(string filename)
        {
            RunComparison();
            _results.WriteXml(filename, XmlWriteMode.WriteSchema);
        }

        public void AddObjectComparison(string schemaName, string objectName)
        {
            _objectsToCompare.Add(new ObjectToCompare(schemaName, objectName));
        }

        public void AddColumnToIgnore(string schemaName, string objectName, string columnName)
        {
            _objectsToCompare.Find(
                delegate(ObjectToCompare item) { return item.SchemaName == schemaName && item.ObjectName == objectName; }).AddColumnToIgnore(
                columnName);
        }

        public void Dispose()
        {
            if(_transaction != null)
                EndTestTransaction();

            _dataComparer.Dispose();
        }

        private void RunComparison()
        {
            _results.Reset();

            foreach(ObjectToCompare objectToCompare in _objectsToCompare)
                _dataComparer.Compare(objectToCompare, _transaction, _results);

            _results.AcceptChanges();
        }
    }
}