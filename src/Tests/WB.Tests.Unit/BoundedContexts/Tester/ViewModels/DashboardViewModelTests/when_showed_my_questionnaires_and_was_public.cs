using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Machine.Specifications;

using Moq;

using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.BoundedContexts.Tester.Infrastructure;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.BoundedContexts.Tester.Views;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Tester.ViewModels.DashboardViewModelTests
{
    public class when_showed_my_questionnaires_and_was_public : DashboardViewModelTestContext
    {
        Establish context = () =>
        {
            var designerApiService = Mock.Of<IDesignerApiService>(_ => _.GetQuestionnairesAsync(false, Moq.It.IsAny<CancellationToken>()) == Task.FromResult(MyQuestionnaires) &&
                _.GetQuestionnairesAsync(true, Moq.It.IsAny<CancellationToken>()) == Task.FromResult(PublicQuestionnaires));

            var storageAccessor = new Mock<IPlainStorageAccessor<QuestionnaireListItem>>();
            storageAccessor.Setup(
                x => x.Query(Moq.It.IsAny<Func<IQueryable<QuestionnaireListItem>, List<QuestionnaireListItem>>>()))
                .Returns(new List<QuestionnaireListItem>());

            viewModel = CreateDashboardViewModel(
                designerApiService: designerApiService,
                questionnaireListStorageAccessor: storageAccessor.Object);

            viewModel.Init();
            viewModel.ShowPublicQuestionnairesCommand.Execute();
        };

        Because of = () => viewModel.ShowMyQuestionnairesCommand.Execute();

        It should_set_IsPublicShowed_to_false = () => viewModel.IsPublicShowed.ShouldBeFalse();
        It should_Questionnaires_have_2_questionnaires = () => viewModel.Questionnaires.Count.ShouldEqual(2);
        It should_contains_only_my_questionnaires = () => viewModel.Questionnaires.All(_ => !_.IsPublic).ShouldBeTrue();

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