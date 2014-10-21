using System;

namespace DatabaseUnitTesting.Utilities
{
    internal class Column
    {
        private readonly string _name;
        private readonly string _value;
        private readonly string _sortString;
        public const string DELIMITER = "\x1f;;";

        public Column(string name, object value)
        {
            _name = name;
            _value = Convert(value);
            _sortString = String.Concat(_name.ToLower(), DELIMITER, _value);
        }

        public static string Convert(object value)
        {
            if(value is byte[])
            {
                string[] binary = new string[((byte[]) value).Length + 1];
                binary[0] = "0x";
                for(int i = 1; i < binary.Length; i++)
                    binary[i] = ((byte[]) value)[i - 1].ToString("X1");

                return String.Join("", binary);
            }

            if(value is DateTime)
            {
                string time = ((DateTime) value).ToShortDateString() + " ";
                time += ((DateTime) value).TimeOfDay;
                return time.TrimEnd('0').TrimEnd(':');
            }

            return value.ToString();
        }

        public string Name
        {
            get { return _name; }
        }

        public string Value
        {
            get { return _value; }
        }

        public string SortString
        {
            get { return _sortString; }
        }
    }
}