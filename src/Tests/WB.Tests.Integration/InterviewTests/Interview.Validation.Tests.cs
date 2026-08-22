using System;
using System.Collections.Generic;
using System.Linq;

using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests
{
    internal class InterviewValidationTests : InterviewTestsContext
    {
        [Test]
        public void When_answering_on_question_with_condition_on_warnings_Then_warning_should_be_raised()
        {
            var interviewerId = Guid.Parse("11111111111111111111111111111111");
            var intQuestionId = Guid.Parse("22222222222222222222222222222222");

            var appDomainContext = AppDomainContext.Create();

            var results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.NumericIntegerQuestion(intQuestionId, variable: "i", validationConditions: new[]
                    {
                        Create.Entity.ValidationCondition("i > 5", "warning", ValidationSeverity.Warning),
                        Create.Entity.ValidationCondition("i < 5", "warning", ValidationSeverity.Warning),
                    })
                );
                var interview = SetupInterview(appDomainContext.AssemblyLoadContext, questionnaire);

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(interviewerId, intQuestionId, RosterVector.Empty, DateTime.UtcNow, 7);

                    return new
                    {
                        AnswersDeclaredPlausibleEvent = GetFirstEventByType<AnswersDeclaredPlausible>(eventContext.Events),
                        AnswersDeclaredValidEvent = GetFirstEventByType<AnswersDeclaredValid>(eventContext.Events),
                        AnswersDeclaredImplausibleEvent = GetFirstEventByType<AnswersDeclaredImplausible>(eventContext.Events),
                        AnswersDeclaredInvalidEvent = GetFirstEventByType<AnswersDeclaredInvalid>(eventContext.Events)
                    };
                }
            });

            Assert.That(results, Is.Not.Null);
            Assert.That(results.AnswersDeclaredImplausibleEvent, Is.Not.Null);
            Assert.That(results.AnswersDeclaredImplausibleEvent.FailedValidationConditions.Count, Is.EqualTo(1));
            Assert.That(results.AnswersDeclaredImplausibleEvent.FailedValidationConditions.Select(e => e.Key.Id).ToArray(), Is.EquivalentTo(new[] { intQuestionId }));
            Assert.That(results.AnswersDeclaredImplausibleEvent.FailedValidationConditions.Select(e => e.Value.Single()).Select(v => v.FailedConditionIndex).ToArray(), Is.EquivalentTo(new[] { 1 }));

            Assert.That(results.AnswersDeclaredPlausibleEvent, Is.Null);
            Assert.That(results.AnswersDeclaredValidEvent, Is.Null);
            Assert.That(results.AnswersDeclaredInvalidEvent, Is.Null);

            appDomainContext.Dispose();
            appDomainContext = null;
        }


        [Test]
        public void When_answering_on_question_and_exists_static_text_with_condition_on_warnings_Then_warning_should_be_raised()
        {
            var interviewerId = Guid.Parse("11111111111111111111111111111111");
            var intQuestionId = Guid.Parse("22222222222222222222222222222222");
            var staticTextId = Guid.Parse("33333333333333333333333333333333");

            var appDomainContext = AppDomainContext.Create();

            var results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.NumericIntegerQuestion(intQuestionId, variable: "i"),
                    Create.Entity.StaticText(staticTextId, validationConditions: new List<ValidationCondition>
                    {
                        Create.Entity.ValidationCondition("i >= 5", "warning", ValidationSeverity.Warning),
                        Create.Entity.ValidationCondition("i <= 5", "warning", ValidationSeverity.Warning),
                    })
                );
                var interview = SetupInterview(appDomainContext.AssemblyLoadContext, questionnaire);
                interview.AnswerNumericIntegerQuestion(interviewerId, intQuestionId, RosterVector.Empty, DateTime.UtcNow, 5);

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(interviewerId, intQuestionId, RosterVector.Empty, DateTime.UtcNow, 7);

                    return new
                    {
                        StaticTextsDeclaredPlausibleEvent = GetFirstEventByType<StaticTextsDeclaredPlausible>(eventContext.Events),
                        StaticTextsDeclaredValidEvent = GetFirstEventByType<StaticTextsDeclaredValid>(eventContext.Events),
                        StaticTextsDeclaredImplausibleEvent = GetFirstEventByType<StaticTextsDeclaredImplausible>(eventContext.Events),
                        StaticTextsDeclaredInvalidEvent = GetFirstEventByType<StaticTextsDeclaredInvalid>(eventContext.Events)
                    };
                }
            });

            Assert.That(results, Is.Not.Null);
            Assert.That(results.StaticTextsDeclaredImplausibleEvent, Is.Not.Null);
            Assert.That(results.StaticTextsDeclaredImplausibleEvent.FailedValidationConditions.Count, Is.EqualTo(1));
            Assert.That(results.StaticTextsDeclaredImplausibleEvent.FailedValidationConditions.Select(e => e.Key.Id).ToArray(), Is.EquivalentTo(new[] { staticTextId }));
            Assert.That(results.StaticTextsDeclaredImplausibleEvent.FailedValidationConditions.Select(e => e.Value.Single()).Select(v => v.FailedConditionIndex).ToArray(), Is.EquivalentTo(new[] { 1 }));

            Assert.That(results.StaticTextsDeclaredPlausibleEvent, Is.Null);
            Assert.That(results.StaticTextsDeclaredValidEvent, Is.Null);
            Assert.That(results.StaticTextsDeclaredInvalidEvent, Is.Null);

            appDomainContext.Dispose();
            appDomainContext = null;
        }

        [Test]
        public void When_correct_answering_on_question_with_condition_on_warnings_Then_warning_should_be_raised_to_reset()
        {
            var interviewerId = Guid.Parse("11111111111111111111111111111111");
            var intQuestionId = Guid.Parse("22222222222222222222222222222222");

            var appDomainContext = AppDomainContext.Create();

            var results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.NumericIntegerQuestion(intQuestionId, variable: "i", validationConditions: new[]
                    {
                        Create.Entity.ValidationCondition("i >= 5", "warning", ValidationSeverity.Warning),
                        Create.Entity.ValidationCondition("i <= 5", "warning", ValidationSeverity.Warning),
                    })
                );
                var interview = SetupInterview(appDomainContext.AssemblyLoadContext, questionnaire);
                interview.AnswerNumericIntegerQuestion(interviewerId, intQuestionId, RosterVector.Empty, DateTime.UtcNow, 7);

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(interviewerId, intQuestionId, RosterVector.Empty, DateTime.UtcNow, 5);

                    return new
                    {
                        AnswersDeclaredPlausibleEvent = GetFirstEventByType<AnswersDeclaredPlausible>(eventContext.Events),
                        AnswersDeclaredValidEvent = GetFirstEventByType<AnswersDeclaredValid>(eventContext.Events),
                        AnswersDeclaredImplausibleEvent = GetFirstEventByType<AnswersDeclaredImplausible>(eventContext.Events),
                        AnswersDeclaredInvalidEvent = GetFirstEventByType<AnswersDeclaredInvalid>(eventContext.Events)
                    };
                }
            });

            Assert.That(results, Is.Not.Null);
            Assert.That(results.AnswersDeclaredPlausibleEvent, Is.Not.Null);
            Assert.That(results.AnswersDeclaredPlausibleEvent.Questions.Count, Is.EqualTo(1));
            Assert.That(results.AnswersDeclaredPlausibleEvent.Questions.Select(e => e.Id).ToArray(), Is.EquivalentTo(new[] { intQuestionId }));

            Assert.That(results.AnswersDeclaredImplausibleEvent, Is.Null);
            Assert.That(results.AnswersDeclaredValidEvent, Is.Null);
            Assert.That(results.AnswersDeclaredInvalidEvent, Is.Null);

            appDomainContext.Dispose();
            appDomainContext = null;
        }


        [Test]
        public void When_correct_answering_on_question_and_exists_static_text_with_condition_on_warnings_Then_warning_should_be_raised_event_to_reset()
        {
            var interviewerId = Guid.Parse("11111111111111111111111111111111");
            var intQuestionId = Guid.Parse("22222222222222222222222222222222");
            var staticTextId = Guid.Parse("33333333333333333333333333333333");

            var appDomainContext = AppDomainContext.Create();

            var results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.NumericIntegerQuestion(intQuestionId, variable: "i"),
                    Create.Entity.StaticText(staticTextId, validationConditions: new List<ValidationCondition>
                    {
                        Create.Entity.ValidationCondition("i >= 5", "warning", ValidationSeverity.Warning),
                        Create.Entity.ValidationCondition("i <= 5", "warning", ValidationSeverity.Warning),
                    })
                );
                var interview = SetupInterview(appDomainContext.AssemblyLoadContext, questionnaire);

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(interviewerId, intQuestionId, RosterVector.Empty, DateTime.UtcNow, 5);

                    return new
                    {
                        StaticTextsDeclaredPlausibleEvent = GetFirstEventByType<StaticTextsDeclaredPlausible>(eventContext.Events),
                        StaticTextsDeclaredValidEvent = GetFirstEventByType<StaticTextsDeclaredValid>(eventContext.Events),
                        StaticTextsDeclaredImplausibleEvent = GetFirstEventByType<StaticTextsDeclaredImplausible>(eventContext.Events),
                        StaticTextsDeclaredInvalidEvent = GetFirstEventByType<StaticTextsDeclaredInvalid>(eventContext.Events)
                    };
                }
            });

            Assert.That(results, Is.Not.Null);
            Assert.That(results.StaticTextsDeclaredPlausibleEvent, Is.Not.Null);
            Assert.That(results.StaticTextsDeclaredPlausibleEvent.StaticTexts.Count, Is.EqualTo(1));
            Assert.That(results.StaticTextsDeclaredPlausibleEvent.StaticTexts.Select(e => e.Id).ToArray(), Is.EquivalentTo(new[] { staticTextId }));

            Assert.That(results.StaticTextsDeclaredImplausibleEvent, Is.Null);
            Assert.That(results.StaticTextsDeclaredValidEvent, Is.Null);
            Assert.That(results.StaticTextsDeclaredInvalidEvent, Is.Null);

            appDomainContext.Dispose();
            appDomainContext = null;
        }

        [Test]
        public void When_answering_second_question_that_reduces_active_warnings_on_static_text_Then_ImplausibleEvent_should_carry_only_remaining_warnings()
        {
            // This tests the scenario from issue #4149: static text with 3 warning conditions, all firing initially,
            // then answering a question should cause only 1 warning to remain.
            var interviewerId = Guid.Parse("11111111111111111111111111111111");
            var triggerQuestionId = Guid.Parse("22222222222222222222222222222222");
            var numericQuestionId = Guid.Parse("44444444444444444444444444444444");
            var staticTextId = Guid.Parse("33333333333333333333333333333333");

            var appDomainContext = AppDomainContext.Create();

            var results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                // Static text warnings fire when: trigger==1 AND n does not exceed threshold.
                // Condition 0: passes when trigger!=1 OR n>10; fails (warning fires) when trigger==1 AND n<=10
                // Condition 1: passes when trigger!=1 OR n>20; fails when trigger==1 AND n<=20
                // Condition 2: passes when trigger!=1 OR n>30; fails when trigger==1 AND n<=30
                // So for trigger=1 and n=21: warnings [0] and [1] clear, only [2] remains.
                var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.NumericIntegerQuestion(triggerQuestionId, variable: "trigger"),
                    Create.Entity.NumericIntegerQuestion(numericQuestionId, variable: "n"),
                    Create.Entity.StaticText(staticTextId, validationConditions: new List<ValidationCondition>
                    {
                        Create.Entity.ValidationCondition("trigger != 1 || n > 10", "warning 0", ValidationSeverity.Warning),
                        Create.Entity.ValidationCondition("trigger != 1 || n > 20", "warning 1", ValidationSeverity.Warning),
                        Create.Entity.ValidationCondition("trigger != 1 || n > 30", "warning 2", ValidationSeverity.Warning),
                    })
                );
                var interview = SetupInterview(appDomainContext.AssemblyLoadContext, questionnaire);

                // Step 1: answer trigger = 1 (with n unanswered, all 3 warnings fire)
                StaticTextsDeclaredImplausible step1ImplausibleEvent = null;
                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(interviewerId, triggerQuestionId, RosterVector.Empty, DateTime.UtcNow, 1);
                    step1ImplausibleEvent = GetFirstEventByType<StaticTextsDeclaredImplausible>(eventContext.Events);
                }

                // Step 2: answer n = 21 — warnings [0] and [1] should clear, only [2] remains
                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(interviewerId, numericQuestionId, RosterVector.Empty, DateTime.UtcNow, 21);

                    var allImplausibleEvents = eventContext.Events
                        .Select(e => e.Payload)
                        .OfType<StaticTextsDeclaredImplausible>()
                        .ToList();

                    var allPlausibleEvents = eventContext.Events
                        .Select(e => e.Payload)
                        .OfType<StaticTextsDeclaredPlausible>()
                        .ToList();

                    return new
                    {
                        Step1ImplausibleConditionsCount = step1ImplausibleEvent?.GetFailedValidationConditionsDictionary()
                            .TryGetValue(new Identity(staticTextId, RosterVector.Empty), out var s1c) == true ? s1c.Count : -1,
                        StaticTextsDeclaredImplausibleEvent = GetFirstEventByType<StaticTextsDeclaredImplausible>(eventContext.Events),
                        StaticTextsDeclaredPlausibleEvent = GetFirstEventByType<StaticTextsDeclaredPlausible>(eventContext.Events),
                        ImplausibleEventCount = allImplausibleEvents.Count,
                        PlausibleEventCount = allPlausibleEvents.Count,
                        AllEventTypes = eventContext.Events.Select(e => e.Payload.GetType().Name).ToArray(),
                    };
                }
            });

            Assert.That(results, Is.Not.Null);

            // Verify step 1 correctly fired all 3 warnings
            Assert.That(results.Step1ImplausibleConditionsCount, Is.EqualTo(3),
                "Step 1 should fire Implausible with all 3 warning conditions when trigger=1 and n=null");

            // The static text still has 1 warning (condition index 2), so Implausible fires (not Plausible)
            Assert.That(results.StaticTextsDeclaredImplausibleEvent, Is.Not.Null,
                "StaticTextsDeclaredImplausible should fire because static text still has warning [2]");
            Assert.That(results.StaticTextsDeclaredPlausibleEvent, Is.Null,
                "StaticTextsDeclaredPlausible should NOT fire because static text still has a warning");

            var failedConditions = results.StaticTextsDeclaredImplausibleEvent
                .GetFailedValidationConditionsDictionary()[new Identity(staticTextId, RosterVector.Empty)];

            Assert.That(failedConditions.Count, Is.EqualTo(1),
                "Only warning [2] should remain — warnings [0] and [1] should have been cleared");
            Assert.That(failedConditions[0].FailedConditionIndex, Is.EqualTo(2),
                "The remaining warning should be at index 2 (condition 'n > 30' fails for n=21)");

            appDomainContext.Dispose();
            appDomainContext = null;
        }
    }
}
