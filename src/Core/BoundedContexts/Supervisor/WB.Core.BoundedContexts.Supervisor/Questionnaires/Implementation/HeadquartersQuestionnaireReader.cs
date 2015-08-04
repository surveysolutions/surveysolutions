using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Supervisor.Extensions;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.SurveySolutions.Services;

namespace WB.Core.BoundedContexts.Supervisor.Questionnaires.Implementation
{
    internal class HeadquartersQuestionnaireReader : HeadquartersEntityReader, IHeadquartersQuestionnaireReader
    {
        public HeadquartersQuestionnaireReader(IJsonUtils jsonUtils, IHeadquartersSettings headquartersSettings, Func<HttpMessageHandler> messageHandler)
            : base(jsonUtils, headquartersSettings, messageHandler) {}

        public async Task<QuestionnaireDocument> GetQuestionnaireByUri(Uri headquartersQuestionnaireUri)
        {
            return await GetEntityByUri<QuestionnaireDocument>(headquartersQuestionnaireUri).ConfigureAwait(false);
        }

        public async Task<byte[]> GetAssemblyByUri(Uri uri)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.AppendAuthToken(this.headquartersSettings);

                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
                httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-msdownload"));

                HttpResponseMessage response = await httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return null;

                byte[] result = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

                return result;
            }
        }
    }
}