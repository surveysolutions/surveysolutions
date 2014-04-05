using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Core.Supervisor.Tests.Merger
{
    [Ignore("Nested rosters bug 1")]
    internal class when_merging_questionnaire_and_interview_data_with_two_nested_roster_inside_roster_by_different_roster_size_ids : InterviewDataAndQuestionnaireMergerTestContext
    {
        Establish context = () =>
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
            merger = CreateMerger();

            var questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(
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
                            PublicKey = nestedRosterSizeQuestionId,
                            QuestionType = QuestionType.Numeric,
                            StataExportCaption = "nestedRosterQuestionId"
                        },
                        new Group(nestedRosterTitle)
                        {
                            PublicKey = nestedRosterId,
                            IsRoster = true,
                            RosterSizeQuestionId = nestedRosterSizeQuestionId,
                            Children =
                                new List<IComposite>()
                                {
                                    new NumericQuestion()
                                    {
                                        PublicKey = questionInNestedRosterId,
                                        QuestionType = QuestionType.Numeric,
                                        StataExportCaption = "questionInNestedRosterId"
                                    },

                                    new Group(nestedRosterTitle)
                                        {
                                            PublicKey = nestedRosterId_level2,
                                            IsRoster = true,
                                            RosterSizeQuestionId = questionInNestedRosterId,
                                            Children =
                                                new List<IComposite>()
                                                {
                                                    new NumericQuestion()
                                                    {
                                                        PublicKey = questionInNestedRosterId_level2,
                                                        QuestionType = QuestionType.Numeric,
                                                        StataExportCaption = "questionInNestedRosterId_l2"
                                                    }
                                                }
                                        }
                                }
                        }
                    }
                });

            interview = CreateInterviewData(interviewId);

            AddInterviewLevel(interview, rosterSizeQuestionId, new decimal[] { 0 },
                new Dictionary<Guid, object> { { nestedRosterSizeQuestionId, 2 } });


            questionnaire = CreateQuestionnaireWithVersion(questionnaireDocument);
            questionnaireReferenceInfo = CreateQuestionnaireReferenceInfo();
            questionnaireRosters = CreateQuestionnaireRosterStructure(questionnaireDocument);
            user = Mock.Of<UserDocument>();
        };

        Because of = () =>
            mergeResult = merger.Merge(interview, questionnaire, questionnaireReferenceInfo, questionnaireRosters, user);

        It should_create_1_group_screens = () =>
            mergeResult.Groups.Count.ShouldEqual(1);


        private static InterviewDataAndQuestionnaireMerger merger;
        private static InterviewDetailsView mergeResult;
        private static InterviewData interview;
        private static QuestionnaireDocumentVersioned questionnaire;
        private static ReferenceInfoForLinkedQuestions questionnaireReferenceInfo;
        private static QuestionnaireRosterStructure questionnaireRosters;
        private static UserDocument user;
        private static Guid nestedRosterId = Guid.Parse("11111111111111111111111111111111");

        private static Guid nestedRosterId_level2 = Guid.Parse("21111111111111111111111111111111");

        private static Guid questionInNestedRosterId = Guid.Parse("55555555555555555555555555555555");

        private static Guid questionInNestedRosterId_level2 = Guid.Parse("55555555555555555555555555555555");
        private static Guid rosterId = Guid.Parse("22222222222222222222222222222222");
        private static Guid interviewId = Guid.Parse("33333333333333333333333333333333");
        private static Guid rosterSizeQuestionId = Guid.Parse("44444444444444444444444444444444");
        private static Guid nestedRosterSizeQuestionId = Guid.Parse("77777777777777777777777777777777");
        private static string nestedRosterTitle = "nested Roster";
    }
}
