using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.BoundedContexts.Tester.Views;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.ViewModels.DashboardViewModelTests
{
    public class when_init_view_model : DashboardViewModelTestContext
    {
        Establish context = () =>
        {
            var designerApiService = Mock.Of<IDesignerApiService>(_ => _.GetQuestionnairesAsync(false, Moq.It.IsAny<CancellationToken>()) == Task.FromResult(MyQuestionnaires) &&
                _.GetQuestionnairesAsync(true, Moq.It.IsAny<CancellationToken>()) == Task.FromResult(PublicQuestionnaires));
            
            viewModel = CreateDashboardViewModel(
                designerApiService: designerApiService);
        };

        Because of = () => viewModel.Init();

        It should_set_IsInitialized_to_true = () => viewModel.IsInitialized.ShouldBeTrue();
        It should_set_IsPublicShowed_to_false = () => viewModel.IsPublicShowed.ShouldBeFalse();
        It should_Questionnaires_have_2_questionnaires = () => viewModel.Questionnaires.Count.ShouldEqual(2);
        It should_contains_only_my_questionnares = () => viewModel.Questionnaires.All(_ => !_.IsPublic).ShouldBeTrue();
        It should_set_MyQuestionnairesCount_to_2 = () => viewModel.MyQuestionnairesCount.ShouldEqual(2);
        It should_set_PublicQuestionnairesCount_to_3 = () => viewModel.PublicQuestionnairesCount.ShouldEqual(3);

        private static DashboardViewModel viewModel;

        private static readonly IList<QuestionnaireListItem> MyQuestionnaires = new List<QuestionnaireListItem>
        {
            new QuestionnaireListItem(){IsPublic = false},
            new QuestionnaireListItem(){IsPublic = false}
        };

        private static readonly IList<QuestionnaireListItem> PublicQuestionnaires = new List<QuestionnaireListItem>
        {
            new QuestionnaireListItem(){IsPublic = true},
            new QuestionnaireListItem(){IsPublic = true},
            new QuestionnaireListItem(){IsPublic = true}
        };
    }
}