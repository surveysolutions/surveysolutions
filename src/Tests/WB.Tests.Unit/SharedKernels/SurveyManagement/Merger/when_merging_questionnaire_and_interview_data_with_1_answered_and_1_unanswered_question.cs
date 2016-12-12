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
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Merger
{
    internal class when_merging_questionnaire_and_interview_data_with_1_answered_and_1_unanswered_question_and_parent_group_is_enabled : InterviewDataAndQuestionnaireMergerTestContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                new Group(nestedGroupTitle)
                {
                    PublicKey = nestedGroupId,
                    Children =
                        new List<IComposite>()
                        {
                            new NumericQuestion()
                            {
                                PublicKey = answeredQuestionId,
                                QuestionType = QuestionType.Numeric,
                                StataExportCaption = "q1"
                            },
                            new NumericQuestion()
                            {
                                PublicKey = unAnsweredQuestionId,
                                QuestionType = QuestionType.Numeric,
                                StataExportCaption = "q2"
                            }
                        }.ToReadOnlyCollection()
                });

            interview = CreateInterviewData(interviewId);

            if (!interview.Levels["#"].QuestionsSearchCache.ContainsKey(answeredQuestionId))
                interview.Levels["#"].QuestionsSearchCache.Add(answeredQuestionId, new InterviewQuestion(answeredQuestionId));

            var answeredQuestion = interview.Levels["#"].QuestionsSearchCache[answeredQuestionId];

            answeredQuestion.Answer = 5;
            
            user = Mock.Of<UserDocument>();

            merger = CreateMerger(questionnaire);
        };

        Because of = () =>
            mergeResult = merger.Merge(interview, questionnaire, user.GetUseLight(), null, null);

        It should_answered_question_be_enabled = () =>
            GetAnsweredQuestion().IsEnabled.ShouldBeTrue();

        It should_answered_question_be_readonly = () =>
            GetAnsweredQuestion().IsReadOnly.ShouldBeTrue();

        It should_unanswered_question_be_enabled = () =>
            GetUnAnsweredQuestion().IsEnabled.ShouldBeTrue();

        It should_unanswered_question_be_readonly = () =>
            GetUnAnsweredQuestion().IsReadOnly.ShouldBeTrue();

        private static InterviewGroupView GetNestedGroup()
        {
            return mergeResult.Groups.Find(g => g.Id == nestedGroupId);
        }

        private static InterviewQuestionView GetAnsweredQuestion()
        {
            return GetNestedGroup().Entities.OfType<InterviewQuestionView>().FirstOrDefault(q => q.Id == answeredQuestionId);
        }

        private static InterviewQuestionView GetUnAnsweredQuestion()
        {
            return GetNestedGroup().Entities.OfType<InterviewQuestionView>().FirstOrDefault(q => q.Id == unAnsweredQuestionId);
        }

        private static InterviewDataAndQuestionnaireMerger merger;
        private static InterviewDetailsView mergeResult;
        private static InterviewData interview;
        private static QuestionnaireDocument questionnaire;
        private static UserDocument user;
        private static Guid nestedGroupId = Guid.Parse("11111111111111111111111111111111");
        private static Guid answeredQuestionId = Guid.Parse("55555555555555555555555555555555");
        private static Guid unAnsweredQuestionId = Guid.Parse("66666666666666666666666666666666");
        private static Guid interviewId = Guid.Parse("33333333333333333333333333333333");
        private static string nestedGroupTitle = "nested Group";
    }
}
