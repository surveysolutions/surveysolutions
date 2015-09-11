using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Machine.Specifications;

using Moq;

using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.BoundedContexts.Tester.Views;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Tester.ViewModels.DashboardViewModelTests
{
    internal class when_refresh_questionnaire_list : DashboardViewModelTestContext
    {
        Establish context = () =>
        {
            var designerApiService = Mock.Of<IDesignerApiService>(_ => _.GetQuestionnairesAsync(false, Moq.It.IsAny<CancellationToken>()) == Task.FromResult(MyQuestionnaires) &&
                _.GetQuestionnairesAsync(true, Moq.It.IsAny<CancellationToken>()) == Task.FromResult(PublicQuestionnaires));

            var storageAccessor = new Mock<IAsyncPlainStorage<QuestionnaireListItem>>();
            storageAccessor.Setup(
                x => x.Query(Moq.It.IsAny<Func<IQueryable<QuestionnaireListItem>, List<QuestionnaireListItem>>>()))
                .Returns(MyQuestionnaires.ToList());

            viewModel = CreateDashboardViewModel(questionnaireListStorage: storageAccessor.Object,
                designerApiService: designerApiService);

            viewModel.Init();
            viewModel.ShowPublicQuestionnairesCommand.Execute();
        };

        Because of = () => viewModel.RefreshQuestionnairesCommand.Execute();

        It should_stay_on_same_tab_with_public_questionnaires = () => viewModel.IsPublicShowed.ShouldBeTrue();
        It should_Questionnaires_have_3_public_questionnaires = () => viewModel.Questionnaires.Count.ShouldEqual(3);
        It should_contains_only_public_questionnaires = () => viewModel.Questionnaires.All(_ => _.IsPublic).ShouldBeTrue();
        It should_set_MyQuestionnairesCount_to_2 = () => viewModel.MyQuestionnairesCount.ShouldEqual(2);
        It should_set_PublicQuestionnairesCount_to_3 = () => viewModel.PublicQuestionnairesCount.ShouldEqual(3);

        private static DashboardViewModel viewModel;
        private static readonly IList<QuestionnaireListItem> MyQuestionnaires = new List<QuestionnaireListItem>
        {
            new QuestionnaireListItem(){IsPublic = false, OwnerName = userName},
            new QuestionnaireListItem(){IsPublic = false, OwnerName = userName}
        };

        private static readonly IList<QuestionnaireListItem> PublicQuestionnaires = new List<QuestionnaireListItem>
        {
            new QuestionnaireListItem(){IsPublic = true},
            new QuestionnaireListItem(){IsPublic = true},
            new QuestionnaireListItem(){IsPublic = true}
        };
    }
}