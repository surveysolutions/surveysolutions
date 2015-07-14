using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;
using QuestionnaireListItem = WB.Core.BoundedContexts.QuestionnaireTester.Views.QuestionnaireListItem;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Services
{
    public interface IDesignerApiService
    {
        Task<bool> Authorize(string login, string password);
        Task<IList<QuestionnaireListItem>> GetQuestionnairesAsync(bool isPublic, CancellationToken token);
        Task<Questionnaire> GetQuestionnaireAsync(QuestionnaireListItem selectedQuestionnaire, Action<decimal> downloadProgress, CancellationToken token);
    }
}