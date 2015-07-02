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
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Merger
{
    internal class when_merging_questionnaire_and_interview_data_with_disabled_chapter_group_inside_it_and_question_inside_group : InterviewDataAndQuestionnaireMergerTestContext
    {
        Establish context = () =>
        {
            merger = CreateMerger();

            var chapterId = Guid.Parse("44444444444444444444444444444444");

            var questionnaireDocument = new QuestionnaireDocument
            {
                Children = new List<IComposite>
                {
                    new Group("Chapter")
                    {
                        PublicKey = chapterId,
                        Children =    new List<IComposite>()
                        {
                            new Group(nestedGroupTitle)
                            {
                                PublicKey = nestedGroupId,
                                Children =
                                    new List<IComposite>()
                                    {
                                        new NumericQuestion()
                                        {
                                            PublicKey = questionInNestedGroupId,
                                            QuestionType = QuestionType.Numeric,
                                            StataExportCaption = "q2"
                                        }
                                    }
                            }
                        }
                    }
                }
            };

            interview = CreateInterviewData(interviewId);

            interview.Levels["#"].DisabledGroups.Add(chapterId);
            
            if (!interview.Levels["#"].QuestionsSearchCahche.ContainsKey(questionInNestedGroupId))
                interview.Levels["#"].QuestionsSearchCahche.Add(questionInNestedGroupId, new InterviewQuestion(questionInNestedGroupId));

            var answeredQuestion = interview.Levels["#"].QuestionsSearchCahche[questionInNestedGroupId];


            answeredQuestion.Answer = 5;

            questionnaire = CreateQuestionnaireWithVersion(questionnaireDocument);
            questionnaireReferenceInfo = CreateQuestionnaireReferenceInfo();
            questionnaireRosters = CreateQuestionnaireRosterStructure(questionnaireDocument);
            user = Mock.Of<UserDocument>();
        };

        Because of = () =>
            mergeResult = merger.Merge(interview, questionnaire, questionnaireReferenceInfo, questionnaireRosters, user);

        It should_question_be_disabled = () =>
            ((InterviewQuestionView)mergeResult.Groups.FirstOrDefault(g => g.Id == nestedGroupId).Entities[0]).IsEnabled.ShouldEqual(false);

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
