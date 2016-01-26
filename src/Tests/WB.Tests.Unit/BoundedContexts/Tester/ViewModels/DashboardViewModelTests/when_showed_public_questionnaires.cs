using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

using Machine.Specifications;

using Moq;

using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.BoundedContexts.Tester.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Tester.ViewModels.DashboardViewModelTests
{
    internal class when_showed_public_questionnaires : DashboardViewModelTestContext
    {
        Establish context = () =>
        {
            var designerApiService = Mock.Of<IDesignerApiService>(_ => _.GetQuestionnairesAsync(false, Moq.It.IsAny<CancellationToken>()) == Task.FromResult(MyQuestionnaires) &&
                _.GetQuestionnairesAsync(true, Moq.It.IsAny<CancellationToken>()) == Task.FromResult(PublicQuestionnaires));

            var storageAccessorMock = new Mock<IAsyncPlainStorage<QuestionnaireListItem>>();
            storageAccessorMock
                .Setup(x => x.Where(Moq.It.IsAny<Expression<Func<QuestionnaireListItem, bool>>>()))
                .Returns(Enumerable.Empty<QuestionnaireListItem>().ToReadOnlyCollection());

            viewModel = CreateDashboardViewModel(questionnaireListStorage: storageAccessorMock.Object,
                designerApiService: designerApiService);
            viewModel.Init();
        };

        Because of = () => viewModel.ShowPublicQuestionnairesCommand.Execute();

        It should_set_IsPublicShowed_to_true = () => viewModel.IsPublicShowed.ShouldBeTrue();
        It should_Questionnaires_have_3_questionnaires = () => viewModel.Questionnaires.Count.ShouldEqual(3);
        It should_contains_only_public_questionnaires = () => viewModel.Questionnaires.All(_ => _.IsPublic).ShouldBeTrue();

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