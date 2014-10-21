using System;
using System.Data;

namespace UnitTestingTools
{
    internal static class DataSetComparer
    {
        internal static bool Compare(DataSet one, DataSet two)
        {
            if(one.Tables.Count !=
               two.Tables.Count)
                return false;

            for(int i = 0; i < one.Tables.Count; i++)
                if(!CompareTables(one.Tables[i], two.Tables[i]))
                    return false;

            return true;
        }

        private static bool CompareTables(DataTable one, DataTable two)
        {
            if(one.Rows.Count !=
               two.Rows.Count)
                return false;

            for(int i = 0; i < one.Rows.Count; i++)
                if(!CompareRows(one.Rows[i], two.Rows[i]))
                    return false;

            return true;
        }

        private static bool CompareRows(DataRow one, DataRow two)
        {
            if(one.ItemArray.Length !=
               two.ItemArray.Length)
                return false;

            for(int i = 0; i < one.ItemArray.Length; i++)
                if(!CompareItems(one.ItemArray[i], two.ItemArray[i]))
                    return false;

            return true;
        }

        private static bool CompareItems(object value1, object value2)
        {
            if(value1.GetType() !=
               value2.GetType())
                return false;

            if(value1 is DBNull)
                return true;

            if(value1 is DateTime)
                return ((DateTime) value1).CompareTo((DateTime) value2) == 0;

            if(value1 is byte[])
            {
                if(((byte[]) value1).Length !=
                   ((byte[]) value2).Length)
                    return false;

                for(int i = 0; i < ((byte[]) value1).Length; i++)
                    if(((byte[]) value1)[i] !=
                       ((byte[]) value2)[i])
                        return false;

                return true;
            }

            return value1.ToString().Equals(value2.ToString());
        }
    }
}