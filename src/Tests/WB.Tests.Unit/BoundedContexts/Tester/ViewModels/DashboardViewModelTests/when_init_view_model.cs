using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;

using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.BoundedContexts.Tester.Services.Infrastructure;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.BoundedContexts.Tester.Views;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Tester.ViewModels.DashboardViewModelTests
{
    internal class when_init_view_model : DashboardViewModelTestContext
    {
        Establish context = () =>
        {
            var designerApiService = Mock.Of<IDesignerApiService>(_ => 
                _.GetQuestionnairesAsync(false, Moq.It.IsAny<CancellationToken>()) == Task.FromResult(MyQuestionnaires) &&
                _.GetQuestionnairesAsync(true, Moq.It.IsAny<CancellationToken>()) == Task.FromResult(PublicQuestionnaires));

            var userName = "Vasya";

            var storageAccessor = new Mock<IAsyncPlainStorage<QuestionnaireListItem>>();
            storageAccessor.Setup(
                x => x.Query(Moq.It.IsAny<Func<IQueryable<QuestionnaireListItem>, List<QuestionnaireListItem>>>()))
                .Returns(
                    new List<QuestionnaireListItem>
                    {
                        new QuestionnaireListItem {IsPublic = false, OwnerName = userName},
                        new QuestionnaireListItem {IsPublic = false, OwnerName = userName},
                        new QuestionnaireListItem { IsPublic = true, OwnerName = userName},
                        new QuestionnaireListItem { IsPublic = true, OwnerName = userName },
                        new QuestionnaireListItem { IsPublic = true, OwnerName = userName }
                    });

            var userIdentity = Mock.Of<IUserIdentity>(_ => _.Name == userName && _.UserId == Guid.Parse("11111111111111111111111111111111"));
            var principal = Mock.Of<IPrincipal>(_ => _.CurrentUserIdentity == userIdentity);

            viewModel = CreateDashboardViewModel(
                principal: principal,
                designerApiService: designerApiService,
                questionnaireListStorage: storageAccessor.Object);
        };

        Because of = () => viewModel.Init();

        It should_set_ShowEmptyQuestionnaireListText_to_true = () => viewModel.ShowEmptyQuestionnaireListText.ShouldBeTrue();
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