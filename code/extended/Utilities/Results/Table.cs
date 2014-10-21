using System;
using System.Collections.Generic;

namespace DatabaseUnitTesting.Utilities
{
    internal class Table
    {
        private readonly string _name1;
        private readonly string _name2;
        private Row _schema = new Row("schema");
        private readonly Dictionary<string, int> rows = new Dictionary<string, int>();
        
        private int _hashCode;
        private int _rowCount;
        private int _fieldCount;

        public Table(string name1): this(name1, String.Empty)
        {}

        public Table(string name1, string name2)
        {
            _name1 = name1.ToLower();
            _name2 = name2.ToLower();
        }
            

        public override bool Equals(object otherObject)
        {
            if(!(otherObject is Table))
                return false;

            Table other = (Table) otherObject;

            if(GetHashCode() != other.GetHashCode())
                return false;

            if (RowCount != other.RowCount ||
               FieldCount != other.FieldCount ||
               !Schema.KeyString.Equals(other.Schema.KeyString))
                return false;

            int otherCount;

            foreach(string row in rows.Keys)
                if(!other.LookupRow(row, out otherCount) ||
                   otherCount != rows[row])
                    return false;

            return true;
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public string Name1
        {
            get { return _name1; }
        }

        public string Name2
        {
            get { return _name2; }
        }

        public void AddRow(Row row)
        {
            string key = row.KeyString;
            if(rows.ContainsKey(key))
                rows[key]++;
            else
                rows.Add(key, 1);

            _rowCount = _rowCount + 1;
            _hashCode = _hashCode + key.GetHashCode();
        }

        public IEnumerable<KeyValuePair<string, int>> Rows
        {
            get { return rows; }
        }

        public int RowCount
        {
            get { return _rowCount; }
        }

        public int FieldCount
        {
            get { return _fieldCount; }
        }

        public Row Schema
        {
            get { return _schema; }
            set
            {
                _schema = value;
                _fieldCount = _schema.ColumnCount;
            }
        }

        public void AddField(string name, string type)
        {
            Column c = new Column(name, type.ToLower());
            _schema.AddColumn(c);
            _fieldCount++;
           // _hashCode = _hashCode + c.SortString.GetHashCode();
        }

        public bool LookupRow(string key, out int other)
        {
            return rows.TryGetValue(key, out other);
        }
    }
}