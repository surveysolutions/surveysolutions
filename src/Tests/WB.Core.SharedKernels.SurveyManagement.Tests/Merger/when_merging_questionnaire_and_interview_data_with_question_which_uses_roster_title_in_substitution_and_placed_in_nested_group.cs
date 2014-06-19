using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.ExpressionProcessor.Implementation.Services;
using WB.Core.SharedKernels.ExpressionProcessor.Services;
using WB.Core.SharedKernels.SurveyManagement.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.Merger
{
    internal class when_merging_questionnaire_and_interview_data_with_question_which_uses_roster_title_in_substitution_and_placed_in_nested_group : InterviewDataAndQuestionnaireMergerTestContext
    {
        Establish context = () =>
        {
            merger = CreateMerger();

            questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(
                new Group()
                {
                    PublicKey = independantRosterId,
                    IsRoster = true,
                    RosterSizeSource = RosterSizeSourceType.FixedTitles,
                    RosterFixedTitles = new[] { "1", "2", "3" }
                },
                new Group()
                {
                    PublicKey = rosterId,
                    IsRoster = true,
                    RosterSizeSource = RosterSizeSourceType.FixedTitles,
                    RosterFixedTitles = new[] { "a", "b", "" },
                    Children = new List<IComposite>()
                    {
                        new Group()
                        {
                            PublicKey = nestedGroupId,
                            Children = new List<IComposite>()
                            {
                                new NumericQuestion()
                                {
                                    PublicKey = questionWithSubstitutionId,
                                    QuestionType = QuestionType.Numeric,
                                    QuestionText = "test %rostertitle%",
                                    StataExportCaption = "var"
                                }
                            }
                        }
                    }
                });

            interview = CreateInterviewData(interviewId);

            SetupInstanceToMockedServiceLocator<ISubstitutionService>(new SubstitutionService());

            AddInterviewLevel(interview, new ValueVector<Guid> { independantRosterId }, new decimal[] { 0 },
                new Dictionary<Guid, object>(),
                new Dictionary<Guid, string>() { { independantRosterId, "1" } });

            AddInterviewLevel(interview, new ValueVector<Guid> { rosterId }, new decimal[] { 0 },
                new Dictionary<Guid, object>(),
                new Dictionary<Guid, string>() { { rosterId, "a" } });
            AddInterviewLevel(interview, new ValueVector<Guid> { rosterId }, new decimal[] { 1 },
                new Dictionary<Guid, object>(),
                new Dictionary<Guid, string>() { { rosterId, "b" } });

            AddInterviewLevel(interview, new ValueVector<Guid> { rosterId }, new decimal[] { 2 },
                new Dictionary<Guid, object>(),
                new Dictionary<Guid, string>() { { rosterId, "" } });


            questionnaire = CreateQuestionnaireWithVersion(questionnaireDocument);
            questionnaireReferenceInfo = CreateQuestionnaireReferenceInfo(questionnaireDocument);
            questionnaireRosters = CreateQuestionnaireRosterStructure(questionnaireDocument);
            user = Mock.Of<UserDocument>();
        };

        Because of = () =>
            mergeResult = merger.Merge(interview, questionnaire, questionnaireReferenceInfo, questionnaireRosters, user);


        It should_title_of_question_in_first_row_has_rostertitle_replaced_with_a = () =>
            GetQuestion(mergeResult, questionWithSubstitutionId, new decimal[] { 0 }).Title.ShouldEqual("test a");

        It should_title_of_question_in_second_row_has_rostertitle_replaced_with_b = () =>
            GetQuestion(mergeResult, questionWithSubstitutionId, new decimal[] { 1 }).Title.ShouldEqual("test b");

        It should_title_of_question_in_third_row_has_rostertitle_replaced_with_ellipsis = () =>
            GetQuestion(mergeResult, questionWithSubstitutionId, new decimal[] { 2 }).Title.ShouldEqual("test [...]");


        private static InterviewDataAndQuestionnaireMerger merger;
        private static InterviewDetailsView mergeResult;
        private static InterviewData interview;
        private static QuestionnaireDocumentVersioned questionnaire;
        private static ReferenceInfoForLinkedQuestions questionnaireReferenceInfo;
        private static QuestionnaireRosterStructure questionnaireRosters;
        private static UserDocument user;

        private static Guid rosterId = Guid.Parse("20000000000000000000000000000000");
        private static Guid nestedGroupId = Guid.Parse("30000000000000000000000000000000");
        private static Guid independantRosterId = Guid.Parse("33333333333333333333333333333333");
        private static Guid questionWithSubstitutionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid interviewId = Guid.Parse("43333333333333333333333333333333");
        private static QuestionnaireDocument questionnaireDocument;
    }
}