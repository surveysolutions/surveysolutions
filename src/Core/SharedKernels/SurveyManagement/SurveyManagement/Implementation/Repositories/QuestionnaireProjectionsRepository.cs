using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Supervisor.Factories;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.EventHandler.WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Repositories
{
    internal class QuestionnaireProjectionsRepository: IQuestionnaireProjectionsRepository
    {
        private readonly IPlainQuestionnaireRepository plainQuestionnaireRepository;

        private readonly IReferenceInfoForLinkedQuestionsFactory referenceInfoForLinkedQuestionsFactory;
        private readonly IQuestionnaireRosterStructureFactory questionnaireRosterStructureFactory;
        private readonly IExportViewFactory exportViewFactory;

        private readonly Dictionary<QuestionnaireIdentity, ReferenceInfoForLinkedQuestions> referenceInfoForLinkedQuestionsCache = new Dictionary<QuestionnaireIdentity, ReferenceInfoForLinkedQuestions>();
        private readonly Dictionary<QuestionnaireIdentity, QuestionnaireRosterStructure> questionnaireRosterStructureCache = new Dictionary<QuestionnaireIdentity, QuestionnaireRosterStructure>();
        private readonly Dictionary<QuestionnaireIdentity, QuestionnaireExportStructure> questionnaireExportStructureCache = new Dictionary<QuestionnaireIdentity, QuestionnaireExportStructure>();
        private readonly Dictionary<QuestionnaireIdentity, QuestionnaireQuestionsInfo> questionnaireQuestionsInfoCache = new Dictionary<QuestionnaireIdentity, QuestionnaireQuestionsInfo>();

        public QuestionnaireProjectionsRepository(
            IPlainQuestionnaireRepository plainQuestionnaireRepository, 
            IReferenceInfoForLinkedQuestionsFactory referenceInfoForLinkedQuestionsFactory, 
            IQuestionnaireRosterStructureFactory questionnaireRosterStructureFactory, 
            IExportViewFactory exportViewFactory)
        {
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
            this.referenceInfoForLinkedQuestionsFactory = referenceInfoForLinkedQuestionsFactory;
            this.questionnaireRosterStructureFactory = questionnaireRosterStructureFactory;
            this.exportViewFactory = exportViewFactory;
        }

        public ReferenceInfoForLinkedQuestions GetReferenceInfoForLinkedQuestions(QuestionnaireIdentity identity)
        {
            if (!this.referenceInfoForLinkedQuestionsCache.ContainsKey(identity))
            {
                var questionnaire = GetQuestionnaireDocument(identity);
                if (questionnaire == null)
                    return null;

                this.referenceInfoForLinkedQuestionsCache[identity] = this.referenceInfoForLinkedQuestionsFactory.CreateReferenceInfoForLinkedQuestions(questionnaire, identity.Version);
            }
            return this.referenceInfoForLinkedQuestionsCache[identity];
        }

        public QuestionnaireRosterStructure GetQuestionnaireRosterStructure(QuestionnaireIdentity identity)
        {
            if (!this.questionnaireRosterStructureCache.ContainsKey(identity))
            {
                var questionnaire = GetQuestionnaireDocument(identity);
                if (questionnaire == null)
                    return null;

                this.questionnaireRosterStructureCache[identity] =this.questionnaireRosterStructureFactory.CreateQuestionnaireRosterStructure(questionnaire,
                    identity.Version);
            }
            return this.questionnaireRosterStructureCache[identity];
        }

        public QuestionnaireExportStructure GetQuestionnaireExportStructure(QuestionnaireIdentity identity)
        {
            if (!this.questionnaireExportStructureCache.ContainsKey(identity))
            {
                var questionnaire = GetQuestionnaireDocument(identity);
                if (questionnaire == null)
                    return null;

                this.questionnaireExportStructureCache[identity] = this.exportViewFactory.CreateQuestionnaireExportStructure(questionnaire, identity.Version);
            }
            return this.questionnaireExportStructureCache[identity];
        }

        public QuestionnaireQuestionsInfo GetQuestionnaireQuestionsInfo(QuestionnaireIdentity identity)
        {
            if (!this.questionnaireQuestionsInfoCache.ContainsKey(identity))
            {
                var questionnaire = GetQuestionnaireDocument(identity);
                if (questionnaire == null)
                    return null;

                this.questionnaireQuestionsInfoCache[identity] = new QuestionnaireQuestionsInfo
                {
                    QuestionIdToVariableMap =
                   questionnaire.Find<IQuestion>(question => true).ToDictionary(x => x.PublicKey, x => x.StataExportCaption)
                };
            }
            return this.questionnaireQuestionsInfoCache[identity];
        }

        private QuestionnaireDocument GetQuestionnaireDocument(QuestionnaireIdentity identity)
        {
            var questionnaire = this.plainQuestionnaireRepository.GetQuestionnaireDocument(identity);
            if (questionnaire == null)
                return null;

            questionnaire.ConnectChildrenWithParent();
            return questionnaire;
        }
    }
}