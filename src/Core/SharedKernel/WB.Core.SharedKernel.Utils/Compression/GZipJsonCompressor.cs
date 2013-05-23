// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GZipJsonCompressor.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace WB.Core.SharedKernel.Utils.Compression
{
    using System.IO;
    using System.IO.Compression;
    using System.Text;

    using Newtonsoft.Json;

    /// <summary>
    /// Compress/Decompress json objects with GZip archiever.
    /// </summary>
    public class GZipJsonCompressor : IStringCompressor
    {
        #region Public Methods and Operators

        /// <summary>
        /// Compress data.
        /// </summary>
        /// <param name="data">
        /// Deserializable json object as string.
        /// </param>
        /// <returns>
        /// Zip archieve as stream <see cref="Stream"/>.
        /// </returns>
        public Stream Compress(string data)
        {
            var output = new MemoryStream();

            using (var zip = new GZipStream(output, CompressionMode.Compress, true))
            {
                using (var writer = new StreamWriter(zip, Encoding.UTF8))
                {
                    writer.Write(data);
                }
            }

            output.Seek(0, SeekOrigin.Begin);

            return output;
        }

        /// <summary>
        /// Decompress data.
        /// </summary>
        /// <param name="stream">
        /// Zip stream.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// <see cref="T"/>.
        /// </returns>
        public T Decompress<T>(Stream stream) where T : class
        {
            using (var zip = new GZipStream(stream, CompressionMode.Decompress))
            {
                using (var reader = new StreamReader(zip, Encoding.UTF8))
                {
                    return JsonConvert.DeserializeObject<T>(
                        reader.ReadToEnd(), new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects });
                }
            }
        }

        #endregion
    }
}