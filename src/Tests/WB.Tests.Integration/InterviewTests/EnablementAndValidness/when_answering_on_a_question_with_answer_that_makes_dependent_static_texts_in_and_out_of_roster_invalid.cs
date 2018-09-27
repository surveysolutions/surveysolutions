using System;
using System.Linq;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_answering_on_a_question_with_answer_that_makes_dependent_static_texts_in_and_out_of_roster_invalid : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        protected static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;

        public void BecauseOf() => results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
        {
            SetUp.MockedServiceLocator();

            var answeredQuestionId = Guid.Parse("11111111111111111111111111111111");
            var dependentStaticTextOutsideRosterId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
            var dependentStaticTextInsideRosterId = Guid.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");

            var interview = SetupInterviewWithExpressionStorage(questionnaireDocument: Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Abc.Create.Entity.NumericIntegerQuestion(answeredQuestionId, "q1"),

                 Abc.Create.Entity.StaticText(dependentStaticTextOutsideRosterId,
                    validationConditions: Create.Entity.ValidationCondition(expression: "q1 != 0", message: null).ToEnumerable().ToList()),

                Abc.Create.Entity.Roster(fixedRosterTitles: new [] { IntegrationCreate.FixedTitle(1, "one") }, children: new IComposite[]
                {
                     Abc.Create.Entity.StaticText(dependentStaticTextInsideRosterId,
                        validationConditions: Create.Entity.ValidationCondition(expression: "q1 != 0", message: null).ToEnumerable().ToList()),
                })
            }));

            using (var eventContext = new EventContext())
            {
                interview.AnswerNumericIntegerQuestion(Abc.Create.Command.AnswerNumericIntegerQuestionCommand(questionId: answeredQuestionId, answer: 0));

                return new InvokeResults
                {
                    WasStaticTextsDeclaredInvalidEventPublishedForDependentStaticTextOutsideRoster = 
                        eventContext
                            .GetSingleEventOrNull<StaticTextsDeclaredInvalid>()?
                            .GetFailedValidationConditionsDictionary()
                            .ContainsKey(Abc.Create.Identity(dependentStaticTextOutsideRosterId))
                        ?? false,

                    WasStaticTextsDeclaredInvalidEventPublishedForDependentStaticTextInsideRoster = 
                        eventContext
                            .GetSingleEventOrNull<StaticTextsDeclaredInvalid>()?
                            .GetFailedValidationConditionsDictionary()
                            .ContainsKey(Abc.Create.Identity(dependentStaticTextInsideRosterId, Abc.Create.Entity.RosterVector(new[] {1})))
                        ?? false,
                };
            }
        });

        [NUnit.Framework.Test] public void should_mark_dependent_static_text_outside_roster_as_invalid () =>
            results.WasStaticTextsDeclaredInvalidEventPublishedForDependentStaticTextOutsideRoster.Should().BeTrue();

        [NUnit.Framework.Test] public void should_mark_dependent_static_text_inside_roster_as_invalid () =>
            results.WasStaticTextsDeclaredInvalidEventPublishedForDependentStaticTextInsideRoster.Should().BeTrue();

        private static InvokeResults results;

        [Serializable]
        internal class InvokeResults
        {
            public bool WasStaticTextsDeclaredInvalidEventPublishedForDependentStaticTextOutsideRoster { get; set; }
            public bool WasStaticTextsDeclaredInvalidEventPublishedForDependentStaticTextInsideRoster { get; set; }
        }
    }
}
