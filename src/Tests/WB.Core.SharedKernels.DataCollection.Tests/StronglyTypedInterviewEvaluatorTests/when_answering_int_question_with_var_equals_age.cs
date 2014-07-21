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
    internal class when_answering_int_question_with_var_equals_age : InterviewTestsContext
    {
        Establish context = () =>
        {
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFF1111111111");

            answer = 20;

            var questionnaire = Mock.Of<IQuestionnaire>
                (_
                    => _.HasQuestion(questionId) == true
                        && _.GetQuestionType(questionId) == QuestionType.Numeric
                        && _.IsQuestionInteger(questionId) == true
                        && _.GetRostersFromTopToSpecifiedQuestion(questionId) == new[] { StronglyTypedInterviewEvaluator.IdOf.hhMember }
                        && _.DoesQuestionSpecifyRosterTitle(questionId) == false
                );

            SetupInstanceToMockedServiceLocator<IQuestionnaireRepository>(
                CreateQuestionnaireRepositoryStubWithOneQuestionnaire(StronglyTypedInterviewEvaluator.IdOf.questionnaire, questionnaire));

            interview = CreateInterview(questionnaireId: StronglyTypedInterviewEvaluator.IdOf.questionnaire);
            interview.Apply(new TextQuestionAnswered(userId, StronglyTypedInterviewEvaluator.IdOf.id, emptyRosterVector, DateTime.Now, "Id"));
            interview.Apply(new NumericIntegerQuestionAnswered(userId, StronglyTypedInterviewEvaluator.IdOf.persons_count, emptyRosterVector, DateTime.Now, 1));
            interview.Apply(new RosterInstancesAdded(new[]
            {
                new AddedRosterInstance(StronglyTypedInterviewEvaluator.IdOf.hhMember, emptyRosterVector, 0.0m, sortIndex: null), 
                new AddedRosterInstance(StronglyTypedInterviewEvaluator.IdOf.jobActivity, emptyRosterVector, 0.0m, sortIndex: null)
            }));
            interview.Apply(new QuestionsDisabled(new Events.Interview.Dtos.Identity[]
            {
                new Events.Interview.Dtos.Identity(StronglyTypedInterviewEvaluator.IdOf.age, rosterVector),
                new Events.Interview.Dtos.Identity(StronglyTypedInterviewEvaluator.IdOf.person_id, rosterVector),
                new Events.Interview.Dtos.Identity(StronglyTypedInterviewEvaluator.IdOf.marital_status, rosterVector),
                new Events.Interview.Dtos.Identity(StronglyTypedInterviewEvaluator.IdOf.married_with, rosterVector),
                new Events.Interview.Dtos.Identity(StronglyTypedInterviewEvaluator.IdOf.food, rosterVector),
                new Events.Interview.Dtos.Identity(StronglyTypedInterviewEvaluator.IdOf.has_job, rosterVector),
                new Events.Interview.Dtos.Identity(StronglyTypedInterviewEvaluator.IdOf.job_title, rosterVector),
                new Events.Interview.Dtos.Identity(StronglyTypedInterviewEvaluator.IdOf.best_job_owner, rosterVector),
            }));
            interview.Apply(new AnswersDeclaredValid(new Events.Interview.Dtos.Identity[]
            {
                new Events.Interview.Dtos.Identity(StronglyTypedInterviewEvaluator.IdOf.name, rosterVector),
                new Events.Interview.Dtos.Identity(StronglyTypedInterviewEvaluator.IdOf.role, rosterVector),
            }));
            interview.Apply(new GroupsDisabled(new Events.Interview.Dtos.Identity[]
            {
                new Events.Interview.Dtos.Identity(StronglyTypedInterviewEvaluator.IdOf.groupId, rosterVector),
            }));
            interview.Apply(new TextQuestionAnswered(userId, StronglyTypedInterviewEvaluator.IdOf.name, rosterVector, DateTime.Now, "aaa"));
            interview.Apply(new QuestionsEnabled(new Events.Interview.Dtos.Identity[]
            {
                new Events.Interview.Dtos.Identity(StronglyTypedInterviewEvaluator.IdOf.age, rosterVector),
            }));
            interview.Apply(new AnswersDeclaredInvalid(new Events.Interview.Dtos.Identity[]
            {
                new Events.Interview.Dtos.Identity(StronglyTypedInterviewEvaluator.IdOf.age, rosterVector),
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

        It should_raise_NumericIntegerQuestionAnswered_event = () =>
            eventContext.ShouldContainEvent<NumericIntegerQuestionAnswered>();

        It should_enable_questions = () =>
            eventContext.GetEvent<QuestionsEnabled>().Questions.Select(q => q.Id)
                .ShouldContainOnly(StronglyTypedInterviewEvaluator.IdOf.has_job, StronglyTypedInterviewEvaluator.IdOf.person_id, StronglyTypedInterviewEvaluator.IdOf.marital_status);

        It should_enable_group = () =>
            eventContext.GetEvent<GroupsEnabled>().Groups.Select(q => q.Id)
                .ShouldContainOnly(StronglyTypedInterviewEvaluator.IdOf.groupId);

        It should_declare_valid_age_question = () =>
            eventContext.GetEvent<AnswersDeclaredValid>().Questions.Select(q => q.Id)
                .ShouldContainOnly(StronglyTypedInterviewEvaluator.IdOf.age);

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid questionId = StronglyTypedInterviewEvaluator.IdOf.age;
        private static int answer;
        private static decimal[] emptyRosterVector = new decimal[0];
        private static decimal[] rosterVector = new decimal[] { 0.0m };
    }
}