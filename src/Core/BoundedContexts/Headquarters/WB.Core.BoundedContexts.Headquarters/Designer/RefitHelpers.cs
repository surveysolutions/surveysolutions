using Refit;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.BoundedContexts.Headquarters.Designer
{
    public static class RefitHelpers
    {
        public static async Task<RestFile> AsRestFile(this Task<HttpResponseMessage> responseTask)
        {
            var response = await responseTask;
            if (response.StatusCode == HttpStatusCode.NotModified)
            {
                return new RestFile(null, string.Empty, null, null, null, response.StatusCode);
            }

            var responseMessage = response;

            var contentMD5 = response.Content.Headers.ContentMD5;
            var rawContentType = responseMessage.Content?.Headers?.ContentType?.MediaType;
            var length = responseMessage.Content?.Headers?.ContentLength;
            var eTag = responseMessage.Headers?.ETag?.Tag;
            var fileName = responseMessage.Content?.Headers?.ContentDisposition?.FileName;
            var fileContent = await response.Content.ReadAsByteArrayAsync();

            if (contentMD5 != null)
            {
                using (var crypto = MD5.Create())
                {
                    var hash = crypto.ComputeHash(fileContent);

                    if (!hash.SequenceEqual(contentMD5))
                    {
                        throw new RestException("Downloaded file failed hash check. Please try again");
                    }
                }
            }

            return new RestFile(content: fileContent, contentType: rawContentType,
                contentHash: eTag, contentLength: length, fileName: fileName,
                statusCode: response.StatusCode)
            {
                ContentMD5 = contentMD5
            };
        }

        public static async Task<RestFile> AsRestFileAsync(this HttpContent content)
        {
            var rawContentType = content?.Headers?.ContentType?.MediaType;
            var length = content?.Headers?.ContentLength;
            var fileName = content?.Headers?.ContentDisposition?.FileName;
            var fileContent = await content.ReadAsByteArrayAsync();

            return new RestFile(content: fileContent, contentType: rawContentType,
               null, contentLength: length, fileName: fileName, HttpStatusCode.OK);
        }
    }
}
