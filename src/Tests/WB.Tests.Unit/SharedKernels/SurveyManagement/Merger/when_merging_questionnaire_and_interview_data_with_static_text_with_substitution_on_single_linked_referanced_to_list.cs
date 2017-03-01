using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using It = Machine.Specifications.It;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Merger
{
    internal class when_merging_questionnaire_and_interview_data_with_static_text_with_substitution_on_single_linked_referanced_to_list : InterviewDataAndQuestionnaireMergerTestContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                Create.Entity.TextListQuestion(textListQuestionId, variable: "list"),
                Create.Entity.SingleOptionQuestion(linkedSingleQuestionId, "single", linkedToQuestionId: textListQuestionId),
                Create.Entity.StaticText(staticTextWithSubstitutionId, "test %single%")
            );

            interview = CreateInterviewData(interviewId);

            Setup.InstanceToMockedServiceLocator<ISubstitutionService>(new SubstitutionService());

            interview.Levels["#"].QuestionsSearchCache.Add(textListQuestionId, Create.Entity.InterviewQuestion(textListQuestionId, Create.Entity.InterviewTextListAnswers(new[]
                        {
                            new Tuple<decimal, string>(1, "line1"),
                            new Tuple<decimal, string>(2, "line2"),
                        })));
            interview.Levels["#"].QuestionsSearchCache.Add(linkedSingleQuestionId, Create.Entity.InterviewQuestion(linkedSingleQuestionId, 2m));


            user = Mock.Of<UserDocument>();

            merger = CreateMerger(questionnaire);
        };

        Because of = () =>
            mergeResult = merger.Merge(interview, questionnaire, user.GetUseLight(), null, null);


        It should_title_of_statictext_has_title_replaced_with_answer_on_linked = () =>
            GetStaticText(mergeResult, staticTextWithSubstitutionId, new decimal[] { }).Text.ShouldEqual("test line2");



        private static InterviewDataAndQuestionnaireMerger merger;
        private static InterviewDetailsView mergeResult;
        private static InterviewData interview;
        private static QuestionnaireDocument questionnaire;
        private static UserDocument user;

        private static readonly Guid linkedSingleQuestionId       = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid textListQuestionId           = Guid.Parse("55555555555555555555555555555555");
        private static readonly Guid staticTextWithSubstitutionId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid interviewId                  = Guid.Parse("43333333333333333333333333333333");
    }
}
