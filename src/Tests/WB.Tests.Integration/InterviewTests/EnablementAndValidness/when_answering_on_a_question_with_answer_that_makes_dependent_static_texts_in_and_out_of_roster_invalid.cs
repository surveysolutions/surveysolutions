using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_answering_on_a_question_with_answer_that_makes_dependent_static_texts_in_and_out_of_roster_invalid : in_standalone_app_domain
    {
        Because of = () => results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
        {
            Setup.MockedServiceLocator();

            var answeredQuestionId = Guid.Parse("11111111111111111111111111111111");
            var dependentStaticTextOutsideRosterId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
            var dependentStaticTextInsideRosterId = Guid.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");

            var interview = SetupInterview(questionnaireDocument: Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Abc.Create.Entity.NumericIntegerQuestion(answeredQuestionId, "q1"),

                 Abc.Create.Entity.StaticText(dependentStaticTextOutsideRosterId,
                    validationConditions: IntegrationCreate.ValidationCondition(expression: "q1 != 0").ToEnumerable().ToList()),

                Abc.Create.Entity.Roster(fixedRosterTitles: new [] { IntegrationCreate.FixedTitle(1, "one") }, children: new IComposite[]
                {
                     Abc.Create.Entity.StaticText(dependentStaticTextInsideRosterId,
                        validationConditions: IntegrationCreate.ValidationCondition(expression: "q1 != 0").ToEnumerable().ToList()),
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
                            .ContainsKey(IntegrationCreate.Identity(dependentStaticTextOutsideRosterId))
                        ?? false,

                    WasStaticTextsDeclaredInvalidEventPublishedForDependentStaticTextInsideRoster = 
                        eventContext
                            .GetSingleEventOrNull<StaticTextsDeclaredInvalid>()?
                            .GetFailedValidationConditionsDictionary()
                            .ContainsKey(IntegrationCreate.Identity(dependentStaticTextInsideRosterId, IntegrationCreate.RosterVector(1)))
                        ?? false,
                };
            }
        });

        It should_mark_dependent_static_text_outside_roster_as_invalid = () =>
            results.WasStaticTextsDeclaredInvalidEventPublishedForDependentStaticTextOutsideRoster.ShouldBeTrue();

        It should_mark_dependent_static_text_inside_roster_as_invalid = () =>
            results.WasStaticTextsDeclaredInvalidEventPublishedForDependentStaticTextInsideRoster.ShouldBeTrue();

        private static InvokeResults results;

        [Serializable]
        internal class InvokeResults
        {
            public bool WasStaticTextsDeclaredInvalidEventPublishedForDependentStaticTextOutsideRoster { get; set; }
            public bool WasStaticTextsDeclaredInvalidEventPublishedForDependentStaticTextInsideRoster { get; set; }
        }
    }
}