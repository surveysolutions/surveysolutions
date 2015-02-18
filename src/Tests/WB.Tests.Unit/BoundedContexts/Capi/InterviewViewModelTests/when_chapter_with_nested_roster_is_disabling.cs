using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
    internal class when_chapter_with_nested_roster_is_disabling : InterviewViewModelTestContext
    {
        Establish context = () =>
        {
            chapterId = Guid.Parse("FFF000AAA111EE2DD2EE111AAA000FFF");
            rosterGroupId = Guid.Parse("10000000000000000000000000000000");
            nestedGroupInnterQuestionId = Guid.Parse("11111111111111111111111111111111");
            nestedGroupInnterGroupId = Guid.Parse("33333333333333333333333333333333");

            questionnarie = new QuestionnaireDocument
            {
                Children = new List<IComposite>
                {
                    new Group("Chapter")
                    {
                        PublicKey = chapterId,
                        Children = new List<IComposite>()
                        {
                            new Group()
                            {
                                PublicKey = rosterGroupId,
                                IsRoster = true,
                                RosterSizeSource=RosterSizeSourceType.FixedTitles,
                                Children = new List<IComposite>
                                {
                                    new NumericQuestion() { PublicKey = nestedGroupInnterQuestionId },
                                    new Group() { PublicKey = nestedGroupInnterGroupId }
                                }
                            }
                        },
                    }
                }
            };

            rosterStructure = CreateQuestionnaireRosterStructure(questionnarie);

            interviewSynchronizationDto = CreateInterviewSynchronizationDto(
                answers: new AnsweredQuestionSynchronizationDto[0],
                propagatedGroupInstanceCounts: new Dictionary<InterviewItemId, RosterSynchronizationDto[]>());
            interviewViewModel = CreateInterviewViewModel(questionnarie, rosterStructure, interviewSynchronizationDto);

            PropagateScreen(interviewViewModel, rosterGroupId, 0);
            PropagateScreen(interviewViewModel, rosterGroupId, 1);
            interviewViewModel.SetAnswer(ConversionHelper.ConvertIdAndRosterVectorToString(nestedGroupInnterQuestionId, new decimal[] { 0 }), 3);
        
        };

        Because of = () => interviewViewModel.SetScreenStatus(ConversionHelper.ConvertIdAndRosterVectorToString(chapterId, new decimal[0]), false);

        It should_roster_be_disabled = () =>
            ((QuestionnaireGridViewModel)interviewViewModel.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(rosterGroupId, new decimal[0])]).Enabled.ShouldEqual(false);

        It should_roster_be_disabled_as_item_inside_chapter = () =>
            ((QuestionnaireNavigationPanelItem)((QuestionnaireScreenViewModel)interviewViewModel.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(chapterId, new decimal[0])]).Items[0]).Enabled.ShouldEqual(false);

        It should_first_row_of_Roster_be_disabled_as_screen = () =>
            ((QuestionnaireScreenViewModel)interviewViewModel.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(rosterGroupId, new decimal[] { 0 })]).Enabled.ShouldEqual(false);

        It should_second_row_of_Roster_be_disabled_as_screen = () =>
           ((QuestionnaireScreenViewModel)interviewViewModel.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(rosterGroupId, new decimal[] { 1 })]).Enabled.ShouldEqual(false);

        It should_roster_first_row_be_disabled = () =>
            ((QuestionnaireGridViewModel)interviewViewModel.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(rosterGroupId, new decimal[0])]).Rows.First().Enabled.ShouldEqual(false);

        It should_roster_second_row_be_disabled = () =>
            ((QuestionnaireGridViewModel)interviewViewModel.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(rosterGroupId, new decimal[0])]).Rows.Last().Enabled.ShouldEqual(false);

        It should_roster_total_question_count_be_equal_to_0 = () =>
            ((QuestionnaireGridViewModel)interviewViewModel.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(rosterGroupId, new decimal[0])]).Total.ShouldEqual(0);

        It should_roster_answered_question_count_be_equal_to_1 = () =>
           ((QuestionnaireGridViewModel)interviewViewModel.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(rosterGroupId, new decimal[0])]).Answered.ShouldEqual(0);

        private static InterviewViewModel interviewViewModel;
        private static QuestionnaireDocument questionnarie;
        private static QuestionnaireRosterStructure rosterStructure;
        private static Guid rosterGroupId;
        private static Guid nestedGroupInnterGroupId;
        private static Guid nestedGroupInnterQuestionId;
        private static Guid chapterId;
        private static InterviewSynchronizationDto interviewSynchronizationDto;
    }
}
