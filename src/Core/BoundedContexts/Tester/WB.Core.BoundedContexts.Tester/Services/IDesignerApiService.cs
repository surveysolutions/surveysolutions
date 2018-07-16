using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.Questionnaire.Api;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;
using QuestionnaireListItem = WB.Core.BoundedContexts.Tester.Views.QuestionnaireListItem;

namespace WB.Core.BoundedContexts.Tester.Services
{
    public interface IDesignerApiService
    {
        Task<bool> Authorize(string login, string password);
        Task<IReadOnlyCollection<QuestionnaireListItem>> GetQuestionnairesAsync(CancellationToken token);
        Task<Questionnaire> GetQuestionnaireAsync(string questionnaireId, IProgress<TransferProgress> transferProgress, CancellationToken token);
        Task<AttachmentContent> GetAttachmentContentAsync(string attachmentId, IProgress<TransferProgress> transferProgress, CancellationToken token);
        Task<TranslationDto[]> GetTranslationsAsync(string questionnaireId, CancellationToken token);
    }
}
