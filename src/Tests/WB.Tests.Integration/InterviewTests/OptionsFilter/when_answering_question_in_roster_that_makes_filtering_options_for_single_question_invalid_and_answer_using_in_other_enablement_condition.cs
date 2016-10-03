using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Tests.Integration.InterviewTests.OptionsFilter
{
    internal class when_answering_question_in_roster_that_makes_filtering_options_for_single_question_invalid_and_answer_using_in_other_enablement_condition : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();

                var options = new List<Answer>
                {
                    Create.Option(value: "1", text: "Option 1"),
                    Create.Option(value: "2", text: "Option 2"),
                    Create.Option(value: "3", text: "Option 3"),
                    Create.Option(value: "12", text: "Option 12"),
                };

                var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(questionnaireId, children: new IComposite[]
                {
                    Create.SingleQuestion(q1Id, variable: "q1", options: options),
                    Create.Roster(rosterId, rosterSizeSourceType: RosterSizeSourceType.FixedTitles, fixedRosterTitles: new []{ Create.FixedRosterTitle(1, "Hello")}, children: new IComposite[]
                    {
                        Create.SingleQuestion(q2Id, variable: "q2", options: options, optionsFilter: "@optioncode < (int)q1"),
                        Create.SingleQuestion(q3Id, variable: "q3", options: options, enablementCondition: "q2==2"),
                        Create.SingleQuestion(q4Id, variable: "q4", options: options, optionsFilter: "@optioncode % (int)q3 == 0"),
                        Create.SingleQuestion(q5Id, variable: "q5", options: options, enablementCondition: "q4 ==2"),
                    })
                });

                ILatestInterviewExpressionState interviewState = GetInterviewExpressionState(questionnaireDocument);

                var interview = SetupInterview(questionnaireDocument, precompiledState: interviewState);

                interview.AnswerSingleOptionQuestion(userId, q1Id, RosterVector.Empty, DateTime.Now, 12);
                interview.AnswerSingleOptionQuestion(userId, q2Id, Create.RosterVector(1), DateTime.Now, 2 );
                interview.AnswerSingleOptionQuestion(userId, q3Id, Create.RosterVector(1), DateTime.Now, 2);
                interview.AnswerSingleOptionQuestion(userId, q4Id, Create.RosterVector(1), DateTime.Now, 2);
                interview.AnswerSingleOptionQuestion(userId, q5Id, Create.RosterVector(1), DateTime.Now, 3);

                var result = new InvokeResults();

                using (var eventContext = new EventContext())
                {
                    interview.AnswerSingleOptionQuestion(userId, q1Id, RosterVector.Empty, DateTime.Now, 1);

                    result.QuestionsQ5Disabled = eventContext.AnyEvent<QuestionsDisabled>(x => x.Questions.Any(q => q.Id == q5Id));

                    result.QuestionqQ4HasEmptyAnswer = eventContext.GetEvents<AnswerRemoved>().Count(x => x.QuestionId == q4Id) == 1;
                    result.QuestionqQ2HasEmptyAnswer = eventContext.AnyEvent<AnswerRemoved>(x => x.QuestionId == q2Id);
                }

                return result;
            });

        It should_disable_q5 = () =>
            results.QuestionsQ5Disabled.ShouldBeTrue();

        It should_have_empty_answer_q2 = () =>
            results.QuestionqQ2HasEmptyAnswer.ShouldBeTrue();

        It should_have_empty_answer_q4 = () =>
            results.QuestionqQ4HasEmptyAnswer.ShouldBeTrue();

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        private static InvokeResults results;
        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        private static readonly Guid questionnaireId = Guid.Parse("99999999999999999999999999999999");
        private static readonly Guid rosterId = Guid.Parse("88888888888888888888888888888888");
        private static readonly Guid q1Id = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid q2Id = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid q3Id = Guid.Parse("33333333333333333333333333333333");
        private static readonly Guid q4Id = Guid.Parse("44444444444444444444444444444444");
        private static readonly Guid q5Id = Guid.Parse("55555555555555555555555555555555");
        private static readonly Guid userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

        [Serializable]
        internal class InvokeResults
        {
            public bool QuestionsQ5Disabled { get; set; }
            public bool QuestionqQ4HasEmptyAnswer { get; set; }
            public bool QuestionqQ2HasEmptyAnswer { get; set; }
        }
    }
}