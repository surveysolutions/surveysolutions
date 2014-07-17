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
    internal class when_answering_single_option_question_with_var_equals_has_job : InterviewTestsContext
    {
        Establish context = () =>
        {
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFF1111111111");

            answer = 1;

            var questionnaire = Mock.Of<IQuestionnaire>
                (_
                    => _.HasQuestion(questionId) == true
                        && _.GetQuestionType(questionId) == QuestionType.SingleOption
                        && _.GetRostersFromTopToSpecifiedQuestion(questionId) == new[] { IdOf.hhMember }
                        && _.DoesQuestionSpecifyRosterTitle(questionId) == false
                        && _.GetAnswerOptionsAsValues(questionId) == new[] { 1m, 2m }
                        && _.GetAnswerOptionTitle(questionId, 1m) == "Yes"
                );

            SetupInstanceToMockedServiceLocator<IQuestionnaireRepository>(
                CreateQuestionnaireRepositoryStubWithOneQuestionnaire(IdOf.questionnaire, questionnaire));

            interview = CreateInterview(questionnaireId: IdOf.questionnaire);
            interview.Apply(new TextQuestionAnswered(userId, IdOf.id, emptyRosterVector, DateTime.Now, "Id"));
            interview.Apply(new NumericIntegerQuestionAnswered(userId, IdOf.persons_count, emptyRosterVector, DateTime.Now, 1));
            interview.Apply(new RosterInstancesAdded(new[]
            {
                new AddedRosterInstance(IdOf.hhMember, emptyRosterVector, 0.0m, sortIndex: null), 
                new AddedRosterInstance(IdOf.jobActivity, emptyRosterVector, 0.0m, sortIndex: null)
            }));
            interview.Apply(new QuestionsDisabled(new Identity[]
            {
                new Identity(IdOf.age, rosterVector),
                new Identity(IdOf.person_id, rosterVector),
                new Identity(IdOf.marital_status, rosterVector),
                new Identity(IdOf.married_with, rosterVector),
                new Identity(IdOf.food, rosterVector),
                new Identity(IdOf.has_job, rosterVector),
                new Identity(IdOf.job_title, rosterVector),
                new Identity(IdOf.best_job_owner, rosterVector),
            }));

            interview.Apply(new GroupsDisabled(new Identity[]
            {
                new Identity(IdOf.groupId, rosterVector),
            }));
            interview.Apply(new TextQuestionAnswered(userId, IdOf.name, rosterVector, DateTime.Now, "aaa"));
            interview.Apply(new QuestionsEnabled(new Identity[]
            {
                new Identity(IdOf.age, rosterVector),
            }));
            interview.Apply(new AnswersDeclaredInvalid(new Identity[]
            {
                new Identity(IdOf.age, rosterVector),
            }));
            interview.Apply(new NumericIntegerQuestionAnswered(userId, IdOf.age, rosterVector, DateTime.Now, 20));
            interview.Apply(new GroupsEnabled(new Identity[]
            {
                new Identity(IdOf.groupId, rosterVector),
            }));
            interview.Apply(new QuestionsEnabled(new Identity[]
            {
                new Identity(IdOf.has_job, rosterVector),
                new Identity(IdOf.person_id, rosterVector),
                new Identity(IdOf.marital_status, rosterVector),
            }));
            eventContext = new EventContext();
        };

        Because of = () =>
            interview.AnswerSingleOptionQuestion(userId, questionId, rosterVector, DateTime.Now, answer);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_SingleOptionQuestionAnswered_event = () =>
            eventContext.ShouldContainEvent<SingleOptionQuestionAnswered>();

        It should_enable_job_title_question = () =>
            eventContext.GetEvent<QuestionsEnabled>().Questions.Select(q => q.Id)
                .ShouldContainOnly(IdOf.job_title);


        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid questionId = IdOf.has_job;
        private static decimal answer;
        private static decimal[] emptyRosterVector = new decimal[0];
        private static decimal[] rosterVector = new decimal[] { 0.0m };
    }
}