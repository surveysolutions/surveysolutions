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
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.Merger
{
    internal class when_merging_questionnaire_and_interview_data_with_group_inside_roster : InterviewDataAndQuestionnaireMergerTestContext
    {
        Establish context = () =>
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
            merger = CreateMerger();

            var rosterSizeQuestionId = Guid.Parse("44444444444444444444444444444444");

            var questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(
                new NumericQuestion()
                {
                    PublicKey = rosterSizeQuestionId,
                    QuestionType = QuestionType.Numeric,
                    StataExportCaption = "q1"
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
                            PublicKey = nestedGroupId,
                            Children =
                                new List<IComposite>()
                                {
                                    new NumericQuestion() { PublicKey = questionInNestedGroupId, QuestionType = QuestionType.Numeric, StataExportCaption = "q2" }
                                }
                        }
                    }
                });

            interview = CreateInterviewData(interviewId);

            AddInterviewLevel(interview, rosterSizeQuestionId, new decimal[] { 0 },
                new Dictionary<Guid, object> { { questionInNestedGroupId, 1 } });

            AddInterviewLevel(interview, rosterSizeQuestionId, new decimal[] { 1 },
                new Dictionary<Guid, object> { { questionInNestedGroupId, 2 } });
            
            questionnaire = CreateQuestionnaireWithVersion(questionnaireDocument);
            questionnaireReferenceInfo = CreateQuestionnaireReferenceInfo();
            questionnaireRosters = CreateQuestionnaireRosterStructure(questionnaireDocument);
            user = Mock.Of<UserDocument>();
        };

        Because of = () =>
            mergeResult = merger.Merge(interview, questionnaire, questionnaireReferenceInfo, questionnaireRosters, user);

        It should_create_6_group_screens = () =>
            mergeResult.Groups.Count.ShouldEqual(6);

        It should_have_in_first_row_nested_group_as_separate_screen = () =>
            mergeResult.Groups.FirstOrDefault(g => g.Id == nestedGroupId && g.RosterVector.Length==1 && g.RosterVector[0]==0).ShouldNotBeNull();

        It should_have_in_second_row_nested_group_as_separate_screen = () =>
           mergeResult.Groups.FirstOrDefault(g => g.Id == nestedGroupId && g.RosterVector.Length == 1 && g.RosterVector[0] == 1).ShouldNotBeNull();

        It should_have_in_first_nested_group_answered_question = () =>
            mergeResult.Groups.FirstOrDefault(g => g.Id == nestedGroupId && g.RosterVector.Length == 1 && g.RosterVector[0] == 0)
                .Questions.FirstOrDefault(q => q.Id == questionInNestedGroupId).Answer.ShouldEqual(1);

        It should_have_in_second_nested_group_answered_question = () =>
            mergeResult.Groups.FirstOrDefault(g => g.Id == nestedGroupId && g.RosterVector.Length == 1 && g.RosterVector[0] == 1)
                .Questions.FirstOrDefault(q => q.Id == questionInNestedGroupId).Answer.ShouldEqual(2);

        private static InterviewDataAndQuestionnaireMerger merger;
        private static InterviewDetailsView mergeResult;
        private static InterviewData interview;
        private static QuestionnaireDocumentVersioned questionnaire;
        private static ReferenceInfoForLinkedQuestions questionnaireReferenceInfo;
        private static QuestionnaireRosterStructure questionnaireRosters;
        private static UserDocument user;
        private static Guid nestedGroupId = Guid.Parse("11111111111111111111111111111111");
        private static Guid questionInNestedGroupId = Guid.Parse("55555555555555555555555555555555");
        private static Guid rosterId = Guid.Parse("22222222222222222222222222222222");
        private static Guid interviewId = Guid.Parse("33333333333333333333333333333333");
        private static string nestedGroupTitle = "nested Group";
    }
}
