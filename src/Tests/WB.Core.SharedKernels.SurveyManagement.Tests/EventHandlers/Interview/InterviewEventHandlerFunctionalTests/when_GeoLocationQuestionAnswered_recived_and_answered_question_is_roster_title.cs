using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.ReadSide;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.EventHandlers.InterviewEventHandlerFunctionalTests
{
    internal class when_GeoLocationQuestionAnswered_recived_and_answered_question_is_roster_title :
        InterviewEventHandlerFunctionalTestContext
    {
        private Establish context = () =>
        {
            rosterGroupId = Guid.Parse("10000000000000000000000000000000");
            rosterScopeId = Guid.Parse("12222222222222222222222222222222");
            rosterTitleQuestionId = Guid.Parse("13333333333333333333333333333333");
            viewState = CreateViewWithSequenceOfInterviewData();
            var questionnaireRosterStructure = CreateQuestionnaireRosterStructure(rosterScopeId,
                new Dictionary<Guid, Guid?>() { { rosterGroupId, rosterTitleQuestionId } });

            interviewEventHandlerFunctional = CreateInterviewEventHandlerFunctional(questionnaireRosterStructure);
            viewState = interviewEventHandlerFunctional.Update(viewState,
                CreatePublishableEvent(new RosterRowAdded(rosterGroupId, new decimal[0], 0, null)));
        };

        private Because of = () =>
            viewState = interviewEventHandlerFunctional.Update(viewState,
                CreatePublishableEvent(new GeoLocationQuestionAnswered(Guid.NewGuid(), rosterTitleQuestionId, new decimal[] { 0 },
                    DateTime.Now,
                    geoPositionAnswer.Latitude, geoPositionAnswer.Longitude, geoPositionAnswer.Accuracy, geoPositionAnswer.Altitude,
                    geoPositionAnswer.Timestamp)));

        private It should_roster_title_be_equal_to_answer_text_representation = () =>
            viewState.Document.Levels["0"].RosterRowTitles[rosterGroupId].ShouldEqual("1,2[3]4");

        private It should_answer_on_head_question_Latitude_be_equal_to_recived_answer_Latitude = () =>
            ((GeoPosition) viewState.Document.Levels["0"].GetAllQuestions().First(q => q.Id == rosterTitleQuestionId).Answer).Latitude
                .ShouldEqual(geoPositionAnswer.Latitude);

        private It should_answer_on_head_question_Longitude_be_equal_to_recived_answer_Longitude = () =>
            ((GeoPosition) viewState.Document.Levels["0"].GetAllQuestions().First(q => q.Id == rosterTitleQuestionId).Answer).Longitude
                .ShouldEqual(geoPositionAnswer.Longitude);

        private It should_answer_on_head_question_Accuracy_be_equal_to_recived_answer_Accuracy = () =>
            ((GeoPosition) viewState.Document.Levels["0"].GetAllQuestions().First(q => q.Id == rosterTitleQuestionId).Answer).Accuracy
                .ShouldEqual(geoPositionAnswer.Accuracy);

        private static InterviewEventHandlerFunctional interviewEventHandlerFunctional;
        private static ViewWithSequence<InterviewData> viewState;
        private static Guid rosterGroupId;
        private static Guid rosterScopeId;
        private static Guid rosterTitleQuestionId;
        private static GeoPosition geoPositionAnswer = new GeoPosition(1, 2, 3, 4, DateTimeOffset.UtcNow);
    }
}
