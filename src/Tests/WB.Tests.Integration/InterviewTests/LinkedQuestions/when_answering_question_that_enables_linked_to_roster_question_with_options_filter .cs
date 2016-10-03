using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;

namespace WB.Tests.Integration.InterviewTests.LinkedQuestions
{
    internal class when_answering_question_that_enables_linked_to_roster_question_with_options_filter : InterviewTestsContext
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
                    Create.Option("1"), Create.Option("2"), Create.Option("3"), Create.Option("12")
                };

                var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(questionnaireId, children: new IComposite[]
                {
                    Create.MultyOptionsQuestion(q1Id, variable: "q1", options: options),
                    Create.Roster(rosterId, variable:"r1", rosterSizeQuestionId: q1Id, rosterSizeSourceType: RosterSizeSourceType.Question, children: new IComposite[]
                    {
                        Create.NumericIntegerQuestion(q2Id, variable: "age"),
                    }),
                    Create.Roster(roster1Id, variable:"r2", rosterSizeQuestionId: q1Id, rosterSizeSourceType: RosterSizeSourceType.Question, children: new IComposite[]
                    {
                        Create.NumericIntegerQuestion(q3Id, variable: "q3"),
                        Create.SingleQuestion(q4Id, variable: "q4", linkedToRosterId: rosterId, linkedFilter: "age != current.age", enablementCondition: "q3 == 1")
                    })
                });

                StatefulInterview interview = SetupStatefullInterview(questionnaireDocument);

                interview.AnswerMultipleOptionsQuestion(userId, q1Id, RosterVector.Empty, DateTime.Now, new[] { 1m, 2m });
                interview.AnswerNumericIntegerQuestion(userId, q2Id, Create.RosterVector(1), DateTime.Now, 20);
                interview.AnswerNumericIntegerQuestion(userId, q2Id, Create.RosterVector(2), DateTime.Now, 15);
                //interview.AnswerNumericIntegerQuestion(userId, q3Id, Create.RosterVector(1), DateTime.Now, 1);
                var result = new InvokeResults();

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(userId, q3Id, Create.RosterVector(2), DateTime.Now, 1);

                    result.OptionsCountForQuestion4InRoster1 = interview.FindReferencedRostersForLinkedQuestion(rosterId,
                        Create.Identity(q4Id, Create.RosterVector(1))).Count();
                    result.OptionsCountForQuestion4InRoster2 = interview.FindReferencedRostersForLinkedQuestion(rosterId,
                        Create.Identity(q4Id, Create.RosterVector(2))).Count();
                }

                return result;
            });

        It should_return_1_option_for_linked_question_in_1_roster = () =>
            results.OptionsCountForQuestion4InRoster1.ShouldEqual(1);

        It should_return_1_option_for_linked_question_in_2_roster = () =>
            results.OptionsCountForQuestion4InRoster2.ShouldEqual(1);

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        private static InvokeResults results;
        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        private static readonly Guid questionnaireId = Guid.Parse("99999999999999999999999999999999");
        private static readonly Guid rosterId = Guid.Parse("88888888888888888888888888888888");
        private static readonly Guid roster1Id = Guid.Parse("77777777777777777777777777777777");
        private static readonly Guid q1Id = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid q2Id = Guid.Parse("33333333333333333333333333333333");
        private static readonly Guid q4Id = Guid.Parse("44444444444444444444444444444444");
        private static readonly Guid q3Id = Guid.Parse("55555555555555555555555555555555");
        private static readonly Guid userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

        [Serializable]
        internal class InvokeResults
        {
            public int OptionsCountForQuestion4InRoster1 { get; set; }
            public int OptionsCountForQuestion4InRoster2 { get; set; }
        }
    }
}