using System;

namespace StatData.Core
{
    internal class StringArrayDataQuery:IDataQuery
    {
        private readonly string[,] _data;
        public StringArrayDataQuery(string[,] data)
        {
            _data = data;
        }

        #region Implementation of IDataQuery

        public string GetValue(Int64 row, int col)
        {
            return col >= GetColCount()
                       ? String.Empty           // not sure why this is necessary
                       : _data[row, col];
        }

        public int GetRowCount()
        {
            return _data.GetLength(0);
        }

        public int GetColCount()
        {
            return _data.GetLength(1);
        }

        public string GetName()
        {
            return "data.tab";
        }
        #endregion

        #region Implementation of IDisposable

        public void Dispose()
        {
            // nothing to dispose of
        }

        #endregion
    }
}
