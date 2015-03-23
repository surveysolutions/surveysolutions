using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    internal class when_add_nested_roster_row_method_called_and_1_row_in_roster_already_present_and_other_row_present_in_other_parent_roster_row : InterviewViewModelTestContext
    {
        Establish context = () =>
        {
            rosterId = Guid.Parse("10000000000000000000000000000000");
            nestedRosterId = Guid.Parse("20000000000000000000000000000000");
            questionnarie = CreateQuestionnaireDocumentWithOneChapter(
                new Group()
                {
                    PublicKey = rosterId,
                    IsRoster = true,
                    RosterFixedTitles = new[] { "a", "b", "c" },
                    RosterSizeSource = RosterSizeSourceType.FixedTitles,
                    Children = new List<IComposite>
                    {
                        new Group()
                        {
                            PublicKey = nestedRosterId,
                            IsRoster = true,
                            RosterFixedTitles = new[] { "1", "2", "3" },
                            RosterSizeSource = RosterSizeSourceType.FixedTitles
                        }
                    }
                });

            rosterStructure = CreateQuestionnaireRosterStructure(questionnarie);

            interviewSynchronizationDto = CreateInterviewSynchronizationDto(
               answers: new AnsweredQuestionSynchronizationDto[0],
               propagatedGroupInstanceCounts: new Dictionary<InterviewItemId, RosterSynchronizationDto[]>());

            interviewViewModel = CreateInterviewViewModel(questionnarie, rosterStructure,
             interviewSynchronizationDto);

            PropagateScreen(interviewViewModel, rosterId, 0);
            PropagateScreen(interviewViewModel, nestedRosterId, 0, new decimal[]{0});
            PropagateScreen(interviewViewModel, rosterId, 1);
            PropagateScreen(interviewViewModel, nestedRosterId, 0, new decimal[] { 1 });
        };

        Because of = () =>
            PropagateScreen(interviewViewModel, nestedRosterId, 1, new decimal[] { 0 });

        It should_added_screen_has_2_siblings = () =>
            interviewViewModel.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(nestedRosterId, new decimal[] { 0, 1 })].Siblings.Count().ShouldEqual(2);

        It should_added_screen_has_first_roster_row_as_sibling = () =>
            interviewViewModel.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(nestedRosterId, new decimal[] { 0, 1 })].Siblings.Select(
                s => s.InterviewItemPropagationVector.LastOrDefault()).SequenceEqual(new decimal[] { 0, 1 });

        It should_added_screen_siblings_has_nestedRosterId_as_screen_id = () =>
            interviewViewModel.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(nestedRosterId, new decimal[] { 0, 1 })].Siblings.Select(
                s => s.Id).SequenceEqual(new [] { nestedRosterId });

        private static InterviewViewModel interviewViewModel;
        private static QuestionnaireDocument questionnarie;
        private static QuestionnaireRosterStructure rosterStructure;
        private static InterviewSynchronizationDto interviewSynchronizationDto;

        private static Guid rosterId;
        private static Guid nestedRosterId;
    }
}
