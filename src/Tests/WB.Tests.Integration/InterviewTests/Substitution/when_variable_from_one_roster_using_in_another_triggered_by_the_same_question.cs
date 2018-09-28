using System;
using System.Linq;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.Substitution
{
    internal class when_variable_from_one_roster_using_in_another_triggered_by_the_same_question: InterviewTestsContext
    {
        private AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;

        [OneTimeSetUp]
        public void SetupTest()
        {
            appDomainContext = AppDomainContext.Create();
        }

        [Test]
        public void should_calculate_substitutions()
        {
            var results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var questionnaireId = Guid.Parse("77778888000000000000000000000000");
                var rosterSizeQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

                var roster1Id = Guid.Parse("11111111111111111111111111111111");
                var roster2Id = Guid.Parse("22222222222222222222222222222222");
                var roster3Id = Guid.Parse("33333333333333333333333333333333");

                var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Create.Entity.TextListQuestion(rosterSizeQuestionId, variable: "list"),

                    Create.Entity.ListRoster(
                        rosterId: roster1Id,
                        rosterSizeQuestionId: rosterSizeQuestionId,
                        variable: "first",
                        children: new IComposite[]
                        {
                            Create.Entity.TextQuestion(roster3Id, variable: "text", text: "Text: %s%")
                        }),
                    Create.Entity.ListRoster(
                        rosterId: roster2Id,
                        rosterSizeQuestionId: rosterSizeQuestionId,
                        variable: "second",
                        children: new IComposite[]
                        {
                            Create.Entity.Variable(variableName:"s", type: VariableType.String, expression: "list.Where(p=>p.Item1==@rowcode).First().Item2.ToUpper()")
                        })
                );

                var result = new InvokeResults();

                var interview = SetupStatefullInterview(questionnaireDocument);
               
                using (var eventContext = new EventContext())
                {
                    interview.AnswerTextListQuestion(Guid.NewGuid(), rosterSizeQuestionId, RosterVector.Empty, DateTime.Now, new[]
                    {
                        Tuple.Create(1m, "Vasya"),
                        Tuple.Create(2m, "Petya")
                    });

                    var questionsWithChangedSubstitutions = eventContext.GetSingleEvent<SubstitutionTitlesChanged>().Questions;

                    result.QuestionTitleWasChangedInRoster1 = questionsWithChangedSubstitutions.Contains(Create.Identity(roster3Id, 1));
                    result.QuestionTitleWasChangedInRoster2 = questionsWithChangedSubstitutions.Contains(Create.Identity(roster3Id, 2));
                }

                return result;
            });


            results.QuestionTitleWasChangedInRoster1.Should().BeTrue();
            results.QuestionTitleWasChangedInRoster2.Should().BeTrue();
        }

        [TearDown]
        public void TearDown()
        {
            appDomainContext.Dispose();
        }

        [Serializable]
        internal class InvokeResults
        {
            public bool QuestionTitleWasChangedInRoster1 { get; set; }
            public bool QuestionTitleWasChangedInRoster2 { get; set; }
        }
    }
}