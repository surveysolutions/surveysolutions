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
    internal class when_chapter_with_nested_group_is_disabling : InterviewViewModelTestContext
    {
        Establish context = () =>
        {
            chapterId = Guid.Parse("FFF000AAA111EE2DD2EE111AAA000FFF");
            nestedGroupId = Guid.Parse("10000000000000000000000000000000");
            nestedGroupInnterQuestionId = Guid.Parse("11111111111111111111111111111111");
            chapterLevelQuestionId = Guid.Parse("21111111111111111111111111111111");
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
                                PublicKey = nestedGroupId,
                                IsRoster = false,
                                Children = new List<IComposite>
                                {
                                    new NumericQuestion() { PublicKey = nestedGroupInnterQuestionId },
                                    new Group() { PublicKey = nestedGroupInnterGroupId }
                                }
                            },
                            new NumericQuestion() { PublicKey = chapterLevelQuestionId }
                        }
                    }
                }
            };

            rosterStructure = CreateQuestionnaireRosterStructure(questionnarie);

            interviewSynchronizationDto = CreateInterviewSynchronizationDto(
                answers: new AnsweredQuestionSynchronizationDto[0],
                propagatedGroupInstanceCounts: new Dictionary<InterviewItemId, RosterSynchronizationDto[]>());
            interviewViewModel = CreateInterviewViewModel(questionnarie, rosterStructure, interviewSynchronizationDto);
        };

        Because of = () =>
            interviewViewModel.SetScreenStatus(ConversionHelper.ConvertIdAndRosterVectorToString(chapterId, new decimal[0]), false);

        It should_nested_group_be_disabled = () =>
            ((QuestionnaireScreenViewModel)interviewViewModel.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(nestedGroupId, new decimal[0])]).Enabled.ShouldEqual(false);

        It should_nested_group_be_disabled_as_item_inside_chapter = () =>
            ((QuestionnaireNavigationPanelItem)((QuestionnaireScreenViewModel)interviewViewModel.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(chapterId, new decimal[0])]).Items[0]).Enabled.ShouldEqual(false);

        It should_quesition_inside_nested_group_be_disabled = () =>
            ((QuestionViewModel)
                ((QuestionnaireScreenViewModel)interviewViewModel.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(nestedGroupId, new decimal[0])]).Items[0])
                .Status.HasFlag(QuestionStatus.ParentEnabled).ShouldEqual(false);

        It should_quesition_inside_nested_group_be_disabled_if_question_is_queried_from_interviewViewModel = () =>
           interviewViewModel.FindQuestion(q => q.PublicKey.Id == nestedGroupInnterQuestionId).First().Status.HasFlag(QuestionStatus.ParentEnabled).ShouldEqual(false);

        It should_nested_inner_group_be_disabled = () =>
            ((QuestionnaireScreenViewModel)interviewViewModel.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(nestedGroupInnterGroupId, new decimal[0])]).Enabled.ShouldEqual(false);

        It should_nested_inner_group_be_disabled_as_item_inside_chapter = () =>
            ((QuestionnaireNavigationPanelItem)((QuestionnaireScreenViewModel)interviewViewModel.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(nestedGroupId, new decimal[0])]).Items[1]).Enabled.ShouldEqual(false);

        It should_quesition_inside_chapter_be_disabled = () =>
           ((QuestionViewModel)
               ((QuestionnaireScreenViewModel)interviewViewModel.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(chapterId, new decimal[0])]).Items[1])
               .Status.HasFlag(QuestionStatus.ParentEnabled).ShouldEqual(false);

        It should_quesition_inside_chapter_be_disabled_if_question_is_queried_from_interviewViewModel = () =>
           interviewViewModel.FindQuestion(q => q.PublicKey.Id == chapterLevelQuestionId).First().Status.HasFlag(QuestionStatus.ParentEnabled).ShouldEqual(false);

        private static InterviewViewModel interviewViewModel;
        private static QuestionnaireDocument questionnarie;
        private static QuestionnaireRosterStructure rosterStructure;
        private static Guid nestedGroupId;
        private static Guid nestedGroupInnterGroupId;
        private static Guid nestedGroupInnterQuestionId;
        private static Guid chapterLevelQuestionId;
        private static Guid chapterId;
        private static InterviewSynchronizationDto interviewSynchronizationDto;
    }
}
