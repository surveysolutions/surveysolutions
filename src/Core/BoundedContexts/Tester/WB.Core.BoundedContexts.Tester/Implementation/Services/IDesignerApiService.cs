using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;
using QuestionnaireListItem = WB.Core.BoundedContexts.Tester.Views.QuestionnaireListItem;

namespace WB.Core.BoundedContexts.Tester.Implementation.Services
{
    public interface IDesignerApiService
    {
        Task<bool> Authorize(string login, string password);
        Task<IReadOnlyCollection<QuestionnaireListItem>> GetQuestionnairesAsync(CancellationToken token);
        Task<Questionnaire> GetQuestionnaireAsync(QuestionnaireListItem selectedQuestionnaire, Action<DownloadProgressChangedEventArgs> onDownloadProgressChanged, CancellationToken token);
    }
}