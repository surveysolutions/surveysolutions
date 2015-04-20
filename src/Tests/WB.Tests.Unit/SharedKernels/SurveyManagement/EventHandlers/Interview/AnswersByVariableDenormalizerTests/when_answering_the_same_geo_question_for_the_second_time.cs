using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.AnswersByVariableDenormalizerTests
{
    internal class when_answering_the_same_geo_question_for_the_second_time : AnswersByVariableDenormalizerTestContext
    {
        Establish context = () =>
        {
            answersByVariableStorageMock = new Mock<IReadSideKeyValueStorage<AnswersByVariableCollection>>();

            answersByVariableStorageMock
                .Setup(x => x.GetById(answersCollectionViewId))
                .Returns(CreateAnswersByVariableCollectionWithOneAnswer(interviewId));

            answersByVariableStorageMock
                .Setup(x => x.Store(Moq.It.IsAny<AnswersByVariableCollection>(), Moq.It.IsAny<string>()))
                .Callback((AnswersByVariableCollection collection, string id) => answersCollection = collection);

            var interviewBriefMock = Mock.Of<InterviewSummary>(i => i.QuestionnaireId == questionnaireId && i.QuestionnaireVersion == 1);

            interviewBriefStorage = Mock.Of<IReadSideRepositoryReader<InterviewSummary>>(x => x.GetById(interviewId.FormatGuid()) == interviewBriefMock);

            var questionIdToVariableMap = new Dictionary<Guid, string>() { { questionId, variableName } };

            var questionsInfoMock = Mock.Of<QuestionnaireQuestionsInfo>(x => x.QuestionIdToVariableMap == questionIdToVariableMap);

            variablesStorage = Mock.Of<IReadSideKeyValueStorage<QuestionnaireQuestionsInfo>>(x => x.GetById("11111111111111111111111111111111-1") == questionsInfoMock);
            denormalizer = CreateAnswersByVariableDenormalizer(interviewBriefStorage, variablesStorage, answersByVariableStorageMock.Object);
            evnt = CreateGeoLocationQuestionAnsweredEvent(interviewId, questionId: questionId, latitude: latitude, longitude: longitude);
        };

        Because of = () => denormalizer.Handle(evnt);

        It should_query_previous_answers_collection = () =>
           answersByVariableStorageMock.Verify(x => x.GetById(answersCollectionViewId), Times.Once());

        It should_store_one_not_null_view = () =>
            answersCollection.ShouldNotBeNull();

        It should_have_one_row_in_Answers_collection = () =>
            answersCollection.Answers.Count.ShouldEqual(1);

        It should_set_interview_id_as_key_in_Answers_collection = () =>
            answersCollection.Answers.Keys.ShouldContainOnly(new[] { interviewId });

        It should_set_dictionary_with_only_one_row_in_Answers_collection_with_interviewId_key = () =>
            answersCollection.Answers[interviewId].Count.ShouldEqual(1);

        It should_set_dictionary_with_key_equals__sharp__in_Answers_collection_with_interviewId_key = () =>
            answersCollection.Answers[interviewId].Keys.ShouldContainOnly(new[] { "#" });

        It should_set_dictionary_with_value_equals__lat_lon__in_Answers_collection_with_interviewId_key = () =>
            answersCollection.Answers[interviewId].Values.ShouldContainOnly(new[] { "11.154;50.01" });


        private static string answersCollectionViewId = "var-11111111111111111111111111111111-1";
        private static AnswersByVariableCollection answersCollection;
        private static readonly string variableName = "var";
        private static Guid questionnaireId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static Guid interviewId = Guid.Parse("22222222222222222222222222222222");
        private static Guid questionId = Guid.Parse("33333333333333333333333333333333");

        private static double latitude = 11.154d;
        private static double longitude = 50.01d;
        private static Mock<IReadSideKeyValueStorage<AnswersByVariableCollection>> answersByVariableStorageMock;
        private static IReadSideRepositoryReader<InterviewSummary> interviewBriefStorage;
        private static IReadSideKeyValueStorage<QuestionnaireQuestionsInfo> variablesStorage;
        private static AnswersByVariableDenormalizer denormalizer;
        private static IPublishedEvent<GeoLocationQuestionAnswered> evnt;
        
    }
}