using System;
using Ncqrs;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.UI.WebTester.Services;

namespace WB.UI.WebTester.Infrastructure
{
    public class WebTesterStatefulInterview : StatefulInterview
    {
        private readonly IAppdomainsPerInterviewManager appdomainsPerInterviewManager;
        private readonly IQuestionnaireStorage questionnaireStorage;

        public WebTesterStatefulInterview(
            ISubstitutionTextFactory substitutionTextFactory,
            IAppdomainsPerInterviewManager appdomainsPerInterviewManager,
            IInterviewTreeBuilder treeBuilder,
            IQuestionOptionsRepository optionsRepository,
            IQuestionnaireStorage questionnaireStorage,
            IClock clock
            ) 
            : base(
                substitutionTextFactory, 
                treeBuilder,
                optionsRepository,
                clock
                )
        {
            this.appdomainsPerInterviewManager = appdomainsPerInterviewManager;
            this.questionnaireStorage = questionnaireStorage;
        }

        //public override List<CategoricalOption> GetFirstTopFilteredOptionsForQuestion(Identity questionIdentity,
        //    int? parentQuestionValue, string filter, int itemsCount = 200, int[] excludedOptionIds = null)
        //{
        //    return this.appdomainsPerInterviewManager.GetFirstTopFilteredOptionsForQuestion(this.Id, questionIdentity, parentQuestionValue, filter, itemsCount, excludedOptionIds);
        //}

        protected override IQuestionnaire GetQuestionnaireOrThrow(string? language)
        {
            var questionnaire = this.questionnaireStorage.GetQuestionnaire(this.QuestionnaireIdentity, language);
            if(questionnaire == null)
                throw new InvalidOperationException("Questionnaire must not be null.");

            return questionnaire;
        }

        public IQuestionnaire Questionnaire => GetQuestionnaireOrThrow(null);
    }
}
