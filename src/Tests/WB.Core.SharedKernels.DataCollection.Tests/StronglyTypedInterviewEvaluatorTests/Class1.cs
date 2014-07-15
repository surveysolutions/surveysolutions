using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
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

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid questionId = IdOf.persons_count;
        private static int answer;
        private static decimal[] emptyRosterVector = new decimal[0];
        private static decimal[] rosterVector = new decimal[] { 0.0m };
    }

    internal class when_answering_text_question_with_var__equals_name : InterviewTestsContext
    {
        Establish context = () =>
        {
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFF1111111111");

            answer = "aaa";


            var questionnaire = Mock.Of<IQuestionnaire>
            (_
                => _.HasQuestion(questionId) == true
                && _.GetQuestionType(questionId) == QuestionType.Text
                && _.GetRostersFromTopToSpecifiedQuestion(questionId) == new[] { IdOf.hhMember }
                && _.DoesQuestionSpecifyRosterTitle(questionId) == true
                && _.GetRostersAffectedByRosterTitleQuestion(questionId) == new[] { IdOf.hhMember, IdOf.jobActivity }
            );

            SetupInstanceToMockedServiceLocator<IQuestionnaireRepository>(
                CreateQuestionnaireRepositoryStubWithOneQuestionnaire(IdOf.questionnaire, questionnaire));

            interview = CreateInterview(questionnaireId: IdOf.questionnaire);
            interview.Apply(new TextQuestionAnswered(userId, IdOf.id, emptyRosterVector, DateTime.Now, "Id"));
            interview.Apply(new NumericIntegerQuestionAnswered(userId, IdOf.persons_count, emptyRosterVector, DateTime.Now, 1));
            interview.Apply(new RosterInstancesAdded(new []
            {
                new AddedRosterInstance(IdOf.hhMember, emptyRosterVector, 0.0m, sortIndex: null), 
                new AddedRosterInstance(IdOf.jobActivity, emptyRosterVector, 0.0m, sortIndex: null)
            }));
            interview.Apply(new QuestionsDisabled(new Identity[]
            {
                new Identity(IdOf.age, rosterVector),
                new Identity(IdOf.groupId, rosterVector),
                new Identity(IdOf.groupId, rosterVector),
                new Identity(IdOf.person_id, rosterVector),
                new Identity(IdOf.marital_status, rosterVector),
                new Identity(IdOf.married_with, rosterVector),
                new Identity(IdOf.food, rosterVector),
                new Identity(IdOf.has_job, rosterVector),
                new Identity(IdOf.job_title, rosterVector),
                new Identity(IdOf.best_job_owner, rosterVector),
            }));

            eventContext = new EventContext();
        };

        Because of = () =>
            interview.AnswerTextQuestion(userId, questionId, rosterVector, DateTime.Now, answer);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_TextQuestionAnswered_event = () =>
            eventContext.ShouldContainEvent<TextQuestionAnswered>();

        It should_disable_age_question = () =>
            eventContext.GetEvent<QuestionsEnabled>().Questions.Select(q => q.Id)
                .ShouldContainOnly(IdOf.age);

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid questionId = IdOf.name;
        private static string answer;
        private static decimal[] emptyRosterVector = new decimal[0];
        private static decimal[] rosterVector = new decimal[]{ 0.0m };
    }
}
