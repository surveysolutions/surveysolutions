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

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Merger
{
    internal class when_merging_questionnaire_and_interview_data_with_static_text_question_which_uses_numeric_question_in_substitution : InterviewDataAndQuestionnaireMergerTestContext
    {
        Establish context = () =>
        {
            staticTextWithSubstitutionId = Guid.Parse("11111111111111111111111111111111");
            numericQuestionId = Guid.Parse("20000000000000000000000000000000");
            groupId    = Guid.Parse("55555555555555555555555555555555");

            interviewId = Guid.Parse("43333333333333333333333333333333");

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                Create.Entity.FixedRoster(rosterId: groupId,
                    obsoleteFixedTitles: new[] {"a", "b", "c"},
                    children: new IComposite[]
                    {
                        new StaticText(staticTextWithSubstitutionId, "test %q1%", null, false, null),
                        new NumericQuestion()
                        {
                            PublicKey = numericQuestionId,
                            StataExportCaption = "q1",
                            QuestionType = QuestionType.Numeric,
                            IsInteger = true
                        }
                    }));

            interview = CreateInterviewData(interviewId);

            Setup.InstanceToMockedServiceLocator<ISubstitutionService>(new SubstitutionService());

            AddInterviewLevel(interview, new ValueVector<Guid> { groupId }, new decimal[] { 0 },
                new Dictionary<Guid, object>() { { numericQuestionId, 1 } },
                new Dictionary<Guid, string>() { { groupId, "a" } });
            AddInterviewLevel(interview, new ValueVector<Guid> { groupId }, new decimal[] { 1 },
                new Dictionary<Guid, object>() { { numericQuestionId, 2 } },
                new Dictionary<Guid, string>() { { groupId, "b" } });
            AddInterviewLevel(interview, new ValueVector<Guid> { groupId }, new decimal[] { 2 },
                new Dictionary<Guid, object>(),
                new Dictionary<Guid, string>() { { groupId, "c" } });
            
            user = Mock.Of<UserDocument>();

            merger = CreateMerger(questionnaire);
        };

        Because of = () =>
            mergeResult = merger.Merge(interview, questionnaire, user.GetUseLight(), null, null);


        It should_title_of_statictext_in_first_row_has_variable_replaced_with_1 = () =>
            GetStaticText(mergeResult, staticTextWithSubstitutionId, new decimal[] { 0 }).Text.ShouldEqual("test 1");

        It should_title_of_statictext_in_second_row_has_variable_replaced_with_2 = () =>
            GetStaticText(mergeResult, staticTextWithSubstitutionId, new decimal[] { 1 }).Text.ShouldEqual("test 2");

        It should_title_of_statictext_in_third_row_has_variable_replaced_with_ellipsis = () =>
            GetStaticText(mergeResult, staticTextWithSubstitutionId, new decimal[] { 2 }).Text.ShouldEqual("test [...]");


        private static InterviewDataAndQuestionnaireMerger merger;
        private static InterviewDetailsView mergeResult;
        private static InterviewData interview;
        private static QuestionnaireDocument questionnaire;
        private static UserDocument user;

        private static Guid numericQuestionId;
        private static Guid groupId;
        private static Guid staticTextWithSubstitutionId;
        private static Guid interviewId;
    }
}
