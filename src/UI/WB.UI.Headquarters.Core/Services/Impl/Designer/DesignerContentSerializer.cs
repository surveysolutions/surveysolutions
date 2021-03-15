using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Refit;
using WB.Core.Infrastructure.HttpServices.HttpClient;

namespace WB.UI.Headquarters.Services.Impl
{
    /// <summary>
    /// JsonContentSerializer with ability to handle RestFile result separately
    /// </summary>
    internal class DesignerContentSerializer : IHttpContentSerializer
    {
        NewtonsoftJsonContentSerializer json = new();

        public async Task<RestFile> AsRestFileAsync(HttpContent content)
        {
            var rawContentType = content?.Headers?.ContentType?.MediaType;
            var length = content?.Headers?.ContentLength;
            var fileName = content?.Headers?.ContentDisposition?.FileName;
            var fileContent = await content.ReadAsByteArrayAsync();

            return new RestFile(content: fileContent, contentType: rawContentType,
                null, contentLength: length, fileName: fileName, HttpStatusCode.OK);
        }

        public HttpContent ToHttpContent<T>(T item)
        {
            return json.ToHttpContent(item);
        }

        public async Task<T> FromHttpContentAsync<T>(HttpContent content, CancellationToken cancellationToken = default)
        {
            if (typeof(T) == typeof(RestFile))
            {
                object result = await AsRestFileAsync(content);
                return (T)result;
            }

            return await json.FromHttpContentAsync<T>(content, cancellationToken);
        }

        public string GetFieldNameForProperty(PropertyInfo propertyInfo)
        {
            return json.GetFieldNameForProperty(propertyInfo);
        }
    }
}
