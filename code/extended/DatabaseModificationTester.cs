using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using DatabaseUnitTesting.Utilities;

namespace DatabaseUnitTesting
{
    public class DatabaseModificationTester : IDisposable
    {
        private readonly DatabaseComparer _dataComparer;
        private SqlTransaction _transaction;
        private SqlConnection _connection;
         private readonly string _snapshotName;
        private readonly DatabaseAdapter _databaseAdapter;
        private bool _activeSnapshot = false;

        public DatabaseModificationTester(SqlConnection connection, string databaseName, string snapshotName)
        {
            _connection = connection;
            _snapshotName = snapshotName;
            _dataComparer = new DatabaseComparer(connection, databaseName, snapshotName);
            _databaseAdapter = new DatabaseAdapter(connection);
            _databaseAdapter.CreateSnapshot(databaseName, snapshotName);
            _activeSnapshot = true;
        }

        public SqlTransaction BeginTestTransaction()
        {
            if (_transaction != null)
                EndTestTransaction();

            return (_transaction = _connection.BeginTransaction());
        }

        public void EndTestTransaction()
        {
            if (_transaction == null)
                throw new InvalidOperationException("Transaction does not exist");

            _dataComparer.CleanUp();
            _transaction.Rollback();
            _transaction.Dispose();
            _transaction = null;
        }

        public void WriteDiffsToXml(string filename)
        {
            XmlFileAdapter.Write(filename, _dataComparer.GenerateDifferences(_transaction));
        }

        public bool CompareDiffsToXml(string filename)
        {
            return XmlFileAdapter.Read(filename).Equals(_dataComparer.GenerateDifferences(_transaction));
        }

        public bool AreEqual()
        {
            return _dataComparer.GenerateDifferences(_transaction).TableCount == 0;
        }

        public void AddColumnToIgnore(string schemaName, string objectName, string columnName)
        {
            _dataComparer.AddColumnToIgnore(schemaName, objectName, columnName);
        }

        public void AddColumnsToIgnore(string schema1, string name1, List<string> columnNames)
        {
            _dataComparer.AddColumnsToIgnore(schema1, name1, columnNames);
        }
    
        public void AddObjectComparison(string schemaName, string tableName)
        {
            _dataComparer.AddObjectComparison(schemaName, tableName, schemaName, tableName);
        }

        public void Dispose()
        {
            if(_activeSnapshot)
            {
                _activeSnapshot = false;
                _databaseAdapter.DropSnapshot(_snapshotName);
            }
        }
    }
}