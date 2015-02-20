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
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Tests.Unit.BoundedContexts.Capi.InterviewViewModelTests
{
    internal class when_add_roster_row_method_called_and_row_is_nested_roster : InterviewViewModelTestContext
    {
        Establish context = () =>
        {
            rosterId = Guid.Parse("10000000000000000000000000000000");

            var rosterSizeQuestionId = Guid.Parse("33333333333333333333333333333333");
            var nestedRosterSizeQuestionId = Guid.Parse("43333333333333333333333333333333");
            nestedRosterId = Guid.Parse("22222222222222222222222222222222");

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
                        new Group(nestedGroupTitle)
                        {
                            PublicKey = nestedRosterId,
                            IsRoster = true,
                            RosterSizeQuestionId = nestedRosterSizeQuestionId
                        },
                        new NumericQuestion()
                        {
                            PublicKey = nestedRosterSizeQuestionId,
                            QuestionType = QuestionType.Numeric
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
        };

        Because of = () =>
            PropagateScreen(interviewViewModel, nestedRosterId, 0, new decimal[] { 0 });

        It should_contain_nested_grid_screen = () =>
            interviewViewModel.Screens.Keys.ShouldContain(ConversionHelper.ConvertIdAndRosterVectorToString(nestedRosterId, new decimal[] { 0 }));

        It should_contain_screen_with_added_nested_roster_row = () =>
            interviewViewModel.Screens.Keys.ShouldContain(ConversionHelper.ConvertIdAndRosterVectorToString(nestedRosterId, new decimal[] { 0, 0 }));

        It should_contain_grid_screen_with_nested_roster = () =>
            ((QuestionnaireGridViewModel)interviewViewModel.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(rosterId, new decimal[0])]).Rows.First().Items[0].PublicKey.ShouldEqual(new InterviewItemId(nestedRosterId, new decimal[] { 0 }));

        It should_contain_grid_screen_with_header_with_nested_roster_id = () =>
            ((QuestionnaireGridViewModel)interviewViewModel.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(rosterId, new decimal[0])]).Header[0].PublicKey.ShouldEqual(nestedRosterId);

        It should_contain_grid_screen_with_header_with_nested_roster_title = () =>
            ((QuestionnaireGridViewModel)interviewViewModel.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(rosterId, new decimal[0])]).Header[0].Title.ShouldEqual(nestedGroupTitle);

        It should_contain_roster_screen_with_nested_roster_inside_roster_row = () =>
            ((QuestionnaireGridViewModel)interviewViewModel.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(nestedRosterId, new decimal[] { 0 })]).Rows.First().ScreenId.ShouldEqual(new InterviewItemId(nestedRosterId, new decimal[] { 0, 0 }));

        It should_breadcrumbs_count_of_nested_roster_be_equal_to_4 = () =>
            ((QuestionnaireGridViewModel)interviewViewModel.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(nestedRosterId, new decimal[] { 0 })]).Breadcrumbs.Count().ShouldEqual(4);

        It should_contain_nested_roster_screen_with_empty_screen_name = () =>
            ((QuestionnairePropagatedScreenViewModel)interviewViewModel.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(nestedRosterId, new decimal[] { 0, 0 })]).ScreenName.ShouldEqual(string.Empty);

        It should_last_breadcrumb_of_nested_group_be_equal_to_nested_roster_id = () =>
            ((QuestionnairePropagatedScreenViewModel)interviewViewModel.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(nestedRosterId, new decimal[] { 0, 0 })]).Breadcrumbs.Last().ShouldEqual(new InterviewItemId(nestedRosterId, new decimal[] { 0, 0 }));

        It should_4th_breadcrumb_of_nested_roster_be_equal_to_roster_row_screen_id = () =>
            ((QuestionnairePropagatedScreenViewModel)interviewViewModel.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(nestedRosterId, new decimal[] { 0, 0 })]).Breadcrumbs.ToList()[3].ShouldEqual(new InterviewItemId(nestedRosterId, new decimal[] { 0 }));

        It should_3n_breadcrumb_of_nested_roster_be_equal_to_roster_row_screen_id = () =>
         ((QuestionnairePropagatedScreenViewModel)interviewViewModel.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(nestedRosterId, new decimal[] { 0, 0 })]).Breadcrumbs.ToList()[2].ShouldEqual(new InterviewItemId(rosterId, new decimal[] { 0 }));

        It should_2n_breadcrumb_of_nested_roster_be_equal_to_greed_screen_id = () =>
            ((QuestionnairePropagatedScreenViewModel)interviewViewModel.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(nestedRosterId, new decimal[] { 0, 0 })]).Breadcrumbs.ToList()[1].ShouldEqual(new InterviewItemId(rosterId, new decimal[0]));

        It should_have_1_sibling_of_nested_roster = () =>
            ((QuestionnairePropagatedScreenViewModel)interviewViewModel.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(nestedRosterId, new decimal[] { 0, 0 })]).Siblings.Count().ShouldEqual(1);

        It should_have_him_self_in_list_of_siblings = () =>
            ((QuestionnairePropagatedScreenViewModel)interviewViewModel.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(nestedRosterId, new decimal[] { 0, 0 })]).Siblings.First().ShouldEqual(new InterviewItemId(nestedRosterId, new decimal[] { 0, 0 }));

        It should_have_him_self_in_list_of_siblings_for_nested_roster_grid = () =>
          ((QuestionnaireGridViewModel)interviewViewModel.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(nestedRosterId, new decimal[] { 0 })]).Siblings.First().ShouldEqual(new InterviewItemId(nestedRosterId, new decimal[] { 0 }));

        private static InterviewViewModel interviewViewModel;
        private static QuestionnaireDocument questionnarie;
        private static QuestionnaireRosterStructure rosterStructure;
        private static InterviewSynchronizationDto interviewSynchronizationDto;

        private static Guid rosterId;
        private static Guid nestedRosterId;
        private static string nestedGroupTitle;
    }
}
