using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Repositories;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.EventHandler.WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.UI.Headquarters.Services;

namespace WB.UI.Headquarters.Implementation.Services
{
    internal class RestoreDeletedQuestionnaireProjectionsService : IRestoreDeletedQuestionnaireProjectionsService
    {
        private readonly IQuestionnaireStorage questionnaireStorage;

        private readonly IReferenceInfoForLinkedQuestionsFactory referenceInfoForLinkedQuestionsFactory;
        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage;

        private readonly IPlainKeyValueStorage<ReferenceInfoForLinkedQuestions> referenceInfoForLinkedQuestionsStorage;
        private readonly IPlainKeyValueStorage<QuestionnaireQuestionsInfo> questionnaireQuestionsInfoStorage;

        public RestoreDeletedQuestionnaireProjectionsService(IQuestionnaireStorage questionnaireStorage,
            IReferenceInfoForLinkedQuestionsFactory referenceInfoForLinkedQuestionsFactory,
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage,
            IPlainKeyValueStorage<ReferenceInfoForLinkedQuestions> referenceInfoForLinkedQuestionsStorage,
            IPlainKeyValueStorage<QuestionnaireQuestionsInfo> questionnaireQuestionsInfoStorage)
        {
            this.questionnaireStorage = questionnaireStorage;
            this.referenceInfoForLinkedQuestionsFactory = referenceInfoForLinkedQuestionsFactory;
            this.questionnaireBrowseItemStorage = questionnaireBrowseItemStorage;
            this.referenceInfoForLinkedQuestionsStorage = referenceInfoForLinkedQuestionsStorage;
            this.questionnaireQuestionsInfoStorage = questionnaireQuestionsInfoStorage;
        }

        public void RestoreAllDeletedQuestionnaireProjections()
        {
            var allDeletedQuestionnaireIds =
                this.questionnaireBrowseItemStorage.Query(
                    _ => _.Where(q => q.IsDeleted).Select(q => new {q.QuestionnaireId, q.Version}).ToList());

            foreach (var allDeletedQuestionnaireId in allDeletedQuestionnaireIds)
            {
                var document =
                    this.questionnaireStorage.GetQuestionnaireDocument(
                        allDeletedQuestionnaireId.QuestionnaireId, allDeletedQuestionnaireId.Version);
                var questionnaireEntityId = new QuestionnaireIdentity(allDeletedQuestionnaireId.QuestionnaireId, allDeletedQuestionnaireId.Version).ToString();

                this.referenceInfoForLinkedQuestionsStorage.Store(this.referenceInfoForLinkedQuestionsFactory.CreateReferenceInfoForLinkedQuestions(document, allDeletedQuestionnaireId.Version), questionnaireEntityId);
                this.questionnaireQuestionsInfoStorage.Store(new QuestionnaireQuestionsInfo
                {
                    QuestionIdToVariableMap =
                        document.Find<IQuestion>(question => true).ToDictionary(x => x.PublicKey, x => x.StataExportCaption)
                }, questionnaireEntityId);
            }
        }
    }
}