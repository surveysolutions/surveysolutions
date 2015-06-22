using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.ViewModels.CascadingSingleOptionQuestionViewModelTests
{
    public class when_handling_SingleOptionQuestionAnswered_for_parent_question : CascadingSingleOptionQuestionViewModelTestContext
    {
        Establish context = () =>
        {
            var childAnswer = Mock.Of<SingleOptionAnswer>(_ => _.IsAnswered == true && _.Answer == answerOnChildQuestion);
            var parentOptionAnswer = Mock.Of<SingleOptionAnswer>(_ => _.IsAnswered == true && _.Answer == 1);
            var secondParentOptionAnswer = Mock.Of<SingleOptionAnswer>(_ => _.IsAnswered == true && _.Answer == 2);

            var userIdentity = Mock.Of<IUserIdentity>(_ => _.UserId == userId);
            var principal = Mock.Of<IPrincipal>(_ => _.CurrentUserIdentity == userIdentity);

            StatefulInterviewMock.Setup(x => x.Id).Returns(interviewGuid);
            StatefulInterviewMock.Setup(x => x.QuestionnaireId).Returns(questionnaireId);
            StatefulInterviewMock.Setup(x => x.GetSingleOptionAnswer(questionIdentity)).Returns(childAnswer);
            StatefulInterviewMock.Setup(x => x.GetSingleOptionAnswer(parentIdentity)).Returns(parentOptionAnswer);


            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(interviewGuid.FormatGuid()) == StatefulInterviewMock.Object);

            var cascadingQuestionModel = Mock.Of<CascadingSingleOptionQuestionModel>(_
                => _.Id == questionIdentity.Id
                   && _.Options == Options
                   && _.CascadeFromQuestionId == parentIdentity.Id
                   && _.RosterLevelDepthOfParentQuestion == 1);

            var questionnaireModel = Mock.Of<QuestionnaireModel>(_ => _.Questions == new Dictionary<Guid, BaseQuestionModel> { { questionIdentity.Id, cascadingQuestionModel } });

            var questionnaireRepository = Mock.Of<IPlainKeyValueStorage<QuestionnaireModel>>(x => x.GetById(questionnaireId) == questionnaireModel);

            cascadingModel = CreateCascadingSingleOptionQuestionViewModel(
                interviewRepository: interviewRepository,
                questionnaireRepository: questionnaireRepository);

            cascadingModel.Init(interviewGuid.FormatGuid(), questionIdentity, navigationState);

            StatefulInterviewMock.Setup(x => x.GetSingleOptionAnswer(parentIdentity)).Returns(secondParentOptionAnswer);
        };

        Because of = () =>
            cascadingModel.Handle(Create.Event.SingleOptionQuestionAnswered(parentIdentity.Id, parentIdentity.RosterVector, 2));

        It should_set_ShouldClearText_in_null = () =>
            cascadingModel.ResetTextInEditor.ShouldBeNull();

        private static CascadingSingleOptionQuestionViewModel cascadingModel;

        private static readonly Mock<IStatefulInterview> StatefulInterviewMock = new Mock<IStatefulInterview>();

        private static readonly int answerOnChildQuestion = 3;
    }
}