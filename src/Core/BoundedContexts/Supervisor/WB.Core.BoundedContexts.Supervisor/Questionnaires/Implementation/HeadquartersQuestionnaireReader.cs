using System;
using System.Net.Http;
using System.Threading.Tasks;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation;
using WB.Core.SharedKernel.Utils.Serialization;

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
    }
}