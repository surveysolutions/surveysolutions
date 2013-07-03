// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ZipHelper.cs" company="">
//   
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Text.RegularExpressions;
using Ionic.Zip;
using Ionic.Zlib;
using Newtonsoft.Json;

namespace Questionnaire.Core.Web.Helpers
{
    using System.Collections.Generic;
    using System.Web;

    using SynchronizationMessages.Export;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public static class ZipHelper
    {
        #region Public Methods and Operators

        /// <summary>
        /// Gets uploaded file and tries to convert it to zip file
        /// </summary>
        /// <param name="request">
        /// The request
        /// </param>
        /// <param name="uploadFile">
        /// The upload file.
        /// </param>
        /// <returns>
        /// List of serialized file contents
        /// </returns>
        public static List<string> ZipFileReader(HttpRequestBase request, HttpPostedFileBase uploadFile)
        {
            if (uploadFile == null && request.Files.Count > 0)
            {
                uploadFile = request.Files[0];
            }

            if (uploadFile == null || uploadFile.ContentLength == 0)
            {
                return null;
            }

            return ZipManager.GetZipContent(uploadFile.InputStream);
        }

        #endregion

        public static byte[] Compress(object data)
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
            var dataAsString= JsonConvert.SerializeObject(data, Formatting.Indented, settings);

            var outputStream = new MemoryStream();
            using (var zip = new ZipFile())
            {
                // var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
                zip.CompressionLevel = CompressionLevel.BestCompression;
                zip.AddEntry("data.txt", dataAsString);
                zip.Save(outputStream);
            }

            outputStream.Seek(0, SeekOrigin.Begin);
            return outputStream.ToArray();
        }

    }
}