using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Tests.InterviewTests;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.StronglyTypedInterviewEvaluatorTests
{
    internal class when_answering_text_question_with_var_equals_persons_count : InterviewTestsContext
    {
        Establish context = () =>
        {
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFF1111111111");

            answer = 1 ;


            var questionnaire = Mock.Of<IQuestionnaire>
                (_
                    => _.HasQuestion(questionId) == true
                        && _.GetQuestionType(questionId) == QuestionType.Numeric
                        && _.IsQuestionInteger(questionId) == true
                        && _.GetRosterGroupsByRosterSizeQuestion(questionId) == new[] { InterviewTests.StronglyTypedInterviewEvaluator.IdOf.hhMember, InterviewTests.StronglyTypedInterviewEvaluator.IdOf.jobActivity }
                        && _.HasGroup(InterviewTests.StronglyTypedInterviewEvaluator.IdOf.hhMember) == true
                        && _.HasGroup(InterviewTests.StronglyTypedInterviewEvaluator.IdOf.jobActivity) == true
                        && _.GetRosterLevelForGroup(InterviewTests.StronglyTypedInterviewEvaluator.IdOf.hhMember) == 1
                        && _.GetRosterLevelForGroup(InterviewTests.StronglyTypedInterviewEvaluator.IdOf.jobActivity) == 1
                        && _.GetRostersFromTopToSpecifiedGroup(InterviewTests.StronglyTypedInterviewEvaluator.IdOf.hhMember) == new[] { InterviewTests.StronglyTypedInterviewEvaluator.IdOf.persons_count }
                        && _.GetRostersFromTopToSpecifiedGroup(InterviewTests.StronglyTypedInterviewEvaluator.IdOf.jobActivity) == new[] { InterviewTests.StronglyTypedInterviewEvaluator.IdOf.persons_count }
                        && _.GetRostersFromTopToSpecifiedQuestion(questionId) == new Guid[0]
                        && _.GetRosterSizeQuestion(InterviewTests.StronglyTypedInterviewEvaluator.IdOf.hhMember) == InterviewTests.StronglyTypedInterviewEvaluator.IdOf.persons_count
                        && _.GetRosterSizeQuestion(InterviewTests.StronglyTypedInterviewEvaluator.IdOf.jobActivity) == InterviewTests.StronglyTypedInterviewEvaluator.IdOf.persons_count
                );

            SetupInstanceToMockedServiceLocator<IQuestionnaireRepository>(
                CreateQuestionnaireRepositoryStubWithOneQuestionnaire(InterviewTests.StronglyTypedInterviewEvaluator.IdOf.questionnaire, questionnaire));

            SetupInstanceToMockedServiceLocator<IInterviewExpressionStateProvider>(CreateInterviewExpressionStateProviderStub());

            interview = CreateInterview(questionnaireId: InterviewTests.StronglyTypedInterviewEvaluator.IdOf.questionnaire);
            interview.Apply(new TextQuestionAnswered(userId, InterviewTests.StronglyTypedInterviewEvaluator.IdOf.id, emptyRosterVector, DateTime.Now, "Id"));
            
            eventContext = new EventContext();
        };

        Because of = () =>
            interview.AnswerNumericIntegerQuestion(userId, questionId, emptyRosterVector, DateTime.Now, answer);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };
        
        It should_disable_some_questions = () =>
            eventContext.GetEvent<QuestionsDisabled>().Questions.Select(q => q.Id)
                .ShouldContainOnly(
                    InterviewTests.StronglyTypedInterviewEvaluator.IdOf.age,InterviewTests.StronglyTypedInterviewEvaluator.IdOf.person_id,
                    InterviewTests.StronglyTypedInterviewEvaluator.IdOf.marital_status, InterviewTests.StronglyTypedInterviewEvaluator.IdOf.married_with, InterviewTests.StronglyTypedInterviewEvaluator.IdOf.food, InterviewTests.StronglyTypedInterviewEvaluator.IdOf.has_job, InterviewTests.StronglyTypedInterviewEvaluator.IdOf.job_title, InterviewTests.StronglyTypedInterviewEvaluator.IdOf.best_job_owner);

        It should_disable_group = () =>
            eventContext.GetEvent<GroupsDisabled>().Groups.Select(q => q.Id)
                .ShouldContainOnly(InterviewTests.StronglyTypedInterviewEvaluator.IdOf.groupId);

        It should_declare_answers_as_invalid = () =>
            eventContext.GetEvent<AnswersDeclaredInvalid>().Questions.Select(q => q.Id)
                .ShouldContainOnly(InterviewTests.StronglyTypedInterviewEvaluator.IdOf.name);

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid questionId = InterviewTests.StronglyTypedInterviewEvaluator.IdOf.persons_count;
        private static int answer;
        private static decimal[] emptyRosterVector = new decimal[0];
        private static decimal[] rosterVector = new decimal[] { 0.0m };
    }
}