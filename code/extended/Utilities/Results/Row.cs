using System;
using System.Collections.Generic;

namespace DatabaseUnitTesting.Utilities
{
    internal class Row
    {
        private readonly string _type;
        private readonly List<Column> _columns = new List<Column>();
        private string _keyString;
        private bool _keyValid = false;
        public const string DELIMITER = "\x1e;;";

        public Row(string type)
        {
            _type = type.ToLower();
        }

        public void AddColumn(Column column)
        {
            _columns.Add(column);
            _keyValid = false;
        }

        public string KeyString
        {
            get
            {
                if(!_keyValid)
                {
                    string[] keyString = new string[_columns.Count + 1];
                    keyString[0] = _type;
                    for(int i = 1; i < keyString.Length; i++)
                        keyString[i] = _columns[i - 1].SortString;

                    _keyString = String.Join(DELIMITER, keyString);
                    _keyValid = true;
                }
                return _keyString;
            }
        }

        public int ColumnCount
        {
            get { return _columns.Count; }
        }
    }
}