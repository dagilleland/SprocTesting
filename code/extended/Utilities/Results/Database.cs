using System.Collections.Generic;

namespace DatabaseUnitTesting.Utilities
{
    internal class Database
    {
        private int _tableCount = 0;
        private int _hashCode = 0;
        private readonly Dictionary<Table, int> _tables = new Dictionary<Table, int>();

        public int TableCount
        {
            get { return _tableCount; }
        }

        public IEnumerable<KeyValuePair<Table, int>> Tables
        {
            get { return _tables; }
        }

        public void AddTable(Table table)
        {
            if(_tables.ContainsKey(table))
                _tables[table]++;
            else
                _tables.Add(table, 1);

            _tableCount++;
            _hashCode = _hashCode + table.GetHashCode();
        }

        public bool ContainsTable(Table table)
        {
            return _tables.ContainsKey(table);
        }

        public int GetCount(Table table)
        {
            return _tables[table];
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override bool Equals(object otherObject)
        {
            if(!(otherObject is Database))
                return false;

            Database other = (Database) otherObject;

            if(TableCount != other.TableCount ||
               GetHashCode() != other.GetHashCode())
                return false;

            foreach(KeyValuePair<Table, int> pair in _tables)
                if(!other.ContainsTable(pair.Key) ||
                   other.GetCount(pair.Key) != pair.Value)
                    return false;

            return true;
        }
    }
}