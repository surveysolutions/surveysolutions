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
    public interface IDesignerApi
    {
        [Post("/v3/questionnaires/revision/{id}/tag")]
        Task Tag(Guid id, [Body] DesignerApiTagModel model);

        [Get("/v3/questionnaires/{id}")]
        Task<QuestionnaireCommunicationPackage> GetQuestionnaire(
            Guid id,
            int clientQuestionnaireContentVersion,
            [Query] int? minSupportedQuestionnaireVersion);

        [Get("/attachment/{contentId}")]
        Task<HttpResponseMessage> DownloadQuestionnaireAttachment(string contentId, [Query] Guid attachmentId);

        [Get("/translations/{questionnaireId}")]
        Task<List<TranslationDto>> GetTranslations(Guid questionnaireId);

        [Get("/lookup/{questionnaireId}/{lookupId}")]
        Task<QuestionnaireLookupTable> GetLookupTables(Guid questionnaireId, Guid lookupId);

    }

}
