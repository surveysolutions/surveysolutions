using System;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.EventHandlers.Interview.AnswersByVariableDenormalizerTests
{
    internal class when_answering_geo_question_and_where_is_no_question_id_to_variable_map : AnswersByVariableDenormalizerTestContext
    {
        Establish context = () =>
        {
            answersByVariableStorageMock = new Mock<IReadSideRepositoryWriter<AnswersByVariableCollection>>();

            var interviewBriefMock = Mock.Of<InterviewBrief>(i => i.QuestionnaireId == questionnaireId && i.QuestionnaireVersion == 1);

            interviewBriefStorage = Mock.Of<IReadSideRepositoryWriter<InterviewBrief>>(x => x.GetById(interviewId.ToString()) == interviewBriefMock);

            variablesStorage = Mock.Of<IReadSideRepositoryWriter<QuestionnaireQuestionsInfo>>();
            denormalizer = CreateAnswersByVariableDenormalizer(interviewBriefStorage, variablesStorage, answersByVariableStorageMock.Object);
            evnt = CreateGeoLocationQuestionAnsweredEvent(interviewId);
        };

        Because of = () => denormalizer.Handle(evnt);

        It should_not_store_any_view = () =>
            answersByVariableStorageMock.Verify(x => x.Store(Moq.It.IsAny<AnswersByVariableCollection>(), Moq.It.IsAny<string>()), Times.Never());

        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static Guid interviewId = Guid.Parse("22222222222222222222222222222222");
        private static Mock<IReadSideRepositoryWriter<AnswersByVariableCollection>> answersByVariableStorageMock;
        private static IReadSideRepositoryWriter<InterviewBrief> interviewBriefStorage;
        private static IReadSideRepositoryWriter<QuestionnaireQuestionsInfo> variablesStorage;
        private static AnswersByVariableDenormalizer denormalizer;
        private static IPublishedEvent<GeoLocationQuestionAnswered> evnt;
    }
}