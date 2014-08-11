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
                        && _.GetRosterGroupsByRosterSizeQuestion(questionId) == new[] { StronglyTypedInterviewEvaluator.IdOf.hhMember, StronglyTypedInterviewEvaluator.IdOf.jobActivity }
                        && _.HasGroup(StronglyTypedInterviewEvaluator.IdOf.hhMember) == true
                        && _.HasGroup(StronglyTypedInterviewEvaluator.IdOf.jobActivity) == true
                        && _.GetRosterLevelForGroup(StronglyTypedInterviewEvaluator.IdOf.hhMember) == 1
                        && _.GetRosterLevelForGroup(StronglyTypedInterviewEvaluator.IdOf.jobActivity) == 1
                        && _.GetRostersFromTopToSpecifiedGroup(StronglyTypedInterviewEvaluator.IdOf.hhMember) == new[] { StronglyTypedInterviewEvaluator.IdOf.persons_count }
                        && _.GetRostersFromTopToSpecifiedGroup(StronglyTypedInterviewEvaluator.IdOf.jobActivity) == new[] { StronglyTypedInterviewEvaluator.IdOf.persons_count }
                        && _.GetRostersFromTopToSpecifiedQuestion(questionId) == new Guid[0]
                        && _.GetRosterSizeQuestion(StronglyTypedInterviewEvaluator.IdOf.hhMember) == StronglyTypedInterviewEvaluator.IdOf.persons_count
                        && _.GetRosterSizeQuestion(StronglyTypedInterviewEvaluator.IdOf.jobActivity) == StronglyTypedInterviewEvaluator.IdOf.persons_count
                );

            SetupInstanceToMockedServiceLocator<IQuestionnaireRepository>(
                CreateQuestionnaireRepositoryStubWithOneQuestionnaire(StronglyTypedInterviewEvaluator.IdOf.questionnaire, questionnaire));

            SetupInstanceToMockedServiceLocator<IInterviewExpressionStateProvider>(CreateInterviewExpressionStateProviderStub());

            interview = CreateInterview(questionnaireId: StronglyTypedInterviewEvaluator.IdOf.questionnaire);
            interview.Apply(new TextQuestionAnswered(userId, StronglyTypedInterviewEvaluator.IdOf.id, emptyRosterVector, DateTime.Now, "Id"));
            
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
                    StronglyTypedInterviewEvaluator.IdOf.age,StronglyTypedInterviewEvaluator.IdOf.person_id,
                    StronglyTypedInterviewEvaluator.IdOf.marital_status, StronglyTypedInterviewEvaluator.IdOf.married_with, StronglyTypedInterviewEvaluator.IdOf.food, StronglyTypedInterviewEvaluator.IdOf.has_job, StronglyTypedInterviewEvaluator.IdOf.job_title, StronglyTypedInterviewEvaluator.IdOf.best_job_owner);

        It should_disable_group = () =>
            eventContext.GetEvent<GroupsDisabled>().Groups.Select(q => q.Id)
                .ShouldContainOnly(StronglyTypedInterviewEvaluator.IdOf.groupId);

        It should_declare_answers_as_invalid = () =>
            eventContext.GetEvent<AnswersDeclaredInvalid>().Questions.Select(q => q.Id)
                .ShouldContainOnly(StronglyTypedInterviewEvaluator.IdOf.name);

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid questionId = StronglyTypedInterviewEvaluator.IdOf.persons_count;
        private static int answer;
        private static decimal[] emptyRosterVector = new decimal[0];
        private static decimal[] rosterVector = new decimal[] { 0.0m };
    }
}