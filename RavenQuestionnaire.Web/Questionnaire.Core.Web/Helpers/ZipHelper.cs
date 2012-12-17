// -----------------------------------------------------------------------
// <copyright file="ZipHelper.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Questionnaire.Core.Web.Helpers
{
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
        /// Zipped file or null
        /// </returns>
        public static ZipFile ZipFileCheck(HttpRequestBase request, HttpPostedFileBase uploadFile)
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

            var zip = ZipFile.Read(uploadFile.InputStream);
            return zip;
        }
    }
}
