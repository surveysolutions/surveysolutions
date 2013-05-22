// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IZipUtils.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace WB.UI.Shared.Compression
{
    using System.IO;

    /// <summary>
    ///     The ZipUtils interface.
    /// </summary>
    public interface IZipUtils
    {
        #region Public Methods and Operators

        /// <summary>
        /// The un zip.
        /// </summary>
        /// <param name="stream">
        /// The stream.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
        T UnZip<T>(Stream stream) where T : class;

        /// <summary>
        /// The zip.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <returns>
        /// The <see>
        ///         <cref>byte[]</cref>
        ///     </see>
        ///     .
        /// </returns>
        Stream Zip(string data);

        #endregion
    }
}