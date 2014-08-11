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
using Identity = WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos.Identity;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.StronglyTypedInterviewEvaluatorTests
{
    [Ignore("C#")]
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
                        && _.GetRostersFromTopToSpecifiedQuestion(questionId) == new[] { StronglyTypedInterviewEvaluator.IdOf.hhMember }
                        && _.DoesQuestionSpecifyRosterTitle(questionId) == false
                        && _.GetAnswerOptionsAsValues(questionId) == new[] { 1m, 2m, 3m }
                        && _.GetAnswerOptionTitle(questionId, 2m) == "Married"
                );

            SetupInstanceToMockedServiceLocator<IQuestionnaireRepository>(
                CreateQuestionnaireRepositoryStubWithOneQuestionnaire(StronglyTypedInterviewEvaluator.IdOf.questionnaire, questionnaire));

            SetupInstanceToMockedServiceLocator<IInterviewExpressionStateProvider>(CreateInterviewExpressionStateProviderStub());

            interview = CreateInterview(questionnaireId: StronglyTypedInterviewEvaluator.IdOf.questionnaire);
            interview.Apply(new TextQuestionAnswered(userId, StronglyTypedInterviewEvaluator.IdOf.id, emptyRosterVector, DateTime.Now, "Id"));
            interview.Apply(new NumericIntegerQuestionAnswered(userId, StronglyTypedInterviewEvaluator.IdOf.persons_count, emptyRosterVector, DateTime.Now, 2));
            interview.Apply(new RosterInstancesAdded(new[]
            {
                new AddedRosterInstance(StronglyTypedInterviewEvaluator.IdOf.hhMember, emptyRosterVector, 0.0m, sortIndex: null), 
                new AddedRosterInstance(StronglyTypedInterviewEvaluator.IdOf.jobActivity, emptyRosterVector, 0.0m, sortIndex: null),
                new AddedRosterInstance(StronglyTypedInterviewEvaluator.IdOf.hhMember, emptyRosterVector, 1.0m, sortIndex: null), 
                new AddedRosterInstance(StronglyTypedInterviewEvaluator.IdOf.jobActivity, emptyRosterVector, 1.0m, sortIndex: null)
            }));
            interview.Apply(new QuestionsDisabled(new Events.Interview.Dtos.Identity[]
            {
                new Events.Interview.Dtos.Identity(StronglyTypedInterviewEvaluator.IdOf.age, firstVector),
                new Events.Interview.Dtos.Identity(StronglyTypedInterviewEvaluator.IdOf.person_id, firstVector),
                new Events.Interview.Dtos.Identity(StronglyTypedInterviewEvaluator.IdOf.marital_status, firstVector),
                new Events.Interview.Dtos.Identity(StronglyTypedInterviewEvaluator.IdOf.married_with, firstVector),
                new Events.Interview.Dtos.Identity(StronglyTypedInterviewEvaluator.IdOf.food, firstVector),
                new Events.Interview.Dtos.Identity(StronglyTypedInterviewEvaluator.IdOf.has_job, firstVector),
                new Events.Interview.Dtos.Identity(StronglyTypedInterviewEvaluator.IdOf.job_title, firstVector),
                new Events.Interview.Dtos.Identity(StronglyTypedInterviewEvaluator.IdOf.best_job_owner, firstVector),

                new Events.Interview.Dtos.Identity(StronglyTypedInterviewEvaluator.IdOf.age, secondVector),
                new Events.Interview.Dtos.Identity(StronglyTypedInterviewEvaluator.IdOf.person_id, secondVector),
                new Events.Interview.Dtos.Identity(StronglyTypedInterviewEvaluator.IdOf.marital_status, secondVector),
                new Events.Interview.Dtos.Identity(StronglyTypedInterviewEvaluator.IdOf.married_with, secondVector),
                new Events.Interview.Dtos.Identity(StronglyTypedInterviewEvaluator.IdOf.food, secondVector),
                new Events.Interview.Dtos.Identity(StronglyTypedInterviewEvaluator.IdOf.has_job, secondVector),
                new Events.Interview.Dtos.Identity(StronglyTypedInterviewEvaluator.IdOf.job_title, secondVector),
                new Events.Interview.Dtos.Identity(StronglyTypedInterviewEvaluator.IdOf.best_job_owner, secondVector),
            }));

            interview.Apply(new GroupsDisabled(new Events.Interview.Dtos.Identity[]
            {
                new Events.Interview.Dtos.Identity(StronglyTypedInterviewEvaluator.IdOf.groupId, firstVector),
                new Events.Interview.Dtos.Identity(StronglyTypedInterviewEvaluator.IdOf.groupId, secondVector),
            }));
            interview.Apply(new TextQuestionAnswered(userId, StronglyTypedInterviewEvaluator.IdOf.name, firstVector, DateTime.Now, "aaa"));
            interview.Apply(new TextQuestionAnswered(userId, StronglyTypedInterviewEvaluator.IdOf.name, secondVector, DateTime.Now, "att"));
            interview.Apply(new QuestionsEnabled(new Events.Interview.Dtos.Identity[]
            {
                new Events.Interview.Dtos.Identity(StronglyTypedInterviewEvaluator.IdOf.age, firstVector),
                new Events.Interview.Dtos.Identity(StronglyTypedInterviewEvaluator.IdOf.age, secondVector),
            }));
            interview.Apply(new AnswersDeclaredInvalid(new Events.Interview.Dtos.Identity[]
            {
                new Events.Interview.Dtos.Identity(StronglyTypedInterviewEvaluator.IdOf.age, firstVector),
                new Events.Interview.Dtos.Identity(StronglyTypedInterviewEvaluator.IdOf.age, secondVector),
            }));
            interview.Apply(new NumericIntegerQuestionAnswered(userId, StronglyTypedInterviewEvaluator.IdOf.age, firstVector, DateTime.Now, 30));
            interview.Apply(new NumericIntegerQuestionAnswered(userId, StronglyTypedInterviewEvaluator.IdOf.age, secondVector, DateTime.Now, 30));
            interview.Apply(new QuestionsEnabled(new Events.Interview.Dtos.Identity[]
            {
                new Events.Interview.Dtos.Identity(StronglyTypedInterviewEvaluator.IdOf.has_job, firstVector),
                new Events.Interview.Dtos.Identity(StronglyTypedInterviewEvaluator.IdOf.person_id, firstVector),
                new Events.Interview.Dtos.Identity(StronglyTypedInterviewEvaluator.IdOf.marital_status, firstVector),

                new Events.Interview.Dtos.Identity(StronglyTypedInterviewEvaluator.IdOf.has_job, secondVector),
                new Events.Interview.Dtos.Identity(StronglyTypedInterviewEvaluator.IdOf.person_id, secondVector),
                new Events.Interview.Dtos.Identity(StronglyTypedInterviewEvaluator.IdOf.marital_status, secondVector),
            }));
            interview.Apply(new GroupsEnabled(new Events.Interview.Dtos.Identity[]
            {
                new Events.Interview.Dtos.Identity(StronglyTypedInterviewEvaluator.IdOf.groupId, firstVector),
                new Events.Interview.Dtos.Identity(StronglyTypedInterviewEvaluator.IdOf.groupId, secondVector),
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
                .ShouldContainOnly(StronglyTypedInterviewEvaluator.IdOf.married_with);

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid questionId = StronglyTypedInterviewEvaluator.IdOf.marital_status;
        private static decimal answer;
        private static decimal[] emptyRosterVector = new decimal[0];
        private static decimal[] firstVector = new decimal[] { 0.0m };
        private static decimal[] secondVector = new decimal[] { 1.0m };
    }
}