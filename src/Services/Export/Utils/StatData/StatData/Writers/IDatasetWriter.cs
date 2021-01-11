using System;
using System.IO;
using StatData.Core;

namespace StatData.Writers
{
    /// <summary>
    /// All dataset writers provide this functionality.
    /// </summary>
    public interface IDatasetWriter
    {
        /// <summary>
        /// Writes data to native binary file
        /// </summary>
        /// <param name="filename">Name of file on disk to be created</param>
        /// <param name="datasetMeta">Meta information</param>
        /// <param name="data">Data</param>
        void WriteToFile(String filename, IDatasetMeta datasetMeta, string[,] data);

        /// <summary>
        /// Writes data to native binary stream (does not close the stream)
        /// </summary>
        /// <param name="stream">Open stream to write to</param>
        /// <param name="meta">Meta information</param>
        /// <param name="data">Data</param>
        void WriteToStream(Stream stream, IDatasetMeta meta, string[,] data);

        /// <summary>
        /// Writes data to native binary file
        /// </summary>
        /// <param name="filename">Name of file on disk to be created</param>
        /// <param name="data">Data</param>
        void WriteToFile(String filename, string[,] data);

        /// <summary>
        /// Writes data to native binary stream
        /// </summary>
        /// <param name="stream">Open stream to write to</param>
        /// <param name="data">Data</param>
        void WriteToStream(Stream stream, string[,] data);

        /// <summary>
        /// Writes data to native binary stream utilizing a query mechanism
        /// </summary>
        /// <param name="stream">Open stream to write to.</param>
        /// <param name="meta">Meta information.</param>
        /// <param name="data">Object responding to data queries.</param>
        void WriteToStream(Stream stream, IDatasetMeta meta, IDataQuery data);

        /// <summary>
        /// Writes data to native binary stream utilizing a query mechanism
        /// </summary>
        /// <param name="stream">Open stream to write to</param>
        /// <param name="data">Object responding to data queries.</param>
        void WriteToStream(Stream stream, IDataQuery data);

        /// <summary>
        /// Writes data to native binary stream utilizing a query mechanism
        /// </summary>
        /// <param name="filename">Name of file on disk to be created</param>
        /// <param name="data">Object responding to data queries.</param>
        void WriteToFile(string filename, IDataQuery data);

        /// <summary>
        /// Writes data to native binary stream utilizing a query mechanism
        /// </summary>
        /// <param name="filename">Name of file on disk to be created</param>
        /// <param name="meta">Meta information.</param>
        /// <param name="data">Object responding to data queries.</param>
        void WriteToFile(string filename, IDatasetMeta meta, IDataQuery data);

        /// <summary>
        /// Event signaling writing progress
        /// </summary>
        event ProgressChangedDelegate OnProgressChanged;
    }
}