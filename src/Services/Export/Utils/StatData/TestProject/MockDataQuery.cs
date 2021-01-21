using System.Globalization;
using StatData.Core;

namespace TestProject
{
    internal class MockDataQuery : IDataQuery
    {
        private readonly int _obs;
        private readonly int _vars;

        public MockDataQuery(int obs, int vars)
        {
            _obs = obs;
            _vars = vars;
        }

        #region Implementation of IDataQuery

        public string GetValue(long row, int col)
        {
            return (row * col).ToString(CultureInfo.InvariantCulture);
        }

        public int GetRowCount()
        {
            return _obs;
        }

        public int GetColCount()
        {
            return _vars;
        }

        public string GetName()
        {
            return "MatrixData.tab";
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
