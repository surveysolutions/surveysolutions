using System;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.UI.WebTester.Services
{
    public interface IQuestionnaireImportService
    {
        /// <summary>
        /// Downloads and registers the questionnaire from Designer.
        /// </summary>
        /// <param name="questionnaireId">
        /// The questionnaire's Designer ID — used for all outbound API calls
        /// (e.g., <c>/api/webtester/{questionnaireId}/questionnaire</c>).
        /// </param>
        /// <param name="interviewId">
        /// The unique per-run interview ID — used as the local storage/appdomain key
        /// so that parallel runs of the same questionnaire don't collide.
        /// </param>
        Task<QuestionnaireIdentity> ImportQuestionnaire(Guid questionnaireId, Guid interviewId);

        /// <param name="interviewId">The per-run interview ID passed to <see cref="ImportQuestionnaire"/>.</param>
        void RemoveQuestionnaire(Guid interviewId);
    }
}
