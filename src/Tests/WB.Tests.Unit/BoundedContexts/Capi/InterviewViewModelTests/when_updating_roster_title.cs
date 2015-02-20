using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Tests.Unit.BoundedContexts.Capi.InterviewViewModelTests
{
    internal class when_updating_roster_title : InterviewViewModelTestContext
    {
        Establish context = () =>
        {
            rosterGroupId = Guid.Parse("10000000000000000000000000000000");
            rosterTitleQuestionId = Guid.Parse("11111111111111111111111111111111");
            rosterSizeQuestionId = Guid.Parse("33333333333333333333333333333333");

            questionnarie = CreateQuestionnaireDocumentWithOneChapter(
                new NumericQuestion()
                {
                    PublicKey = rosterSizeQuestionId,
                    QuestionType = QuestionType.Numeric
                },
                new Group()
                {
                    PublicKey = rosterGroupId,
                    IsRoster = true,
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    RosterTitleQuestionId = rosterTitleQuestionId,
                    Title = groupTitle,
                    Children = new List<IComposite> { new NumericQuestion() { PublicKey = rosterTitleQuestionId } }
                });

            rosterStructure = CreateQuestionnaireRosterStructure(questionnarie);

            interviewSynchronizationDto = CreateInterviewSynchronizationDto(
                answers: new AnsweredQuestionSynchronizationDto[0],
                propagatedGroupInstanceCounts: new Dictionary<InterviewItemId, RosterSynchronizationDto[]>()
                {
                    { new InterviewItemId(rosterGroupId, new decimal[0]), new []{ new RosterSynchronizationDto(rosterGroupId, new decimal[0], 0, null, null)} }
                });
            interviewViewModel = CreateInterviewViewModel(questionnarie, rosterStructure, interviewSynchronizationDto);
        };

        Because of = () =>
            interviewViewModel.UpdateRosterRowTitle(rosterGroupId, new decimal[] { }, 0, rosterTitle);

        It should_roster_title_be_equal_set_title = () =>
            ((QuestionnairePropagatedScreenViewModel)interviewViewModel.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(rosterGroupId, new decimal[] { 0 })]).ScreenName.ShouldEqual(rosterTitle);

        It should_create_numeric_question_with_triggered_roster_titles_filled = () =>
            interviewViewModel.FindQuestion(q => q.PublicKey.Id == rosterSizeQuestionId).First().TriggeredRosters.ShouldEqual(new[] { groupTitle });

        private static InterviewViewModel interviewViewModel;
        private static QuestionnaireDocument questionnarie;
        private static QuestionnaireRosterStructure rosterStructure;
        private static Guid rosterGroupId;
        private static Guid rosterSizeQuestionId;
        private static Guid rosterTitleQuestionId;
        private static InterviewSynchronizationDto interviewSynchronizationDto;
        private static string rosterTitle = "roster title";
        private static string groupTitle = "group Title";
    }
}
