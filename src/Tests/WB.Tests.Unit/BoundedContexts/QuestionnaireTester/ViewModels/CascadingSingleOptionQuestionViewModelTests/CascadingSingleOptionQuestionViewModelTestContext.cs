using System;
using System.Collections.Generic;
using Moq;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionStateViewModels;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

using it = Moq.It;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.ViewModels.CascadingSingleOptionQuestionViewModelTests
{
    public class CascadingSingleOptionQuestionViewModelTestContext 
    {
        protected static CascadingSingleOptionQuestionViewModel CreateCascadingSingleOptionQuestionViewModel(
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository = null,
            IStatefulInterviewRepository interviewRepository = null)
        {
            var userIdentity = Mock.Of<IUserIdentity>(_ => _.UserId == userId);
            var principal = Mock.Of<IPrincipal>(_ => _.CurrentUserIdentity == userIdentity);

            return new CascadingSingleOptionQuestionViewModel(
                principal ?? Mock.Of<IPrincipal>(), 
                questionnaireRepository ?? Mock.Of<IPlainKeyValueStorage<QuestionnaireModel>>(), 
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                QuestionStateMock.Object,
                AnsweringViewModelMock.Object,
                EventRegistry.Object);
        }

        protected static Identity questionIdentity = Create.Identity(Guid.Parse("11111111111111111111111111111111"), new decimal[] { 1, 2 });
        protected static Identity parentIdentity = Create.Identity(Guid.Parse("22222222222222222222222222222222"), new decimal[] { 1 });
        protected static NavigationState navigationState = Create.NavigationState();

        protected static readonly Mock<QuestionStateViewModel<SingleOptionQuestionAnswered>> QuestionStateMock =
            new Mock<QuestionStateViewModel<SingleOptionQuestionAnswered>> { DefaultValue = DefaultValue.Mock };

        protected static readonly Mock<AnsweringViewModel> AnsweringViewModelMock =
            new Mock<AnsweringViewModel> { DefaultValue = DefaultValue.Mock };

        protected static readonly Mock<ILiteEventRegistry> EventRegistry = new Mock<ILiteEventRegistry>();

        protected static List<CascadingOptionModel> Options = new List<CascadingOptionModel>
        {
            Create.CascadingOptionModel(1, "title abc 1", 1),
            Create.CascadingOptionModel(2, "title def 2", 1),
            Create.CascadingOptionModel(3, "title klo 3", 1),
            Create.CascadingOptionModel(4, "title gha 4", 2),
            Create.CascadingOptionModel(5, "title ccc 5", 2),
            Create.CascadingOptionModel(6, "title ebw 6", 2)
        };

        protected static readonly string interviewId = "Some interviewId";
        protected static readonly Guid interviewGuid = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        protected static readonly string questionnaireId = "Questionnaire Id";
        protected static readonly Guid userId = Guid.Parse("ffffffffffffffffffffffffffffffff");
    }
}
