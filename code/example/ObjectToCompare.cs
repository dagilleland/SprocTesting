using System.Collections.Generic;

namespace UnitTestingTools
{
    internal class ObjectToCompare
    {
        private readonly string _schemaName;
        private readonly string _objectName;

        private readonly List<string> _columnsToIgnore = new List<string>();

        public ObjectToCompare(string schemaName, string objectName)
        {
            _schemaName = schemaName;
            _objectName = objectName;
        }

        public IEnumerable<string> ColumnsToIgnore
        {
            get { return _columnsToIgnore; }
        }

        public string ObjectName
        {
            get { return _objectName; }
        }

        public string SchemaName
        {
            get { return _schemaName; }
        }

        public string Qualified
        {
            get { return _schemaName + "." + _objectName; }
        }

        public void AddColumnToIgnore(string columnName)
        {
            _columnsToIgnore.Add(columnName);
        }
    }
}