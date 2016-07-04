﻿using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Merger
{
    internal class when_merging_questionnaire_and_interview_data_with_roster_inside_roster_by_the_same_roster_size_ids : InterviewDataAndQuestionnaireMergerTestContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                new NumericQuestion()
                {
                    PublicKey = rosterSizeQuestionId,
                    QuestionType = QuestionType.Numeric,
                    StataExportCaption = "rosterSizeQuestionId"
                },
                new Group()
                {
                    PublicKey = rosterId,
                    IsRoster = true,
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    Children = new List<IComposite>
                    {
                        new NumericQuestion()
                        {
                            PublicKey = rosterQuestionId,
                            QuestionType = QuestionType.Numeric,
                            StataExportCaption = "rosterQuestionId"
                        },
                        new Group(nestedRosterTitle)
                        {
                            PublicKey = nestedRosterId,
                            IsRoster = true,
                            RosterSizeQuestionId = rosterSizeQuestionId,
                            Children =
                                new List<IComposite>()
                                {
                                    new NumericQuestion()
                                    {
                                        PublicKey = questionInNestedRosterId,
                                        QuestionType = QuestionType.Numeric,
                                        StataExportCaption = "questionInNestedRosterId"
                                    }
                                }
                        }
                    }
                });

            interview = CreateInterviewData(interviewId);

            AddInterviewLevel(interview, new ValueVector<Guid> { rosterSizeQuestionId }, new decimal[] { 0 },
                new Dictionary<Guid, object> { { rosterQuestionId, 1 } });

            AddInterviewLevel(interview, new ValueVector<Guid> { rosterSizeQuestionId, rosterSizeQuestionId }, new decimal[] { 0, 0 },
                new Dictionary<Guid, object> { { questionInNestedRosterId, 1 } });

            AddInterviewLevel(interview, new ValueVector<Guid> { rosterSizeQuestionId, rosterSizeQuestionId }, new decimal[] { 0, 1 },
                new Dictionary<Guid, object> { { questionInNestedRosterId, 2 } });
            
            user = Mock.Of<UserDocument>();

            merger = CreateMerger(questionnaire);
        };

        Because of = () =>
            mergeResult = merger.Merge(interview, questionnaire, user.GetUseLight(), null, null);

        It should_create_5_group_screens = () =>
            mergeResult.Groups.Count.ShouldEqual(5);

        It should_have_in_first_row_parent_roster_as_separate_screen = () =>
            mergeResult.Groups.FirstOrDefault(g => g.Id == rosterId && g.RosterVector.SequenceEqual(new decimal[] { 0})).ShouldNotBeNull();

        It should_have_in_second_row_nested_roster_as_separate_screen = () =>
            mergeResult.Groups.FirstOrDefault(
                g =>
                    g.Id == nestedRosterId && g.RosterVector.SequenceEqual(new decimal[] { 0, 0 })).ShouldNotBeNull();

        It should_have_in_third_row_nested_roster_as_separate_screen = () =>
           mergeResult.Groups.FirstOrDefault(
                g =>
                    g.Id == nestedRosterId && g.RosterVector.SequenceEqual(new decimal[] { 0, 1 })).ShouldNotBeNull();

        It should_have_in_parent_roster_answered_question = () =>
            mergeResult.Groups.FirstOrDefault(g => g.Id == rosterId && g.RosterVector.Length == 1 && g.RosterVector[0] == 0)
                .Entities.OfType<InterviewQuestionView>().FirstOrDefault(q => q.Id == rosterQuestionId).Answer.ShouldEqual(1);

        It should_have_in_first_nested_roster_answered_question = () =>
            mergeResult.Groups.FirstOrDefault(g => g.Id == nestedRosterId && g.RosterVector.SequenceEqual(new decimal[]{0,0}))
                .Entities.OfType<InterviewQuestionView>().FirstOrDefault(q => q.Id == questionInNestedRosterId).Answer.ShouldEqual(1);

        It should_have_in_second_nested_roster_answered_question = () =>
            mergeResult.Groups.FirstOrDefault(g => g.Id == nestedRosterId && g.RosterVector.SequenceEqual(new decimal[] { 0, 1 }))
                .Entities.OfType<InterviewQuestionView>().FirstOrDefault(q => q.Id == questionInNestedRosterId).Answer.ShouldEqual(2);

        private static InterviewDataAndQuestionnaireMerger merger;
        private static InterviewDetailsView mergeResult;
        private static InterviewData interview;
        private static QuestionnaireDocument questionnaire;
        private static UserDocument user;
        private static Guid nestedRosterId = Guid.Parse("11111111111111111111111111111111");
        private static Guid questionInNestedRosterId = Guid.Parse("55555555555555555555555555555555");
        private static Guid rosterId = Guid.Parse("22222222222222222222222222222222");
        private static Guid interviewId = Guid.Parse("33333333333333333333333333333333");
        private static Guid rosterSizeQuestionId = Guid.Parse("44444444444444444444444444444444");
        private static Guid rosterQuestionId = Guid.Parse("66666666666666666666666666666666");
        private static string nestedRosterTitle = "nested Roster";
    }
}
