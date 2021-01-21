using StatData.Core;

//! Ready-made conversion tools utilizing readers and writers in of this library 
namespace StatData.Converters
{
    /// <summary>
    /// Dataset converters must implement the following basic functionality
    /// </summary>
    interface IDatasetConverter
    {
        /// <summary>
        /// Converts a dataset to the new format
        /// </summary>
        /// <param name="srcFile">Source file (must exist)</param>
        /// <param name="dstFile">New file (will be created)</param>
        void Convert(string srcFile, string dstFile);

        /// <summary>
        /// Converts a dataset to the new format following the meta specification
        /// </summary>
        /// <param name="srcFile">Source file (must exist)</param>
        /// <param name="dstFile">New file (will be created)</param>
        /// <param name="meta">Meta information to be used for conversion</param>
        /// 
        /// Output file will have variables ordered in the order of variables 
        /// in the source file.
        void Convert(string srcFile, string dstFile, IDatasetMeta meta);

        /// <summary>
        /// Event signaling conversion progress.
        /// </summary>
        event ProgressChangedDelegate OnProgressChanged;
    }
}
