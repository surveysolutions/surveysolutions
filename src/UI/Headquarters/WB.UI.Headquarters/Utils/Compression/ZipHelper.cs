using System.Collections.Generic;
using System.IO;
using System.Web;
using Ionic.Zip;
using Ionic.Zlib;
using Newtonsoft.Json;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Utils.Compression
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public static class ZipHelper
    {
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
    }
}