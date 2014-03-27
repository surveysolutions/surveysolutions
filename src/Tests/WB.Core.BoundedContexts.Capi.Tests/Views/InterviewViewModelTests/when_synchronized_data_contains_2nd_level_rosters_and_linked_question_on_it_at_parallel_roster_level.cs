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

namespace WB.Core.BoundedContexts.Capi.Tests.Views.InterviewViewModelTests
{
    internal class when_synchronized_data_contains_2nd_level_rosters_and_linked_question_on_it_at_parallel_roster_level : InterviewViewModelTestContext
    {
        private Establish context = () =>
        {
            linkedQuestionId = Guid.Parse("33333333333333333333333333333333");
            sourceForLinkedQuestionId = Guid.Parse("11111111111111111111111111111111");
            firstLevelRosterId = Guid.Parse("10000000000000000000000000000000");
            secondLevelRosterId = Guid.Parse("44444444444444444444444444444444");
            parallelLevelRosterId = Guid.Parse("20000000000000000000000000000000");

            questionnarie = CreateQuestionnaireDocumentWithOneChapter(
                new Group()
                {
                    PublicKey = firstLevelRosterId,
                    IsRoster = true,
                    RosterSizeSource = RosterSizeSourceType.FixedTitles,
                    RosterFixedTitles = new[] { "roster1", "roster2" },
                    Children = new List<IComposite>()
                    {
                        new Group()
                        {
                            PublicKey = secondLevelRosterId,
                            IsRoster = true,
                            RosterSizeSource = RosterSizeSourceType.FixedTitles,
                            RosterFixedTitles = new[] { "t1", "t2" },
                            Children = new List<IComposite>()
                            {
                                new NumericQuestion() { PublicKey = sourceForLinkedQuestionId, QuestionType = QuestionType.Numeric }
                            }
                        }
                    }
                },
                new Group()
                {
                    PublicKey = parallelLevelRosterId,
                    IsRoster = true,
                    RosterSizeSource = RosterSizeSourceType.FixedTitles,
                    RosterFixedTitles = new[] { "parallel roster1", "parallel roster1" },
                    Children = new List<IComposite>()
                    {

                        new SingleQuestion()
                        {
                            PublicKey = linkedQuestionId,
                            LinkedToQuestionId = sourceForLinkedQuestionId
                        }
                    }
                });


            rosterStructure = CreateQuestionnaireRosterStructure(questionnarie);

            interviewSynchronizationDto = CreateInterviewSynchronizationDto(
                answers: new[]
                {
                    new AnsweredQuestionSynchronizationDto(sourceForLinkedQuestionId, new decimal[] { 0, 0 }, 11, string.Empty),
                    new AnsweredQuestionSynchronizationDto(sourceForLinkedQuestionId, new decimal[] { 0, 1 }, 12, string.Empty),
                    new AnsweredQuestionSynchronizationDto(sourceForLinkedQuestionId, new decimal[] { 1, 0 }, 21, string.Empty)
                },
                propagatedGroupInstanceCounts: new Dictionary<InterviewItemId, RosterSynchronizationDto[]>()
                {
                    {
                        new InterviewItemId(firstLevelRosterId, new decimal[0]),
                        new[]
                        {
                            new RosterSynchronizationDto(firstLevelRosterId, new decimal[0], 0, null, "roster1"),
                            new RosterSynchronizationDto(firstLevelRosterId, new decimal[0], 1, null, "roster2")
                        }
                    },
                    {
                        new InterviewItemId(secondLevelRosterId, new decimal[] { 0 }),
                        new[]
                        {
                            new RosterSynchronizationDto(secondLevelRosterId, new decimal[] { 0 }, 0, null, "t1"),
                            new RosterSynchronizationDto(secondLevelRosterId, new decimal[] { 0 }, 1, null, "t2"),
                        }
                    },
                    {
                        new InterviewItemId(secondLevelRosterId, new decimal[] { 1 }),
                        new[]
                        {
                            new RosterSynchronizationDto(secondLevelRosterId, new decimal[] { 1 }, 0, null, "t1")
                        }
                    },
                    {
                        new InterviewItemId(parallelLevelRosterId, new decimal[0]),
                        new[]
                        {
                            new RosterSynchronizationDto(parallelLevelRosterId, new decimal[0], 0, null, "parallel roster1"),
                            new RosterSynchronizationDto(parallelLevelRosterId, new decimal[0], 1, null, "parallel roster2")
                        }
                    }
                });
        };

        Because of = () =>
          interviewViewModel = CreateInterviewViewModel(questionnarie, rosterStructure,
              interviewSynchronizationDto);

        It should_linked_in_first_row_has_3_options = () =>
            ((LinkedQuestionViewModel)interviewViewModel.FindQuestion(
                question => question.PublicKey == new InterviewItemId(linkedQuestionId, new decimal[] { 0 }))
                .FirstOrDefault()).AnswerOptions.Count().ShouldEqual(3);

        It should_linked_in_second_row_has_3_options = () =>
         ((LinkedQuestionViewModel)interviewViewModel.FindQuestion(
             question => question.PublicKey == new InterviewItemId(linkedQuestionId, new decimal[] { 1 }))
             .FirstOrDefault()).AnswerOptions.Count().ShouldEqual(3);

        It should_linked_question_in_first_row_has_first_option_equal_to_11 = () =>
         ((LinkedQuestionViewModel)interviewViewModel.FindQuestion(
             question => question.PublicKey == new InterviewItemId(linkedQuestionId, new decimal[] { 0 }))
             .FirstOrDefault()).AnswerOptions.First().Title.ShouldEqual("roster1: 11");

        It should_linked_question_in_first_row_has_second_option_equal_to_12 = () =>
        ((LinkedQuestionViewModel)interviewViewModel.FindQuestion(
            question => question.PublicKey == new InterviewItemId(linkedQuestionId, new decimal[] { 0 }))
            .FirstOrDefault()).AnswerOptions.ToArray()[1].Title.ShouldEqual("roster1: 12");

        It should_linked_question_in_first_row_has_third_option_equal_to_21 = () =>
         ((LinkedQuestionViewModel)interviewViewModel.FindQuestion(
             question => question.PublicKey == new InterviewItemId(linkedQuestionId, new decimal[] { 0 }))
             .FirstOrDefault()).AnswerOptions.Last().Title.ShouldEqual("roster2: 21");

        private static InterviewViewModel interviewViewModel;
        private static QuestionnaireDocument questionnarie;
        private static QuestionnaireRosterStructure rosterStructure;
        private static InterviewSynchronizationDto interviewSynchronizationDto;

        private static Guid firstLevelRosterId;
        private static Guid linkedQuestionId;
        private static Guid secondLevelRosterId;
        private static Guid parallelLevelRosterId;
        private static Guid sourceForLinkedQuestionId;
    }
}
