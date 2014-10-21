using System;
using System.Collections.Generic;
using System.Security;
using System.Text;
using System.Xml;

namespace DatabaseUnitTesting.Utilities
{
    internal static class XmlFileAdapter
    {
        private const string LEFT = "\0L;;";
        private const string RIGHT = "\0R;;";

        internal static Database Read(string filename)
        {
            XmlDocument document = new XmlDocument();
            Database database = new Database();

            document.Load(filename);
            XmlNode xmlRoot = document.LastChild;
            
            if(xmlRoot == null)
                return database;

            foreach(Table diff in ReadTables(xmlRoot))
                database.AddTable(diff);

            return database;
        }

        internal static List<Table> ReadTables(XmlNode xmlRoot)
        {
            List<Table> tableDiffs = new List<Table>();
            for(XmlNode xmlObject = xmlRoot.FirstChild; xmlObject != null; xmlObject = xmlObject.NextSibling)
            {
                Table tableDiff;
                XmlAttribute name1Attribute = xmlObject.Attributes["name1"];
                XmlAttribute name2Attribute = xmlObject.Attributes["name2"];

                if(name1Attribute == null || name2Attribute == null)
                    throw new XmlSyntaxException("Tables must have name1 and name2 attributes");

                tableDiff = new Table(name1Attribute.Value, name2Attribute.Value);

                Row schema = new Row("schema");
                ReadColumns(xmlObject.FirstChild, schema);

                tableDiff.Schema = schema;

                foreach(Row row in ReadRows(xmlObject))
                {
                    tableDiff.AddRow(row);
                }

                tableDiffs.Add(tableDiff);
            }
            return tableDiffs;
        }

        internal static List<Row> ReadRows(XmlNode xmlTable)
        {
            List<Row> rowDiffs = new List<Row>();
            for(XmlNode xmlRow = xmlTable.FirstChild.NextSibling; xmlRow != null; xmlRow = xmlRow.NextSibling)
            {
                XmlAttribute typeAttribute = xmlRow.Attributes["type"];

                if(typeAttribute == null)
                    throw new XmlSyntaxException("Row does not have a 'type' attribute");

                Row row = new Row(typeAttribute.Value);
                ReadColumns(xmlRow, row);
                rowDiffs.Add(row);
            }
            return rowDiffs;
        }

        internal static void ReadColumns(XmlNode xmlRow, Row row)
        {
            for(XmlNode column = xmlRow.FirstChild; column != null; column = column.NextSibling)
            {
                XmlAttribute nameAttribute = column.Attributes["name"];

                if(nameAttribute == null)
                    throw new XmlSyntaxException("Fields and Keys must have 'name' attributes");

                string name = nameAttribute.Value.ToLower();

                if(column.Name.ToLower().Equals("column"))
                {
                    XmlAttribute valueAttribute = column.Attributes["value"];
                    if(valueAttribute == null)
                        throw new XmlSyntaxException("Columns must have 'value' attribute");

                    row.AddColumn(new Column(name, valueAttribute.Value.Replace(LEFT, "<").Replace(RIGHT, ">")));
                } else
                    throw new XmlSyntaxException("Rows may only contain 'column' children");
            }
        }

        internal static void Write(string filename, Database diffs)
        {
            using(XmlTextWriter writer = new XmlTextWriter(filename, Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;
                writer.WriteStartDocument();
                writer.WriteStartElement("results");

                foreach(KeyValuePair<Table, int> table in diffs.Tables)
                    for(int i = 0; i < table.Value; i++)
                        WriteTable(writer, table.Key);
                
                writer.WriteEndDocument();
            }
        }

        internal static void WriteTable(XmlTextWriter writer, Table tableDiff)
        {
            writer.WriteStartElement("object");

            writer.WriteAttributeString("name1", tableDiff.Name1);
            writer.WriteAttributeString("name2", tableDiff.Name2);
          

            WriteRow(writer, tableDiff.Schema.KeyString);

            foreach(KeyValuePair<string, int> row in tableDiff.Rows)
            {
                for (int i = 0; i < row.Value; i++ )
                    WriteRow(writer, row.Key);
            }

            writer.WriteEndElement();
        }

        internal static void WriteRow(XmlTextWriter writer, string rowDiff)
        {
            writer.WriteStartElement("row");
            string[] columns = rowDiff.Split(new string[] {Row.DELIMITER}, StringSplitOptions.None);

            writer.WriteAttributeString("type", columns[0]);
            for(int i = 1; i < columns.Length; i++)
                WriteColumn(writer, columns[i]);

            writer.WriteEndElement();
        }

        internal static void WriteColumn(XmlTextWriter writer, string column)
        {
            string[] definition = column.Split(new string[] {Column.DELIMITER}, StringSplitOptions.None);

            writer.WriteStartElement("column");
            writer.WriteAttributeString("name", definition[0]);
            writer.WriteAttributeString("value", definition[1].Replace("<", LEFT).Replace(">", RIGHT));
            writer.WriteEndElement();
        }
    }
}