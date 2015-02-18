using System;
using System.Collections.Generic;
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
    internal class when_setting_answer_to_roster_title_question_of_2_rosters_in_one_scope_with_shared_title_question : InterviewViewModelTestContext
    {
        Establish context = () =>
        {
            rosterGroupId1 = Guid.Parse("10000000000000000000000000000000");
            rosterGroupId2 = Guid.Parse("20000000000000000000000000000000");
            rosterTitleQuestionId = Guid.Parse("11111111111111111111111111111111");
            rosterSizeQuestionId = Guid.Parse("33333333333333333333333333333333");

            questionnarie = CreateQuestionnaireDocumentWithOneChapter(
                new NumericQuestion()
                {
                    PublicKey = rosterSizeQuestionId,
                    QuestionType = QuestionType.Numeric
                },
                new Group
                {
                    PublicKey = rosterGroupId1,
                    IsRoster = true,
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    RosterTitleQuestionId = rosterTitleQuestionId,
                    Children = new List<IComposite>
                    {
                        new NumericQuestion()
                        {
                            PublicKey = rosterTitleQuestionId,
                            QuestionType = QuestionType.Numeric
                        }
                    }
                },
                new Group()
                {
                    PublicKey = rosterGroupId2,
                    IsRoster = true,
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    RosterTitleQuestionId = rosterTitleQuestionId
                });

            rosterStructure = CreateQuestionnaireRosterStructure(questionnarie);

            interviewSynchronizationDto = CreateInterviewSynchronizationDto(
                answers: new AnsweredQuestionSynchronizationDto[0],
                propagatedGroupInstanceCounts: new Dictionary<InterviewItemId, RosterSynchronizationDto[]>()
                {
                    {
                        new InterviewItemId(rosterGroupId1, new decimal[0]),
                        new[] { new RosterSynchronizationDto(rosterGroupId1, new decimal[0], 0, null, null) }
                    },
                    {
                        new InterviewItemId(rosterGroupId2, new decimal[0]),
                        new[] { new RosterSynchronizationDto(rosterGroupId2, new decimal[0], 0, null, null) }
                    }
                });
            interviewViewModel = CreateInterviewViewModel(questionnarie, rosterStructure, interviewSynchronizationDto);
        };

        Because of = () =>
            interviewViewModel.SetAnswer(ConversionHelper.ConvertIdAndRosterVectorToString(rosterTitleQuestionId, new decimal[] { 0 }), rosterTitle);

        It should_title_of_first_roster_be_equal_set_title = () =>
            ((QuestionnairePropagatedScreenViewModel)interviewViewModel.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(rosterGroupId1, new decimal[] { 0 })]).ScreenName.ShouldEqual(rosterTitle);

        It should_title_of_second_roster_be_equal_set_title = () =>
            ((QuestionnairePropagatedScreenViewModel)interviewViewModel.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(rosterGroupId2, new decimal[] { 0 })]).ScreenName.ShouldEqual(rosterTitle);

        private static InterviewViewModel interviewViewModel;
        private static QuestionnaireDocument questionnarie;
        private static QuestionnaireRosterStructure rosterStructure;
        private static Guid rosterGroupId1;
        private static Guid rosterGroupId2;
        private static Guid rosterSizeQuestionId;
        private static Guid rosterTitleQuestionId;
        private static InterviewSynchronizationDto interviewSynchronizationDto;
        private static string rosterTitle = "roster title";
    }
}
