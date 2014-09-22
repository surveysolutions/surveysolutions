using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Capi.Tests.CascadingComboboxTests
{
    internal class when_synchronized_data_contains_2nd_level_rosters_and_cascading_questions_on_each_level_and_no_answers : InterviewViewModelTestContext
    {
        private Establish context = () =>
        {
            questionnarie = CreateQuestionnaireWithCascadingQuestions();

            rosterStructure = CreateQuestionnaireRosterStructure(questionnarie);

            interviewSynchronizationDto = CreateInterviewSynchronizationDto(
                answers: new AnsweredQuestionSynchronizationDto[0],
                 propagatedGroupInstanceCounts: new Dictionary<InterviewItemId, RosterSynchronizationDto[]>()
                {
                    {
                        new InterviewItemId(firstLevelRosterId, new decimal[0]),
                        new[]
                        {
                            new RosterSynchronizationDto(firstLevelRosterId, new decimal[0], 0, null, "r 1"),
                            new RosterSynchronizationDto(firstLevelRosterId, new decimal[0], 1, null, "r 2")
                        }
                    },
                    {
                        new InterviewItemId(secondLevelRosterId, new decimal[] { 0 }),
                        new[]
                        {
                            new RosterSynchronizationDto(secondLevelRosterId, new decimal[] { 0 }, 0, null, "r 1.1"),
                            new RosterSynchronizationDto(secondLevelRosterId, new decimal[] { 0 }, 1, null, "r 1.2"),
                        }
                    },
                    {
                        new InterviewItemId(secondLevelRosterId, new decimal[] { 1 }),
                        new[]
                        {
                            new RosterSynchronizationDto(secondLevelRosterId, new decimal[] { 1 }, 0, null, "r 1.1"),
                            new RosterSynchronizationDto(secondLevelRosterId, new decimal[] { 1 }, 1, null, "r 1.2")
                        }
                    }
                });
        };

        Because of = () =>
          interviewViewModel = CreateInterviewViewModel(questionnarie, rosterStructure, interviewSynchronizationDto);

        It should_find_3_parent_cascading_questions = () =>
            interviewViewModel.referencedQuestionToCascadingQuestionsMap.Count.ShouldEqual(3);

        It should_find_cascadingC1Id_cascadingC2Id_and_cascadingC4Id_as_parents = () =>
            interviewViewModel.referencedQuestionToCascadingQuestionsMap.Keys.ShouldContainOnly(cascadingC1Id, cascadingC2Id, cascadingC4Id);

        It should_set_cascadingC1Id_as_parent_of_cascadingC2Id_and_cascadingC4Id = () =>
            interviewViewModel.referencedQuestionToCascadingQuestionsMap[cascadingC1Id].ShouldContainOnly(cascadingC2Id, cascadingC4Id);

        It should_set_cascadingC2Id_as_parent_of_cascadingC3Id_and_cascadingC6Id = () =>
            interviewViewModel.referencedQuestionToCascadingQuestionsMap[cascadingC2Id].ShouldContainOnly(cascadingC3Id, cascadingC6Id);

        It should_set_cascadingC4Id_as_parent_of_cascadingC6 = () =>
            interviewViewModel.referencedQuestionToCascadingQuestionsMap[cascadingC4Id].ShouldContainOnly(cascadingC5Id);

        It should_treat_cascadingC1Id_as_question_referenced_by_cascading = () =>
            interviewViewModel.IsQuestionReferencedByAnyCascadingQuestion(cascadingC1Id).ShouldBeTrue();

        It should_treat_cascadingC2Id_as_question_referenced_by_cascading = () =>
            interviewViewModel.IsQuestionReferencedByAnyCascadingQuestion(cascadingC2Id).ShouldBeTrue();

        It should_not_treat_cascadingC3Id_as_question_referenced_by_cascading = () =>
            interviewViewModel.IsQuestionReferencedByAnyCascadingQuestion(cascadingC3Id).ShouldBeFalse();

        It should_treat_cascadingC4Id_as_question_referenced_by_cascading = () =>
            interviewViewModel.IsQuestionReferencedByAnyCascadingQuestion(cascadingC4Id).ShouldBeTrue();

        It should_not_treat_cascadingC5Id_as_question_referenced_by_cascading = () =>
            interviewViewModel.IsQuestionReferencedByAnyCascadingQuestion(cascadingC5Id).ShouldBeFalse();

        It should_not_treat_cascadingC6Id_as_question_referenced_by_cascading = () =>
            interviewViewModel.IsQuestionReferencedByAnyCascadingQuestion(cascadingC6Id).ShouldBeFalse();

        It should_set_2_options_for_cascadingC1Id_question_model = () =>
            GetQuestion<SelectebleQuestionViewModel>(cascadingC1Id, new decimal[] { }).Answers.Count().ShouldEqual(2);

        It should_set_0_options_for_cascadingC2Id_question_model = () =>
            GetQuestion<CascadingComboboxQuestionViewModel>(cascadingC2Id, new decimal[] { }).AnswerOptions.ShouldBeEmpty();

        It should_set_0_options_for_cascadingC3Id_question_model = () =>
            GetQuestion<CascadingComboboxQuestionViewModel>(cascadingC3Id, new decimal[] { }).AnswerOptions.ShouldBeEmpty();

        It should_set_0_options_for_cascadingC4Id_question_model_from_1st_level_roster = () =>
            GetQuestion<CascadingComboboxQuestionViewModel>(cascadingC4Id, new decimal[] { 0 }).AnswerOptions.ShouldBeEmpty();

        It should_set_0_options_for_cascadingC5Id_question_model_2nd_level_roster = () =>
            GetQuestion<CascadingComboboxQuestionViewModel>(cascadingC5Id, new decimal[] { 0 ,0 }).AnswerOptions.ShouldBeEmpty();

        It should_set_0_options_for_cascadingC6Id_question_model_2nd_level_roster = () =>
            GetQuestion<CascadingComboboxQuestionViewModel>(cascadingC6Id, new decimal[] { 0, 1 }).AnswerOptions.ShouldBeEmpty();

        private static T GetQuestion<T>(Guid id, decimal[] propagationVector) where T : class
        {
            var itemId = new InterviewItemId(id, propagationVector);
            var questionViewModel = interviewViewModel.FindQuestion(question => question.PublicKey == itemId).FirstOrDefault();
            return questionViewModel as T;
        }

        private static InterviewViewModel interviewViewModel;
        private static QuestionnaireDocument questionnarie;
        private static QuestionnaireRosterStructure rosterStructure;
        private static InterviewSynchronizationDto interviewSynchronizationDto;
    }
}
