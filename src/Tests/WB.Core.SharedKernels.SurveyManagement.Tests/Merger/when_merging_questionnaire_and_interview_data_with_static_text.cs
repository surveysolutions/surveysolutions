using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.Merger
{
    internal class when_merging_questionnaire_and_interview_data_with_static_text : InterviewDataAndQuestionnaireMergerTestContext
    {
        Establish context = () =>
        {
            merger = CreateMerger();
            
            var questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(
                new Group(nestedGroupTitle)
                {
                    PublicKey = nestedGroupId,
                    Children =
                        new List<IComposite>()
                        {
                            new StaticText(entityId: staticTextId, text: staticText)
                        }
                });

            interview = CreateInterviewData(interviewId);
            
            questionnaire = CreateQuestionnaireWithVersion(questionnaireDocument);
            questionnaireReferenceInfo = CreateQuestionnaireReferenceInfo();
            questionnaireRosters = CreateQuestionnaireRosterStructure(questionnaireDocument);
            user = Mock.Of<UserDocument>();
        };

        Because of = () =>
            mergeResult = merger.Merge(interview, questionnaire, questionnaireReferenceInfo, questionnaireRosters, user);

        It should_answered_question_be_enabled = () =>
            GetStaticText().ShouldNotBeNull();

        It should_answered_question_be_readonly = () =>
            GetStaticText().Text.ShouldEqual(staticText);
        
        private static InterviewGroupView GetNestedGroup()
        {
            return mergeResult.Groups.Find(g => g.Id == nestedGroupId);
        }

        private static InterviewStaticTextView GetStaticText()
        {
            return GetNestedGroup().Entities.OfType<InterviewStaticTextView>().FirstOrDefault(q => q.Id == staticTextId);
        }

        private static InterviewDataAndQuestionnaireMerger merger;
        private static InterviewDetailsView mergeResult;
        private static InterviewData interview;
        private static QuestionnaireDocumentVersioned questionnaire;
        private static ReferenceInfoForLinkedQuestions questionnaireReferenceInfo;
        private static QuestionnaireRosterStructure questionnaireRosters;
        private static UserDocument user;
        private static Guid nestedGroupId = Guid.Parse("11111111111111111111111111111111");
        private static Guid staticTextId = Guid.Parse("55555555555555555555555555555555");
        private static Guid interviewId = Guid.Parse("33333333333333333333333333333333");
        private static string nestedGroupTitle = "nested Group";
        private static string staticText;
    }
}
