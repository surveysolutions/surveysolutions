using System;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    public class QuestionnaireHistoryRevertService : IQuestionnaireHistoryRevertService
    {
        private readonly IAnonymousQuestionnaireStateService anonymousQuestionnaireStateService;
        private readonly IQuestionnaireViewFactory questionnaireViewFactory;

        public QuestionnaireHistoryRevertService(
            IAnonymousQuestionnaireStateService anonymousQuestionnaireStateService,
            IQuestionnaireViewFactory questionnaireViewFactory)
        {
            this.anonymousQuestionnaireStateService = anonymousQuestionnaireStateService;
            this.questionnaireViewFactory = questionnaireViewFactory;
        }

        public async Task<bool> TryRevertAsync(
            Guid questionnaireId,
            QuestionnaireChangeRecord historicalRecord,
            Guid responsibleId,
            string responsibleName)
        {
            if (historicalRecord.ActionType != QuestionnaireActionType.AnonymousSharingEnabled
                && historicalRecord.ActionType != QuestionnaireActionType.AnonymousSharingDisabled)
            {
                return false;
            }

            var questionnaireTitle = historicalRecord.TargetItemTitle
                ?? this.questionnaireViewFactory.Load(new QuestionnaireViewInputModel(questionnaireId))?.Title
                ?? string.Empty;

            await this.anonymousQuestionnaireStateService.SaveStateAsync(
                questionnaireId,
                historicalRecord.ActionType == QuestionnaireActionType.AnonymousSharingEnabled,
                questionnaireTitle,
                responsibleId,
                responsibleName);

            return true;
        }
    }
}
