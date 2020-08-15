using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Refit;
using WB.Core.GenericSubdomains.Portable.Implementation;

namespace WB.UI.Headquarters.Services.Impl
{
    /// <summary>
    /// JsonContentSerializer with ability to handle RestFile result separately
    /// </summary>
    internal class DesignerContentSerializer : IContentSerializer
    {
        NewtonsoftJsonContentSerializer json = new NewtonsoftJsonContentSerializer();

        public async Task<T> DeserializeAsync<T>(HttpContent content)
        {
            if (typeof(T) == typeof(RestFile))
            {
                object result = await AsRestFileAsync(content);
                return (T)result;
            }

            return await json.DeserializeAsync<T>(content);
        }

        public Task<HttpContent> SerializeAsync<T>(T item)
        {
            return json.SerializeAsync(item);
        }

        public async Task<RestFile> AsRestFileAsync(HttpContent content)
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