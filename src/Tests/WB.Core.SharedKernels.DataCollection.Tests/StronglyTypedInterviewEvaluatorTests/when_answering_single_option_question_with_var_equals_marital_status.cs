using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Tests.InterviewTests;
using WB.Core.SharedKernels.ExpressionProcessing;
using Identity = WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos.Identity;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.StronglyTypedInterviewEvaluatorTests
{
    internal class when_answering_single_option_question_with_var_equals_marital_status : InterviewTestsContext
    {
        Establish context = () =>
        {
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFF1111111111");

            answer = 2m; // married

            var questionnaire = Mock.Of<IQuestionnaire>
                (_
                    => _.HasQuestion(questionId) == true
                        && _.GetQuestionType(questionId) == QuestionType.SingleOption
                        && _.GetRostersFromTopToSpecifiedQuestion(questionId) == new[] { IdOf.hhMember }
                        && _.DoesQuestionSpecifyRosterTitle(questionId) == false
                        && _.GetAnswerOptionsAsValues(questionId) == new[] { 1m, 2m, 3m }
                        && _.GetAnswerOptionTitle(questionId, 2m) == "Married"
                );

            SetupInstanceToMockedServiceLocator<IQuestionnaireRepository>(
                CreateQuestionnaireRepositoryStubWithOneQuestionnaire(IdOf.questionnaire, questionnaire));

            interview = CreateInterview(questionnaireId: IdOf.questionnaire);
            interview.Apply(new TextQuestionAnswered(userId, IdOf.id, emptyRosterVector, DateTime.Now, "Id"));
            interview.Apply(new NumericIntegerQuestionAnswered(userId, IdOf.persons_count, emptyRosterVector, DateTime.Now, 2));
            interview.Apply(new RosterInstancesAdded(new[]
            {
                new AddedRosterInstance(IdOf.hhMember, emptyRosterVector, 0.0m, sortIndex: null), 
                new AddedRosterInstance(IdOf.jobActivity, emptyRosterVector, 0.0m, sortIndex: null),
                new AddedRosterInstance(IdOf.hhMember, emptyRosterVector, 1.0m, sortIndex: null), 
                new AddedRosterInstance(IdOf.jobActivity, emptyRosterVector, 1.0m, sortIndex: null)
            }));
            interview.Apply(new QuestionsDisabled(new Identity[]
            {
                new Identity(IdOf.age, firstVector),
                new Identity(IdOf.person_id, firstVector),
                new Identity(IdOf.marital_status, firstVector),
                new Identity(IdOf.married_with, firstVector),
                new Identity(IdOf.food, firstVector),
                new Identity(IdOf.has_job, firstVector),
                new Identity(IdOf.job_title, firstVector),
                new Identity(IdOf.best_job_owner, firstVector),

                new Identity(IdOf.age, secondVector),
                new Identity(IdOf.person_id, secondVector),
                new Identity(IdOf.marital_status, secondVector),
                new Identity(IdOf.married_with, secondVector),
                new Identity(IdOf.food, secondVector),
                new Identity(IdOf.has_job, secondVector),
                new Identity(IdOf.job_title, secondVector),
                new Identity(IdOf.best_job_owner, secondVector),
            }));

            interview.Apply(new GroupsDisabled(new Identity[]
            {
                new Identity(IdOf.groupId, firstVector),
                new Identity(IdOf.groupId, secondVector),
            }));
            interview.Apply(new TextQuestionAnswered(userId, IdOf.name, firstVector, DateTime.Now, "aaa"));
            interview.Apply(new TextQuestionAnswered(userId, IdOf.name, secondVector, DateTime.Now, "att"));
            interview.Apply(new QuestionsEnabled(new Identity[]
            {
                new Identity(IdOf.age, firstVector),
                new Identity(IdOf.age, secondVector),
            }));
            interview.Apply(new AnswersDeclaredInvalid(new Identity[]
            {
                new Identity(IdOf.age, firstVector),
                new Identity(IdOf.age, secondVector),
            }));
            interview.Apply(new NumericIntegerQuestionAnswered(userId, IdOf.age, firstVector, DateTime.Now, 30));
            interview.Apply(new NumericIntegerQuestionAnswered(userId, IdOf.age, secondVector, DateTime.Now, 30));
            interview.Apply(new QuestionsEnabled(new Identity[]
            {
                new Identity(IdOf.has_job, firstVector),
                new Identity(IdOf.person_id, firstVector),
                new Identity(IdOf.marital_status, firstVector),

                new Identity(IdOf.has_job, secondVector),
                new Identity(IdOf.person_id, secondVector),
                new Identity(IdOf.marital_status, secondVector),
            }));
            interview.Apply(new GroupsEnabled(new Identity[]
            {
                new Identity(IdOf.groupId, firstVector),
                new Identity(IdOf.groupId, secondVector),
            }));
            eventContext = new EventContext();
        };

        Because of = () =>
            interview.AnswerSingleOptionQuestion(userId, questionId, firstVector, DateTime.Now, answer);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_SingleOptionQuestionAnswered_event = () =>
            eventContext.ShouldContainEvent<SingleOptionQuestionAnswered>();

        It should_enable_questions = () =>
            eventContext.GetEvent<QuestionsEnabled>().Questions.Select(q => q.Id)
                .ShouldContainOnly(IdOf.married_with);

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid questionId = IdOf.marital_status;
        private static decimal answer;
        private static decimal[] emptyRosterVector = new decimal[0];
        private static decimal[] firstVector = new decimal[] { 0.0m };
        private static decimal[] secondVector = new decimal[] { 1.0m };
    }
}