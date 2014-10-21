using System.Collections.Generic;

namespace DatabaseUnitTesting.Utilities
{
    internal class ObjectsToCompare
    {
        private readonly string _schema1Name;
        private readonly string _object1Name;
        private readonly string _schema2Name;
        private readonly string _object2Name;

        private readonly List<string> _columnsToIgnore = new List<string>();

        public ObjectsToCompare(string schema1Name, string object1Name, string schema2Name,
                                  string object2Name)
        {
            _schema1Name = schema1Name;
            _object1Name = object1Name;
            _schema2Name = schema2Name;
            _object2Name = object2Name;
        }

        public IEnumerable<string> ColumnsToIgnore
        {
            get { return _columnsToIgnore; }
        }

        public string Object1Name
        {
            get { return _object1Name; }
        }

        public string Schema1Name
        {
            get { return _schema1Name; }
        }

        public string Object2Name
        {
            get { return _object2Name; }
        }

        public string Schema2Name
        {
            get { return _schema2Name; }
        }

        public string Qualified1
        {
            get { return _schema1Name + "." + _object1Name; }
        }

        public string Qualified2
        {
            get { return _schema2Name + "." + _object2Name; }
        }

        public void AddColumnToIgnore(string columnName)
        {
            _columnsToIgnore.Add(columnName);
        }
    }
}