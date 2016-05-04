using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Supervisor.Factories;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.EventHandler.WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.UI.Headquarters.Services;

namespace WB.UI.Headquarters.Implementation.Services
{
    internal class RestoreDeletedQuestionnaireProjectionsService : IRestoreDeletedQuestionnaireProjectionsService
    {
        private readonly IPlainQuestionnaireRepository plainQuestionnaireRepository;

        private readonly IReferenceInfoForLinkedQuestionsFactory referenceInfoForLinkedQuestionsFactory;
        private readonly IQuestionnaireRosterStructureFactory questionnaireRosterStructureFactory;
        private readonly IExportViewFactory exportViewFactory;

        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage;

        private readonly IPlainKeyValueStorage<ReferenceInfoForLinkedQuestions> referenceInfoForLinkedQuestionsStorage;
        private readonly IPlainKeyValueStorage<QuestionnaireRosterStructure> questionnaireRosterStructureStorage;
        private readonly IPlainKeyValueStorage<QuestionnaireExportStructure> questionnaireExportStructureStorage;
        private readonly IPlainKeyValueStorage<QuestionnaireQuestionsInfo> questionnaireQuestionsInfoStorage;

        public RestoreDeletedQuestionnaireProjectionsService(IPlainQuestionnaireRepository plainQuestionnaireRepository,IReferenceInfoForLinkedQuestionsFactory referenceInfoForLinkedQuestionsFactory, IQuestionnaireRosterStructureFactory questionnaireRosterStructureFactory, IExportViewFactory exportViewFactory, IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage, IPlainKeyValueStorage<ReferenceInfoForLinkedQuestions> referenceInfoForLinkedQuestionsStorage, IPlainKeyValueStorage<QuestionnaireRosterStructure> questionnaireRosterStructureStorage, IPlainKeyValueStorage<QuestionnaireExportStructure> questionnaireExportStructureStorage, IPlainKeyValueStorage<QuestionnaireQuestionsInfo> questionnaireQuestionsInfoStorage)
        {
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
            this.referenceInfoForLinkedQuestionsFactory = referenceInfoForLinkedQuestionsFactory;
            this.questionnaireRosterStructureFactory = questionnaireRosterStructureFactory;
            this.exportViewFactory = exportViewFactory;
            this.questionnaireBrowseItemStorage = questionnaireBrowseItemStorage;
            this.referenceInfoForLinkedQuestionsStorage = referenceInfoForLinkedQuestionsStorage;
            this.questionnaireRosterStructureStorage = questionnaireRosterStructureStorage;
            this.questionnaireExportStructureStorage = questionnaireExportStructureStorage;
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
                    this.plainQuestionnaireRepository.GetQuestionnaireDocument(
                        allDeletedQuestionnaireId.QuestionnaireId, allDeletedQuestionnaireId.Version);
                var questionnaireEntityId = new QuestionnaireIdentity(allDeletedQuestionnaireId.QuestionnaireId, allDeletedQuestionnaireId.Version).ToString();

                this.referenceInfoForLinkedQuestionsStorage.Store(this.referenceInfoForLinkedQuestionsFactory.CreateReferenceInfoForLinkedQuestions(document, allDeletedQuestionnaireId.Version), questionnaireEntityId);
                this.questionnaireExportStructureStorage.Store(this.exportViewFactory.CreateQuestionnaireExportStructure(document, allDeletedQuestionnaireId.Version), questionnaireEntityId);
                this.questionnaireRosterStructureStorage.Store(this.questionnaireRosterStructureFactory.CreateQuestionnaireRosterStructure(document, allDeletedQuestionnaireId.Version), questionnaireEntityId);
                this.questionnaireQuestionsInfoStorage.Store(new QuestionnaireQuestionsInfo
                {
                    QuestionIdToVariableMap =
                        document.Find<IQuestion>(question => true).ToDictionary(x => x.PublicKey, x => x.StataExportCaption)
                }, questionnaireEntityId);
            }
        }
    }
}