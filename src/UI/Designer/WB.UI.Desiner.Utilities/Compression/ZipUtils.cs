// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ZipUtils.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace WB.UI.Desiner.Utilities.Compression
{
    using System.IO;
    using System.IO.Compression;

    using Newtonsoft.Json;

    /// <summary>
    /// The zip utils.
    /// </summary>
    public class ZipUtils : IZipUtils
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
        public T UnZip<T>(Stream stream) where T : class
        {
            using (var zip = new GZipStream(stream, CompressionMode.Decompress))
            {
                using (var reader = new StreamReader(zip, System.Text.Encoding.UTF8))
                {
                    return JsonConvert.DeserializeObject<T>(
                        reader.ReadToEnd(), new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects });
                }
            }
        }

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
        public Stream Zip(string data)
        {
            var output = new MemoryStream();

            using (var zip = new GZipStream(output, CompressionMode.Compress, true))
            {
                using (var writer = new StreamWriter(zip, System.Text.Encoding.UTF8))
                {
                    writer.Write(data);
                }
            }

            output.Seek(0, SeekOrigin.Begin);

            return output;
        }

        #endregion
    }
}