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
    internal class when_answering_int_question_with_var_equals_age_for_the_third_time : InterviewTestsContext
    {
        Establish context = () =>
        {
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFF1111111111");

            answer = 35;

            var questionnaire = Mock.Of<IQuestionnaire>
            (_
                 => _.HasQuestion(questionId) == true
                && _.GetQuestionType(questionId) == QuestionType.Numeric
                && _.IsQuestionInteger(questionId) == true
                && _.GetRostersFromTopToSpecifiedQuestion(questionId) == new[] { InterviewTests.StronglyTypedInterviewEvaluator.IdOf.hhMember }
                && _.DoesQuestionSpecifyRosterTitle(questionId) == false
            );

            SetupInstanceToMockedServiceLocator<IQuestionnaireRepository>(
                CreateQuestionnaireRepositoryStubWithOneQuestionnaire(InterviewTests.StronglyTypedInterviewEvaluator.IdOf.questionnaire, questionnaire));

            SetupInstanceToMockedServiceLocator<IInterviewExpressionStateProvider>(CreateInterviewExpressionStateProviderStub());

            interview = CreateInterview(questionnaireId: InterviewTests.StronglyTypedInterviewEvaluator.IdOf.questionnaire);
            interview.Apply(new TextQuestionAnswered(userId, InterviewTests.StronglyTypedInterviewEvaluator.IdOf.id, emptyRosterVector, DateTime.Now, "Id"));
            interview.Apply(new NumericIntegerQuestionAnswered(userId, InterviewTests.StronglyTypedInterviewEvaluator.IdOf.persons_count, emptyRosterVector, DateTime.Now, 1));
            interview.Apply(new RosterInstancesAdded(new[]
            {
                new AddedRosterInstance(InterviewTests.StronglyTypedInterviewEvaluator.IdOf.hhMember, emptyRosterVector, 0.0m, sortIndex: null), 
                new AddedRosterInstance(InterviewTests.StronglyTypedInterviewEvaluator.IdOf.jobActivity, emptyRosterVector, 0.0m, sortIndex: null)
            }));
            interview.Apply(new QuestionsDisabled(new WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos.Identity[]
            {
                new WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos.Identity(InterviewTests.StronglyTypedInterviewEvaluator.IdOf.age, rosterVector),
                new WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos.Identity(InterviewTests.StronglyTypedInterviewEvaluator.IdOf.person_id, rosterVector),
                new WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos.Identity(InterviewTests.StronglyTypedInterviewEvaluator.IdOf.marital_status, rosterVector),
                new WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos.Identity(InterviewTests.StronglyTypedInterviewEvaluator.IdOf.married_with, rosterVector),
                new WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos.Identity(InterviewTests.StronglyTypedInterviewEvaluator.IdOf.food, rosterVector),
                new WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos.Identity(InterviewTests.StronglyTypedInterviewEvaluator.IdOf.has_job, rosterVector),
                new WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos.Identity(InterviewTests.StronglyTypedInterviewEvaluator.IdOf.job_title, rosterVector),
                new WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos.Identity(InterviewTests.StronglyTypedInterviewEvaluator.IdOf.best_job_owner, rosterVector),
            }));

            interview.Apply(new GroupsDisabled(new WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos.Identity[]
            {
                new WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos.Identity(InterviewTests.StronglyTypedInterviewEvaluator.IdOf.groupId, rosterVector),
            }));
            interview.Apply(new TextQuestionAnswered(userId, InterviewTests.StronglyTypedInterviewEvaluator.IdOf.name, rosterVector, DateTime.Now, "aaa"));
            interview.Apply(new QuestionsEnabled(new WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos.Identity[]
            {
                new WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos.Identity(InterviewTests.StronglyTypedInterviewEvaluator.IdOf.age, rosterVector),
            }));
            interview.Apply(new AnswersDeclaredInvalid(new WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos.Identity[]
            {
                new WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos.Identity(InterviewTests.StronglyTypedInterviewEvaluator.IdOf.age, rosterVector),
            }));
            interview.Apply(new NumericIntegerQuestionAnswered(userId, InterviewTests.StronglyTypedInterviewEvaluator.IdOf.age, rosterVector, DateTime.Now, 20));
            interview.Apply(new GroupsEnabled(new WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos.Identity[]
            {
                new WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos.Identity(InterviewTests.StronglyTypedInterviewEvaluator.IdOf.groupId, rosterVector),
            }));
            interview.Apply(new QuestionsEnabled(new WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos.Identity[]
            {
                new WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos.Identity(InterviewTests.StronglyTypedInterviewEvaluator.IdOf.has_job, rosterVector),
                new WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos.Identity(InterviewTests.StronglyTypedInterviewEvaluator.IdOf.person_id, rosterVector),
                new WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos.Identity(InterviewTests.StronglyTypedInterviewEvaluator.IdOf.marital_status, rosterVector),
            }));
            interview.Apply(new SingleOptionQuestionAnswered(userId, InterviewTests.StronglyTypedInterviewEvaluator.IdOf.has_job, rosterVector, DateTime.Now, 1m));
            interview.Apply(new QuestionsEnabled(new WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos.Identity[]
            {
                new WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos.Identity(InterviewTests.StronglyTypedInterviewEvaluator.IdOf.job_title, rosterVector),
            }));
            interview.Apply(new NumericIntegerQuestionAnswered(userId, InterviewTests.StronglyTypedInterviewEvaluator.IdOf.age, rosterVector, DateTime.Now, 5));
            interview.Apply(new QuestionsDisabled(new WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos.Identity[]
            {
                new WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos.Identity(InterviewTests.StronglyTypedInterviewEvaluator.IdOf.person_id, rosterVector),
                new WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos.Identity(InterviewTests.StronglyTypedInterviewEvaluator.IdOf.marital_status, rosterVector),
                new WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos.Identity(InterviewTests.StronglyTypedInterviewEvaluator.IdOf.has_job, rosterVector),
                new WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos.Identity(InterviewTests.StronglyTypedInterviewEvaluator.IdOf.job_title, rosterVector),
            }));

            interview.Apply(new GroupsDisabled(new WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos.Identity[]
            {
                new WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos.Identity(InterviewTests.StronglyTypedInterviewEvaluator.IdOf.groupId, rosterVector),
            }));
            eventContext = new EventContext();
        };

        Because of = () =>
            interview.AnswerNumericIntegerQuestion(userId, questionId, rosterVector, DateTime.Now, answer);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_SingleOptionQuestionAnswered_event = () =>
            eventContext.ShouldContainEvent<NumericIntegerQuestionAnswered>();

        It should_disable_questions = () =>
            eventContext.GetEvent<QuestionsEnabled>().Questions.Select(q => q.Id)
                .ShouldContainOnly(InterviewTests.StronglyTypedInterviewEvaluator.IdOf.person_id, InterviewTests.StronglyTypedInterviewEvaluator.IdOf.marital_status, InterviewTests.StronglyTypedInterviewEvaluator.IdOf.has_job, InterviewTests.StronglyTypedInterviewEvaluator.IdOf.job_title);

        It should_disable_group = () =>
            eventContext.GetEvent<GroupsEnabled>().Groups.Select(q => q.Id)
                .ShouldContainOnly(InterviewTests.StronglyTypedInterviewEvaluator.IdOf.groupId);

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid questionId = InterviewTests.StronglyTypedInterviewEvaluator.IdOf.age;
        private static int answer;
        private static decimal[] emptyRosterVector = new decimal[0];
        private static decimal[] rosterVector = new decimal[] { 0.0m };
    }
}
