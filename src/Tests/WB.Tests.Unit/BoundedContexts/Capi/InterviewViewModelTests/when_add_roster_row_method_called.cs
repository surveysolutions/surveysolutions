using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Tests.Unit.BoundedContexts.Capi.InterviewViewModelTests
{
    internal class when_add_roster_row_method_called : InterviewViewModelTestContext
    {
        Establish context = () =>
        {
            rosterId1 = Guid.Parse("10000000000000000000000000000000");
            rosterId2=Guid.Parse("22222222222222222222222222222222");
            
            var rosterSizeQuestionId = Guid.Parse("33333333333333333333333333333333");

            questionnarie = CreateQuestionnaireDocumentWithOneChapter(
                new NumericQuestion()
                {
                    PublicKey = rosterSizeQuestionId,
                    QuestionType = QuestionType.Numeric
                },
                new Group("roster title") { PublicKey = rosterId1, IsRoster = true, RosterSizeQuestionId = rosterSizeQuestionId },
                new Group() { PublicKey = rosterId2, IsRoster = true, RosterSizeQuestionId = rosterSizeQuestionId });

            rosterStructure = CreateQuestionnaireRosterStructure(questionnarie);

            interviewSynchronizationDto = CreateInterviewSynchronizationDto(
               answers: new AnsweredQuestionSynchronizationDto[0],
               propagatedGroupInstanceCounts: new Dictionary<InterviewItemId, RosterSynchronizationDto[]>());

            interviewViewModel = CreateInterviewViewModel(questionnarie, rosterStructure,
             interviewSynchronizationDto);

            PropagateScreen(interviewViewModel, rosterId2, 0);
        };

        Because of = () =>
             PropagateScreen(interviewViewModel, rosterId1, 0);

        It should_contain_screen_with_added_roster_row = () =>
            interviewViewModel.Screens.Keys.ShouldContain(ConversionHelper.ConvertIdAndRosterVectorToString(rosterId1, new decimal[] { 0 }));

        It should_contain_roster_with_first_row_with_empty_Screen_name = () =>
           ((QuestionnaireGridViewModel)interviewViewModel.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(rosterId1, new decimal[0])]).Rows.First().ScreenName.ShouldEqual(string.Empty);

        It should_contain_screen_with_3_breadcrumbs = () =>
            ((QuestionnairePropagatedScreenViewModel)interviewViewModel.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(rosterId1, new decimal[] { 0 })]).Breadcrumbs.Count().ShouldEqual(3);

        It should_contain_roster_screen_with_empty_screen_title = () =>
           ((QuestionnairePropagatedScreenViewModel)interviewViewModel.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(rosterId1, new decimal[] { 0 })]).ScreenName.ShouldEqual(string.Empty);

        It should_contain_screen_with_last_breadcrumb_of_current_Screen = () =>
            ((QuestionnairePropagatedScreenViewModel)interviewViewModel.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(rosterId1, new decimal[] { 0 })]).Breadcrumbs.Last().ShouldEqual(new InterviewItemId(rosterId1, new decimal[] { 0 }));

        It should_contain_screen_with_2nd_breadcrumb_of_grid_Screen = () =>
            ((QuestionnairePropagatedScreenViewModel)interviewViewModel.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(rosterId1, new decimal[] { 0 })])
                .Breadcrumbs.ToList()[1].ShouldEqual(new InterviewItemId(rosterId1, new decimal[0]));

        private static InterviewViewModel interviewViewModel;
        private static QuestionnaireDocument questionnarie;
        private static QuestionnaireRosterStructure rosterStructure;
        private static InterviewSynchronizationDto interviewSynchronizationDto;

        private static Guid rosterId1;
        private static Guid rosterId2;
    }
}
