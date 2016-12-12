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
    internal class when_merging_questionnaire_and_interview_data_with_question_in_nested_roster_has_substitution_reference_on_parent_roster : InterviewDataAndQuestionnaireMergerTestContext
    {
        Establish context = () =>
        {
            interviewId = Guid.Parse("43333333333333333333333333333333");
            questionWithSubstitutionId = Guid.Parse("11111111111111111111111111111111");
            nestedRosterId = Guid.Parse("20000000000000000000000000000000");
            parentRosterId = Guid.Parse("30000000000000000000000000000000");


            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                Create.Entity.FixedRoster(rosterId: parentRosterId,
                    variable: "r1",
                    obsoleteFixedTitles: new[] {"0", "1"},
                    children: new IComposite[]
                    {
                        Create.Entity.FixedRoster(rosterId: nestedRosterId,
                            variable: "r2",
                            obsoleteFixedTitles: new[] {"a", "b"},
                            children: new IComposite[]
                            {
                                new NumericQuestion()
                                {
                                    PublicKey = questionWithSubstitutionId,
                                    QuestionType = QuestionType.Numeric,
                                    QuestionText = "test %r1% %r2%",
                                    StataExportCaption = "var"
                                }
                            })
                    }));

            interview = CreateInterviewData(interviewId);

            Setup.InstanceToMockedServiceLocator<ISubstitutionService>(new SubstitutionService());

            AddInterviewLevel(interview, new ValueVector<Guid> { parentRosterId }, new decimal[] { 0 },
                new Dictionary<Guid, object>(),
                new Dictionary<Guid, string>() { { parentRosterId, "0" } });
            AddInterviewLevel(interview, new ValueVector<Guid> { parentRosterId }, new decimal[] { 1 },
                new Dictionary<Guid, object>(),
                new Dictionary<Guid, string>() { { parentRosterId, "1" } });

            AddInterviewLevel(interview, new ValueVector<Guid> { parentRosterId, nestedRosterId }, new decimal[] { 0, 0 },
                new Dictionary<Guid, object>(),
                new Dictionary<Guid, string>() { { nestedRosterId, "a" } });
            AddInterviewLevel(interview, new ValueVector<Guid> { parentRosterId, nestedRosterId }, new decimal[] { 0, 1 },
                new Dictionary<Guid, object>(),
                new Dictionary<Guid, string>() { { nestedRosterId, "b" } });

            AddInterviewLevel(interview, new ValueVector<Guid> { parentRosterId, nestedRosterId }, new decimal[] { 1, 0 },
                new Dictionary<Guid, object>(),
                new Dictionary<Guid, string>() { { nestedRosterId, "a" } });
            AddInterviewLevel(interview, new ValueVector<Guid> { parentRosterId, nestedRosterId }, new decimal[] { 1, 1 },
                new Dictionary<Guid, object>(),
                new Dictionary<Guid, string>() { { nestedRosterId, "b" } });
           
            user = Mock.Of<UserDocument>();
            merger = CreateMerger(questionnaire);
        };

        Because of = () =>
            mergeResult = merger.Merge(interview, questionnaire, user.GetUseLight(), null, null);


        It should_title_of_question_in_first_row_of_first_roster_has_rostertitle_replaced_with_0_and_a = () =>
            GetQuestion(mergeResult, questionWithSubstitutionId, new decimal[] { 0, 0 }).Title.ShouldEqual("test 0 a");

        It should_title_of_question_in_second_row_of_first_roster_has_rostertitle_replaced_with_0_and_b = () =>
            GetQuestion(mergeResult, questionWithSubstitutionId, new decimal[] { 0, 1 }).Title.ShouldEqual("test 0 b");

        It should_title_of_question_in_first_row_of_second_roster_has_rostertitle_replaced_with_1_and_a = () =>
           GetQuestion(mergeResult, questionWithSubstitutionId, new decimal[] { 1, 0 }).Title.ShouldEqual("test 1 a");

        It should_title_of_question_in_second_row_of_second_roster_has_rostertitle_replaced_with_1_and_b = () =>
            GetQuestion(mergeResult, questionWithSubstitutionId, new decimal[] { 1, 1 }).Title.ShouldEqual("test 1 b");


        private static InterviewDataAndQuestionnaireMerger merger;
        private static InterviewDetailsView mergeResult;
        private static InterviewData interview;
        private static QuestionnaireDocument questionnaire;
        private static UserDocument user;

        private static Guid nestedRosterId;
        private static Guid questionWithSubstitutionId;
        private static Guid interviewId;
        private static Guid parentRosterId;
    }
}
