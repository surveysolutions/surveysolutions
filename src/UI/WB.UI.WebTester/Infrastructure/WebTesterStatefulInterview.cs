using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection;
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

        public WebTesterStatefulInterview(
            IQuestionnaireStorage questionnaireRepository, 
            IInterviewExpressionStatePrototypeProvider expressionProcessorStatePrototypeProvider, 
            ISubstitutionTextFactory substitutionTextFactory,
            IAppdomainsPerInterviewManager appdomainsPerInterviewManager,
            IInterviewTreeBuilder treeBuilder) 
            : base(questionnaireRepository, expressionProcessorStatePrototypeProvider, substitutionTextFactory, treeBuilder)
        {
            this.appdomainsPerInterviewManager = appdomainsPerInterviewManager;
        }

        public override List<CategoricalOption> FilteredCategoricalOptions(Identity questionIdentity, int itemsCount, IEnumerable<CategoricalOption> unfilteredOptionsForQuestion)
        {
            return this.appdomainsPerInterviewManager.FilteredCategoricalOptions(EventSourceId,
                new CategoricalOptionsFilter(questionIdentity, itemsCount, unfilteredOptionsForQuestion.ToArray()));
        }
    }
}