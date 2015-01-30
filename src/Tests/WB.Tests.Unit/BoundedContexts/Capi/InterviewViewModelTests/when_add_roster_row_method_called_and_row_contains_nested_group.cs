﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Tests.Unit.BoundedContexts.Capi.InterviewViewModelTests
{
    internal class when_add_roster_row_method_called_and_row_contains_nested_group : InterviewViewModelTestContext
    {
        Establish context = () =>
        {
            rosterId = Guid.Parse("10000000000000000000000000000000");

            var rosterSizeQuestionId = Guid.Parse("33333333333333333333333333333333");
            nestedGroupId = Guid.Parse("22222222222222222222222222222222");

            nestedGroupTitle = "nested Group";
            questionnarie = CreateQuestionnaireDocumentWithOneChapter(
                new NumericQuestion()
                {
                    PublicKey = rosterSizeQuestionId,
                    QuestionType = QuestionType.Numeric
                },
                new Group()
                {
                    PublicKey = rosterId,
                    IsRoster = true,
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    Children = new List<IComposite>
                    {
                        new Group(nestedGroupTitle) { PublicKey = nestedGroupId }
                    }
                });

            rosterStructure = CreateQuestionnaireRosterStructure(questionnarie);

            interviewSynchronizationDto = CreateInterviewSynchronizationDto(
               answers: new AnsweredQuestionSynchronizationDto[0],
               propagatedGroupInstanceCounts: new Dictionary<InterviewItemId, RosterSynchronizationDto[]>());

            interviewViewModel = CreateInterviewViewModel(questionnarie, rosterStructure,
             interviewSynchronizationDto);
        };

        Because of = () =>
             PropagateScreen(interviewViewModel, rosterId, 0);

        It should_contain_grid_screen = () =>
            interviewViewModel.Screens.Keys.ShouldContain(InterviewViewModel.ConvertIdAndRosterVectorToString(rosterId, new decimal[0]));

        It should_contain_grid_screen_with_nested_group = () =>
            ((QuestionnaireGridViewModel)interviewViewModel.Screens[InterviewViewModel.ConvertIdAndRosterVectorToString(rosterId, new decimal[0])]).Rows.First().Items[0].PublicKey.ShouldEqual(new InterviewItemId(nestedGroupId, new decimal[] { 0 }));

        It should_contain_grid_screen_with_header_with_nested_group_id = () =>
            ((QuestionnaireGridViewModel)interviewViewModel.Screens[InterviewViewModel.ConvertIdAndRosterVectorToString(rosterId, new decimal[0])]).Header[0].PublicKey.ShouldEqual(nestedGroupId);

        It should_contain_grid_screen_with_header_with_nested_group_title = () =>
            ((QuestionnaireGridViewModel)interviewViewModel.Screens[InterviewViewModel.ConvertIdAndRosterVectorToString(rosterId, new decimal[0])]).Header[0].Title.ShouldEqual(nestedGroupTitle);

        It should_contain_screen_with_added_roster_row = () =>
            interviewViewModel.Screens.Keys.ShouldContain(InterviewViewModel.ConvertIdAndRosterVectorToString(rosterId, new decimal[] { 0 }));

        It should_contain_roster_screen_with_nested_group_inside_roster_row = () =>
            ((QuestionnairePropagatedScreenViewModel)interviewViewModel.Screens[InterviewViewModel.ConvertIdAndRosterVectorToString(rosterId, new decimal[] { 0 })]).Items[0].PublicKey.ShouldEqual(new InterviewItemId(nestedGroupId, new decimal[] { 0 }));

        It should_contain_screen_with_added_nested_roster_row = () =>
            interviewViewModel.Screens.Keys.ShouldContain(InterviewViewModel.ConvertIdAndRosterVectorToString(nestedGroupId, new decimal[] { 0 }));

        It should_not_contain_nested_rosters_grid_screen = () =>
           interviewViewModel.Screens.Keys.ShouldNotContain(InterviewViewModel.ConvertIdAndRosterVectorToString(nestedGroupId, new decimal[0]));

        It should_breadcrumbs_count_of_nested_group_be_equal_to_4 = () =>
            ((QuestionnairePropagatedScreenViewModel)interviewViewModel.Screens[InterviewViewModel.ConvertIdAndRosterVectorToString(nestedGroupId, new decimal[] { 0 })]).Breadcrumbs.Count().ShouldEqual(4);

        It should_contain_nested_group_screen_with_nested_group_title = () =>
            ((QuestionnairePropagatedScreenViewModel)interviewViewModel.Screens[InterviewViewModel.ConvertIdAndRosterVectorToString(nestedGroupId, new decimal[] { 0 })]).ScreenName.ShouldEqual(nestedGroupTitle);

        It should_last_breadcrumb_of_nested_group_be_equal_to_nested_group_id = () =>
            ((QuestionnairePropagatedScreenViewModel)interviewViewModel.Screens[InterviewViewModel.ConvertIdAndRosterVectorToString(nestedGroupId, new decimal[] { 0 })]).Breadcrumbs.Last().ShouldEqual(new InterviewItemId(nestedGroupId, new decimal[] { 0 }));

        It should_3n_breadcrumb_of_nested_group_be_equal_to_roster_row_screen_id = () =>
            ((QuestionnairePropagatedScreenViewModel)interviewViewModel.Screens[InterviewViewModel.ConvertIdAndRosterVectorToString(nestedGroupId, new decimal[] { 0 })]).Breadcrumbs.ToList()[2].ShouldEqual(new InterviewItemId(rosterId, new decimal[] { 0 }));

        It should_2n_breadcrumb_of_nested_group_be_equal_to_greed_screen_id = () =>
            ((QuestionnairePropagatedScreenViewModel)interviewViewModel.Screens[InterviewViewModel.ConvertIdAndRosterVectorToString(nestedGroupId, new decimal[] { 0 })]).Breadcrumbs.ToList()[1].ShouldEqual(new InterviewItemId(rosterId, new decimal[0]));

        It should_have_1_sibling_of_nested_group = () =>
            ((QuestionnairePropagatedScreenViewModel)interviewViewModel.Screens[InterviewViewModel.ConvertIdAndRosterVectorToString(nestedGroupId, new decimal[] { 0 })]).Siblings.Count().ShouldEqual(1);

        It should_have_him_self_in_list_of_siblings = () =>
            ((QuestionnairePropagatedScreenViewModel)interviewViewModel.Screens[InterviewViewModel.ConvertIdAndRosterVectorToString(nestedGroupId, new decimal[] { 0 })]).Siblings.First().ShouldEqual(new InterviewItemId(nestedGroupId, new decimal[] { 0 }));

        private static InterviewViewModel interviewViewModel;
        private static QuestionnaireDocument questionnarie;
        private static QuestionnaireRosterStructure rosterStructure;
        private static InterviewSynchronizationDto interviewSynchronizationDto;

        private static Guid rosterId;
        private static Guid nestedGroupId;
        private static string nestedGroupTitle;
    }
}
