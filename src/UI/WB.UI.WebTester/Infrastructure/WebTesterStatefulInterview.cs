using System.Collections.Generic;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.UI.WebTester.Services;

namespace WB.UI.WebTester.Infrastructure
{
    public class WebTesterStatefulInterview : StatefulInterview
    {
        private readonly IAppdomainsPerInterviewManager appdomainsPerInterviewManager;

        public WebTesterStatefulInterview(
            ISubstitutionTextFactory substitutionTextFactory,
            IAppdomainsPerInterviewManager appdomainsPerInterviewManager,
            IInterviewTreeBuilder treeBuilder,
            IServiceLocator serviceLocator
            ) 
            : base(
                substitutionTextFactory, 
                treeBuilder
                )
        {
            this.appdomainsPerInterviewManager = appdomainsPerInterviewManager;
            base.ServiceLocatorInstance = serviceLocator;
        }

        public override List<CategoricalOption> GetFirstTopFilteredOptionsForQuestion(Identity questionIdentity, int? parentQuestionValue, string filter, int itemsCount = 200)
        {
            return this.appdomainsPerInterviewManager.GetFirstTopFilteredOptionsForQuestion(this.Id, questionIdentity, parentQuestionValue, filter, itemsCount);
        }
    }
}
