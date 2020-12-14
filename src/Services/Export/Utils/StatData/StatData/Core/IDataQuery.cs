using System;

namespace StatData.Core
{
    /// <summary>
    /// Interface for accessing data from arbitrary sources
    /// </summary>
    public interface IDataQuery: IDisposable
    {
        /// <summary>
        /// Request data from a particular cell of the dataset.
        /// </summary>
        /// <param name="row">row (observation number).</param>
        /// <param name="col">column (variable number).</param>
        /// <returns>Content of the dataset in the specified cell in string format.</returns>
        string GetValue(Int64 row, int col);

        /// <summary>
        /// Returns number of rows (observations) in the dataset.
        /// </summary>
        /// <returns>Number of rows.</returns>
        int GetRowCount();
        // todo: this may be incompatible with Stata's unsigned types

        /// <summary>
        /// Returns number of columns (variables) in the dataset.
        /// </summary>
        /// <returns>Number of columns.</returns>
        int GetColCount();

        string GetName();
    }

    interface INamedDataQuery:IDataQuery
    {
        string[] GetVarNames();
    }
}
