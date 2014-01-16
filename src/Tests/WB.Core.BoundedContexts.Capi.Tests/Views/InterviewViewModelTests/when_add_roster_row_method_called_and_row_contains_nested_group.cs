using System;
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

namespace WB.Core.BoundedContexts.Capi.Tests.Views.InterviewViewModelTests
{
    internal class when_add_roster_row_method_called_and_row_contains_nested_group : InterviewViewModelTestContext
    {
        Establish context = () =>
        {
            rosterId = Guid.Parse("10000000000000000000000000000000");

            var rosterSizeQuestionId = Guid.Parse("33333333333333333333333333333333");
            nestedGroupId = Guid.Parse("22222222222222222222222222222222");

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
                        new Group("nested Group") { PublicKey = nestedGroupId }
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
            interviewViewModel.AddPropagateScreen(rosterId, new decimal[0], 0, 0);

        It should_contain_grid_screen = () =>
            interviewViewModel.Screens.Keys.ShouldContain(new InterviewItemId(rosterId, new decimal[0]));

        It should_contain_grid_screen_with_nested_group = () =>
            ((QuestionnaireGridViewModel)interviewViewModel.Screens[new InterviewItemId(rosterId, new decimal[0])]).Rows.First().Items[0].PublicKey.ShouldEqual(new InterviewItemId(nestedGroupId, new decimal[] { 0 }));

        It should_contain_screen_with_added_roster_row = () =>
            interviewViewModel.Screens.Keys.ShouldContain(new InterviewItemId(rosterId, new decimal[] { 0 }));

        It should_contain_roster_screen_with_nested_group = () =>
            ((QuestionnairePropagatedScreenViewModel)interviewViewModel.Screens[new InterviewItemId(rosterId, new decimal[] { 0 })]).Items[0].PublicKey.ShouldEqual(new InterviewItemId(nestedGroupId, new decimal[] { 0 }));

        It should_contain_screen_with_added_nested_roster_row = () =>
            interviewViewModel.Screens.Keys.ShouldContain(new InterviewItemId(nestedGroupId, new decimal[] { 0 }));

        It should_not_contain_nested_rosters_grid_screen = () =>
           interviewViewModel.Screens.Keys.ShouldNotContain(new InterviewItemId(nestedGroupId, new decimal[0]));

        It should_last_breadcrumb_of_nested_group_be_equal_to_nested_group_id = () =>
            ((QuestionnairePropagatedScreenViewModel)interviewViewModel.Screens[new InterviewItemId(nestedGroupId, new decimal[] { 0 })]).Breadcrumbs.Last().ShouldEqual(new InterviewItemId(nestedGroupId, new decimal[] { 0 }));

        It should_3n_breadcrumb_of_nested_group_be_equal_to_nested_group_id = () =>
            ((QuestionnairePropagatedScreenViewModel)interviewViewModel.Screens[new InterviewItemId(nestedGroupId, new decimal[] { 0 })]).Breadcrumbs.ToList()[2].ShouldEqual(new InterviewItemId(rosterId, new decimal[] { 0 }));

        It should_2n_breadcrumb_of_nested_group_be_equal_to_nested_group_id = () =>
            ((QuestionnairePropagatedScreenViewModel)interviewViewModel.Screens[new InterviewItemId(nestedGroupId, new decimal[] { 0 })]).Breadcrumbs.ToList()[1].ShouldEqual(new InterviewItemId(rosterId, new decimal[0]));

        private static InterviewViewModel interviewViewModel;
        private static QuestionnaireDocument questionnarie;
        private static QuestionnaireRosterStructure rosterStructure;
        private static InterviewSynchronizationDto interviewSynchronizationDto;

        private static Guid rosterId;
        private static Guid nestedGroupId;
    }
}
