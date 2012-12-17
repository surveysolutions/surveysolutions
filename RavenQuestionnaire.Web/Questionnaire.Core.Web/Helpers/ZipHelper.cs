// -----------------------------------------------------------------------
// <copyright file="ZipHelper.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Questionnaire.Core.Web.Helpers
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Web;

    using Ionic.Zip;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public static class ZipHelper
    {
        /// <summary>
        /// Gets uploaded file and tries to convert it to zip file
        /// </summary>
        /// <param name="request">
        /// The request</param>
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

            if (!ZipFile.IsZipFile(uploadFile.InputStream, false))
            {
                return null;
            }

            uploadFile.InputStream.Position = 0;

            var list = new List<string>();

            using (ZipFile zip = ZipFile.Read(uploadFile.InputStream))
            {
                using (var stream = new MemoryStream())
                {
                    foreach (ZipEntry e in zip)
                    {
                        e.Extract(stream);
                    }

                    list.Add(Encoding.Default.GetString(stream.ToArray()));
                }
            }

            return list;
        }
    }
}
