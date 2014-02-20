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
    internal class when_deleting_roster_row_with_question_inside_nested_group : InterviewViewModelTestContext
    {
        Establish context = () =>
        {
            rosterId = Guid.Parse("10000000000000000000000000000000");
            questionInNestedGroupId = Guid.Parse("20000000000000000000000000000000");
            nestedGroupId = Guid.Parse("30000000000000000000000000000000");

            var rosterSizeQuestionId = Guid.Parse("33333333333333333333333333333333");

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
                        new Group("nested group")
                        {
                            PublicKey = nestedGroupId,
                            Children = new List<IComposite>
                            {
                                new NumericQuestion("question in nested group")
                                {
                                    PublicKey = questionInNestedGroupId,
                                    QuestionType = QuestionType.Numeric
                                }
                            }
                        }
                    }
                });

            rosterStructure = CreateQuestionnaireRosterStructure(questionnarie);

            interviewSynchronizationDto = CreateInterviewSynchronizationDto(
                answers: new AnsweredQuestionSynchronizationDto[0],
                propagatedGroupInstanceCounts: new Dictionary<InterviewItemId, RosterSynchronizationDto[]>()
                {
                    { new InterviewItemId(rosterId, new decimal[0]), new []{ new RosterSynchronizationDto(rosterId, new decimal[0], 0, null, null)}  }
                });

            interviewViewModel = CreateInterviewViewModel(questionnarie, rosterStructure,
             interviewSynchronizationDto);
        };

        Because of = () =>
            interviewViewModel.RemovePropagatedScreen(rosterId, new decimal[0], 0);

        It should_not_contain_screen_with_added_roster_row = () =>
            interviewViewModel.Screens.Keys.ShouldNotContain(new InterviewItemId(rosterId, new decimal[] { 0 }));

        It should_not_contain_screen_with_nested_group = () =>
           interviewViewModel.Screens.Keys.ShouldNotContain(new InterviewItemId(nestedGroupId, new decimal[] { 0 }));

        It should_not_contain_question_from_nested_group = () =>
            interviewViewModel.FindQuestion(q => q.PublicKey == new InterviewItemId(questionInNestedGroupId, new decimal[] { 0 })).ShouldBeEmpty();

        private static InterviewViewModel interviewViewModel;
        private static QuestionnaireDocument questionnarie;
        private static QuestionnaireRosterStructure rosterStructure;
        private static InterviewSynchronizationDto interviewSynchronizationDto;

        private static Guid rosterId;
        private static Guid nestedGroupId;
        private static Guid questionInNestedGroupId;
    }
}
