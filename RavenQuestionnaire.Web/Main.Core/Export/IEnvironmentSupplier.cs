namespace Main.Core.Export
{
    using System.Collections.Generic;

    /// <summary>
    /// The EnvironmentSupplier interface.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public interface IEnvironmentSupplier<T>
    {
        #region Public Methods and Operators

        /// <summary>
        /// The add completed results.
        /// </summary>
        /// <param name="container">
        /// The container.
        /// </param>
        void AddCompletedResults(IDictionary<string, byte[]> container);

        /// <summary>
        /// The build content.
        /// </summary>
        /// <param name="result">
        /// The result.
        /// </param>
        /// <param name="parentPrimaryKeyName">
        /// The parent primary key name.
        /// </param>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        string BuildContent(T result, string parentPrimaryKeyName, string fileName, FileType type);

        #endregion
    }
}