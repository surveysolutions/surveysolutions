using System;
using System.Collections.Generic;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Tests.Integration.InterviewTests.LinkedQuestions
{
    internal class when_answering_question_that_enables_linked_to_roster_question_with_options_filter : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        public void BecauseOf() =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var options = new List<Answer>
                {
                    Abc.Create.Entity.Option("1"),
                    Abc.Create.Entity.Option("2"),
                    Abc.Create.Entity.Option("3"),
                    Abc.Create.Entity.Option("12")
                };

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId, children: new IComposite[]
                {
                    Abc.Create.Entity.MultyOptionsQuestion(q1Id, variable: "q1", options: options),
                    Abc.Create.Entity.Roster(rosterId, variable:"r1", rosterSizeQuestionId: q1Id, rosterSizeSourceType: RosterSizeSourceType.Question, children: new IComposite[]
                    {
                        Abc.Create.Entity.NumericIntegerQuestion(q2Id, variable: "age"),
                    }),
                    Abc.Create.Entity.Roster(roster1Id, variable:"r2", rosterSizeQuestionId: q1Id, rosterSizeSourceType: RosterSizeSourceType.Question, children: new IComposite[]
                    {
                        Abc.Create.Entity.NumericIntegerQuestion(q3Id, variable: "q3"),
                        Abc.Create.Entity.SingleQuestion(q4Id, variable: "q4", linkedToRosterId: rosterId, linkedFilter: "age != current.age", enablementCondition: "q3 == 1")
                    })
                });

                StatefulInterview interview = SetupStatefullInterview(questionnaireDocument);

                interview.AnswerMultipleOptionsQuestion(userId, q1Id, RosterVector.Empty, DateTime.Now, new[] { 1, 2 });
                interview.AnswerNumericIntegerQuestion(userId, q2Id, Abc.Create.Entity.RosterVector(new[] {1}), DateTime.Now, 20);
                interview.AnswerNumericIntegerQuestion(userId, q2Id, Abc.Create.Entity.RosterVector(new[] {2}), DateTime.Now, 15);
                interview.AnswerNumericIntegerQuestion(userId, q3Id, Abc.Create.Entity.RosterVector(new[] {2}), DateTime.Now, 1);

                return new InvokeResults
                {
                    OptionsCountForQuestion4InRoster1 = interview.GetLinkedSingleOptionQuestion(Identity.Create(q4Id, Abc.Create.Entity.RosterVector(new[] {1}))).Options.Count,
                    OptionsCountForQuestion4InRoster2 = interview.GetLinkedSingleOptionQuestion(Identity.Create(q4Id, Abc.Create.Entity.RosterVector(new[] {2}))).Options.Count

                };
            });

        [NUnit.Framework.Test] public void should_return_1_option_for_linked_question_in_1_roster () =>
            results.OptionsCountForQuestion4InRoster1.Should().Be(1);

        [NUnit.Framework.Test] public void should_return_1_option_for_linked_question_in_2_roster () =>
            results.OptionsCountForQuestion4InRoster2.Should().Be(1);

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

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
