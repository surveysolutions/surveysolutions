// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IStringCompressor.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace WB.Core.SharedKernel.Utils.Compression
{
    using System.IO;

    /// <summary>
    /// The StringCompressor interface.
    /// </summary>
    public interface IStringCompressor
    {
        /// <summary>
        /// Compress string data to a zip archieve
        /// </summary>
        /// <param name="data">
        ///  Any data in string.
        /// </param>
        /// <returns>
        /// Zip archieve as stream <see cref="Stream"/>.
        /// </returns>
        Stream Compress(string data);

        /// <summary>
        /// Decompress text from zip stream and serialize it to a .Net object
        /// </summary>
        /// <param name="stream">
        /// Zip archieve input stream.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
        T Decompress<T>(Stream stream) where T : class;
    }
}