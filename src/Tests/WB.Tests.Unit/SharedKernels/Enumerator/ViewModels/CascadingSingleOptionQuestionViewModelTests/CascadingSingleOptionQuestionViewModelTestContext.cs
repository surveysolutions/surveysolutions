using System;
using System.Collections.Generic;
using MvvmCross.Test.Core;
using Moq;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire.Questions;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.CascadingSingleOptionQuestionViewModelTests
{
    internal class CascadingSingleOptionQuestionViewModelTestContext : MvxIoCSupportingTest
    {
        public CascadingSingleOptionQuestionViewModelTestContext()
        {
            base.Setup();
        }

        protected static CascadingSingleOptionQuestionViewModel CreateCascadingSingleOptionQuestionViewModel(
            IPlainQuestionnaireRepository questionnaireRepository = null,
            IStatefulInterviewRepository interviewRepository = null)
        {
            var userIdentity = Mock.Of<IUserIdentity>(_ => _.UserId == userId);
            var principal = Mock.Of<IPrincipal>(_ => _.CurrentUserIdentity == userIdentity);

            return new CascadingSingleOptionQuestionViewModel(
                principal, 
                questionnaireRepository ?? Mock.Of<IPlainQuestionnaireRepository>(), 
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                QuestionStateMock.Object,
                AnsweringViewModelMock.Object,
                EventRegistry.Object);
        }

        protected static IPlainQuestionnaireRepository SetupQuestionnaireRepositoryWithCascadingQuestion()
        {
            var questionnaire = Mock.Of<IQuestionnaire>(_
                => _.GetRosterLevelForEntity(parentIdentity.Id) == 1
                && _.GetCascadingQuestionParentId(questionIdentity.Id) == parentIdentity.Id
                && _.GetAnswerOptionsAsValues(questionIdentity.Id) == new decimal[] { 1, 2, 3, 4, 5, 6 }
                && _.GetAnswerOptionTitle(questionIdentity.Id, 1) == "title abc 1"
                && _.GetAnswerOptionTitle(questionIdentity.Id, 2) == "title def 2"
                && _.GetAnswerOptionTitle(questionIdentity.Id, 3) == "title klo 3"
                && _.GetAnswerOptionTitle(questionIdentity.Id, 4) == "title gha 4"
                && _.GetAnswerOptionTitle(questionIdentity.Id, 5) == "title ccc 5"
                && _.GetAnswerOptionTitle(questionIdentity.Id, 6) == "title bcw 6"
                && _.GetCascadingParentValue(questionIdentity.Id, 1) == 1
                && _.GetCascadingParentValue(questionIdentity.Id, 2) == 1
                && _.GetCascadingParentValue(questionIdentity.Id, 3) == 1
                && _.GetCascadingParentValue(questionIdentity.Id, 4) == 2
                && _.GetCascadingParentValue(questionIdentity.Id, 5) == 2
                && _.GetCascadingParentValue(questionIdentity.Id, 6) == 2
            );
            return Mock.Of<IPlainQuestionnaireRepository>(x => x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>()) == questionnaire);
        }

        protected static void SetUp()
        {
            
            navigationState = Create.NavigationState();
            QuestionStateMock = new Mock<QuestionStateViewModel<SingleOptionQuestionAnswered>> { DefaultValue = DefaultValue.Mock };
            AnsweringViewModelMock = new Mock<AnsweringViewModel> { DefaultValue = DefaultValue.Mock };
            EventRegistry = new Mock<ILiteEventRegistry>();
        }

        protected static Identity questionIdentity = Create.Identity(Guid.Parse("11111111111111111111111111111111"), new decimal[] { 1, 2 });

        protected static Identity parentIdentity = Create.Identity(Guid.Parse("22222222222222222222222222222222"), new decimal[] { 1 });

        protected static NavigationState navigationState;

        protected static Mock<QuestionStateViewModel<SingleOptionQuestionAnswered>> QuestionStateMock;

        protected static Mock<AnsweringViewModel> AnsweringViewModelMock;

        protected static Mock<ILiteEventRegistry> EventRegistry;

        protected static List<CascadingOptionModel> Options = new List<CascadingOptionModel>
        {
            Create.CascadingOptionModel(1, "title abc 1", 1),
            Create.CascadingOptionModel(2, "title def 2", 1),
            Create.CascadingOptionModel(3, "title klo 3", 1),
            Create.CascadingOptionModel(4, "title gha 4", 2),
            Create.CascadingOptionModel(5, "title ccc 5", 2),
            Create.CascadingOptionModel(6, "title bcw 6", 2)
        };

        protected static readonly string interviewId = "Some interviewId";

        protected static readonly string questionnaireId = "Questionnaire Id";

        protected static Guid interviewGuid = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");

        protected static readonly Guid userId = Guid.Parse("ffffffffffffffffffffffffffffffff");
    }
}
