using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.AnswersByVariableDenormalizerTests
{
    internal class when_removing_answer_and_where_is_no_info_about_income_question : AnswersByVariableDenormalizerTestContext
    {
        Establish context = () =>
        {
            answersByVariableStorageMock = new Mock<IReadSideKeyValueStorage<AnswersByVariableCollection>>();

            answersByVariableStorageMock
                .Setup(x => x.GetById(answersCollectionViewId))
                .Returns(CreateAnswersByVariableCollectionWithOneAnswer(interviewId, "0.5", "22;22"));

            var interviewBriefMock = Mock.Of<InterviewSummary>(i => i.QuestionnaireId == questionnaireId && i.QuestionnaireVersion == 1);

            interviewBriefStorage = Mock.Of<IReadSideRepositoryReader<InterviewSummary>>(x => x.GetById(interviewId.ToString()) == interviewBriefMock);

            var questionIdToVariableMap = new Dictionary<Guid, string>() { { questionId, variableName } };

            var questionsInfoMock = Mock.Of<QuestionnaireQuestionsInfo>(x => x.QuestionIdToVariableMap == questionIdToVariableMap);

            variablesStorage = Mock.Of<IReadSideKeyValueStorage<QuestionnaireQuestionsInfo>>(x => x.GetById("11111111-1111-1111-1111-111111111111-1") == questionsInfoMock);

            denormalizer = CreateAnswersByVariableDenormalizer(interviewBriefStorage, variablesStorage, answersByVariableStorageMock.Object);

            evnt = CreateAnswerRemovedEvent(interviewId, questionId: questionId, propagationVector: new[] { 1.5m });
        };

        Because of = () => denormalizer.Handle(evnt);

        It should_not_store_any_view = () =>
            answersByVariableStorageMock.Verify(x => x.Store(Moq.It.IsAny<AnswersByVariableCollection>(), Moq.It.IsAny<string>()), Times.Never());

        private static string answersCollectionViewId = "var-11111111-1111-1111-1111-111111111111-1";
        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static Guid interviewId = Guid.Parse("22222222222222222222222222222222");
        private static Guid questionId = Guid.Parse("33333333333333333333333333333333");
        private static readonly string variableName = "var";
        private static Mock<IReadSideKeyValueStorage<AnswersByVariableCollection>> answersByVariableStorageMock;
        private static IReadSideRepositoryReader<InterviewSummary> interviewBriefStorage;
        private static IReadSideKeyValueStorage<QuestionnaireQuestionsInfo> variablesStorage;
        private static AnswersByVariableDenormalizer denormalizer;
        private static IPublishedEvent<AnswersRemoved> evnt;
    }
}