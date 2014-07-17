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
using WB.Core.SharedKernels.ExpressionProcessing;
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
                        && _.GetRosterGroupsByRosterSizeQuestion(questionId) == new[] { IdOf.hhMember, IdOf.jobActivity }
                        && _.HasGroup(IdOf.hhMember) == true
                        && _.HasGroup(IdOf.jobActivity) == true
                        && _.GetRosterLevelForGroup(IdOf.hhMember) == 1
                        && _.GetRosterLevelForGroup(IdOf.jobActivity) == 1
                        && _.GetRostersFromTopToSpecifiedGroup(IdOf.hhMember) == new[] { IdOf.persons_count }
                        && _.GetRostersFromTopToSpecifiedGroup(IdOf.jobActivity) == new[] { IdOf.persons_count }
                        && _.GetRostersFromTopToSpecifiedQuestion(questionId) == new Guid[0]
                        && _.GetRosterSizeQuestion(IdOf.hhMember) == IdOf.persons_count
                        && _.GetRosterSizeQuestion(IdOf.jobActivity) == IdOf.persons_count
                );

            SetupInstanceToMockedServiceLocator<IQuestionnaireRepository>(
                CreateQuestionnaireRepositoryStubWithOneQuestionnaire(IdOf.questionnaire, questionnaire));

            interview = CreateInterview(questionnaireId: IdOf.questionnaire);
            interview.Apply(new TextQuestionAnswered(userId, IdOf.id, emptyRosterVector, DateTime.Now, "Id"));
            
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
                    IdOf.age,IdOf.person_id,
                    IdOf.marital_status, IdOf.married_with, IdOf.food, IdOf.has_job, IdOf.job_title, IdOf.best_job_owner);

        It should_disable_group = () =>
            eventContext.GetEvent<GroupsDisabled>().Groups.Select(q => q.Id)
                .ShouldContainOnly(IdOf.groupId);

        It should_declare_answers_as_invalid = () =>
            eventContext.GetEvent<AnswersDeclaredInvalid>().Questions.Select(q => q.Id)
                .ShouldContainOnly(IdOf.name);

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid questionId = IdOf.persons_count;
        private static int answer;
        private static decimal[] emptyRosterVector = new decimal[0];
        private static decimal[] rosterVector = new decimal[] { 0.0m };
    }
}