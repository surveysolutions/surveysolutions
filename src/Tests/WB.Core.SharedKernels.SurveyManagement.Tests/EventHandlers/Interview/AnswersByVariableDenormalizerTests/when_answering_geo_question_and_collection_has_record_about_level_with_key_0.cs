using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

namespace WB.Core.SharedKernels.SurveyManagement.Tests.EventHandlers.AnswersByVariableDenormalizerTests
{
    internal class when_answering_geo_question_and_collection_has_record_about_level_with_key_0 : AnswersByVariableDenormalizerTestContext
    {
        Establish context = () =>
        {
            answersByVariableStorageMock = new Mock<IReadSideRepositoryWriter<AnswersByVariableCollection>>();
            answersByVariableStorageMock
                .Setup(x => x.Store(Moq.It.IsAny<AnswersByVariableCollection>(), Moq.It.IsAny<string>()))
                .Callback((AnswersByVariableCollection collection, string id) => answersCollection = collection);

            var answersByVariableCollection = new AnswersByVariableCollection();
            answersByVariableCollection.Answers.Add(interviewId, new Dictionary<string, string>());
            answersByVariableCollection.Answers[interviewId]["0"] = "some coordinate";

            answersByVariableStorageMock.Setup(x => x.GetById(Moq.It.IsAny<string>())).Returns(answersByVariableCollection);

            var interviewBriefMock = Mock.Of<InterviewBrief>(i => i.QuestionnaireId == questionnaireId && i.QuestionnaireVersion == 1);

            interviewBriefStorage = Mock.Of<IReadSideRepositoryWriter<InterviewBrief>>(x => x.GetById(interviewId.FormatGuid()) == interviewBriefMock);

            var questionIdToVariableMap = new Dictionary<Guid, string>() { { questionId, "var" } };

            var questionsInfoMock = Mock.Of<QuestionnaireQuestionsInfo>(x => x.QuestionIdToVariableMap == questionIdToVariableMap);

            variablesStorage = Mock.Of<IReadSideRepositoryWriter<QuestionnaireQuestionsInfo>>(x => x.GetById("11111111111111111111111111111111-1") == questionsInfoMock);
            denormalizer = CreateAnswersByVariableDenormalizer(interviewBriefStorage, variablesStorage, answersByVariableStorageMock.Object);


            evnt = CreateGeoLocationQuestionAnsweredEvent(interviewId, questionId: questionId, latitude: latitude, longitude: longitude,
                propagationVector: new [] { 0.0m });
        };

        Because of = () => denormalizer.Handle(evnt);

        It should_store_one_not_null_view = () =>
            answersCollection.ShouldNotBeNull();

        It should_have_one_row_in_Answers_collection = () =>
            answersCollection.Answers.Count.ShouldEqual(1);

        It should_set_interview_id_as_key_in_Answers_collection = () =>
            answersCollection.Answers.Keys.ShouldContainOnly(new[] { interviewId });

        It should_set_dictionary_with_only_one_row_in_Answers_collection_with_interviewId_key = () =>
            answersCollection.Answers[interviewId].Count.ShouldEqual(1);

        It should_set_dictionary_with_key_equals__sharp__in_Answers_collection_with_interviewId_key = () =>
           answersCollection.Answers[interviewId].Keys.ShouldContainOnly(new[] { "0" });

        It should_set_dictionary_with_value_equals__lat_lon__in_Answers_collection_with_interviewId_key = () =>
           answersCollection.Answers[interviewId].Values.ShouldContainOnly(new[] { "11.154;50.01" });

        private static AnswersByVariableCollection answersCollection;
        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static Guid interviewId = Guid.Parse("22222222222222222222222222222222");
        private static Guid questionId = Guid.Parse("33333333333333333333333333333333");

        private static double latitude = 11.154d;
        private static double longitude = 50.01d;
        private static Mock<IReadSideRepositoryWriter<AnswersByVariableCollection>> answersByVariableStorageMock;
        private static IReadSideRepositoryWriter<InterviewBrief> interviewBriefStorage;
        private static IReadSideRepositoryWriter<QuestionnaireQuestionsInfo> variablesStorage;
        private static AnswersByVariableDenormalizer denormalizer;
        private static IPublishedEvent<GeoLocationQuestionAnswered> evnt;
    }
}
