using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Tests.Unit.BoundedContexts.Capi.InterviewViewModelTests
{
    internal class when_add_roster_row_method_called_and_2_rows_in_roster_already_present : InterviewViewModelTestContext
    {
        Establish context = () =>
        {
            rosterId = Guid.Parse("10000000000000000000000000000000");
            nestedGroupId1 = Guid.Parse("20000000000000000000000000000000");
            nestedGroupId2 = Guid.Parse("30000000000000000000000000000000");
            questionnarie = CreateQuestionnaireDocumentWithOneChapter(
                new Group()
                {
                    PublicKey = rosterId,
                    IsRoster = true,
                    RosterFixedTitles = new []{"a","b","c"},
                    RosterSizeSource = RosterSizeSourceType.FixedTitles,
                    Children = new List<IComposite>()
                    {
                        new Group("nested group 1"){ PublicKey = nestedGroupId1},
                        new Group("nested group 2"){ PublicKey = nestedGroupId2}
                    }
                });

            rosterStructure = CreateQuestionnaireRosterStructure(questionnarie);

            interviewSynchronizationDto = CreateInterviewSynchronizationDto(
               answers: new AnsweredQuestionSynchronizationDto[0],
               propagatedGroupInstanceCounts: new Dictionary<InterviewItemId, RosterSynchronizationDto[]>());

            interviewViewModel = CreateInterviewViewModel(questionnarie, rosterStructure,
             interviewSynchronizationDto);

            PropagateScreen(interviewViewModel, rosterId, 0);
            PropagateScreen(interviewViewModel, rosterId, 1);
        };

        Because of = () =>
            PropagateScreen(interviewViewModel, rosterId, 2);

        It should_added_screen_has_3_siblings = () =>
            interviewViewModel.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(rosterId, new decimal[] { 2 })].Siblings.Count().ShouldEqual(3);

        It should_added_screen_has_first_and_second_roster_row_as_sibling = () =>
            interviewViewModel.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(rosterId, new decimal[] { 2 })].Siblings.Select(
                s => s.InterviewItemPropagationVector.LastOrDefault()).SequenceEqual(new decimal[] { 0, 1, 2 });

        It should_added_screen_siblings_has_roster_id_as_screen_id = () =>
            interviewViewModel.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(rosterId, new decimal[] { 2 })].Siblings.Select(
                s => s.Id).SequenceEqual(new Guid[] { rosterId });

        It should_first_nested_group_of_added_screen_has_2_siblings = () =>
           interviewViewModel.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(nestedGroupId1, new decimal[] { 2 })].Siblings.Count().ShouldEqual(2);

        It should_first_nested_group_of_added_screen_has_nestedGroupId1_and_nestedGroupId2_in_siblings_list = () =>
          interviewViewModel.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(nestedGroupId1, new decimal[] { 2 })].Siblings.Select(
              s => s.Id).SequenceEqual(new Guid[] { nestedGroupId1, nestedGroupId2 });

        It should_first_nested_group_of_added_screen_has_2_siblings_with_the_same_roster_vector_as_nested_group = () =>
            interviewViewModel.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(nestedGroupId1, new decimal[] { 2 })].Siblings.Count(
                s => s.InterviewItemPropagationVector.SequenceEqual(new decimal[] { 2 })).ShouldEqual(2);

        It should_second_nested_group_of_added_screen_has_2_siblings = () =>
          interviewViewModel.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(nestedGroupId2, new decimal[] { 2 })].Siblings.Count().ShouldEqual(2);

        It should_second_nested_group_of_added_screen_has_2_siblings_with_the_same_roster_vector_as_nested_group = () =>
            interviewViewModel.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(nestedGroupId2, new decimal[] { 2 })].Siblings.Count(
                s => s.InterviewItemPropagationVector.SequenceEqual(new decimal[] { 2 })).ShouldEqual(2);

        It should_second_nested_group_of_added_screen_has_nestedGroupId1_and_nestedGroupId2_in_siblings_list = () =>
          interviewViewModel.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(nestedGroupId2, new decimal[] { 2 })].Siblings.Select(
              s => s.Id).SequenceEqual(new Guid[] { nestedGroupId1, nestedGroupId2 });

        private static InterviewViewModel interviewViewModel;
        private static QuestionnaireDocument questionnarie;
        private static QuestionnaireRosterStructure rosterStructure;
        private static InterviewSynchronizationDto interviewSynchronizationDto;

        private static Guid rosterId;
        private static Guid nestedGroupId1;
        private static Guid nestedGroupId2;
    }
}
