using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
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
                var interview = SetupInterview(questionnaire);

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
                var interview = SetupInterview(questionnaire);
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
                var interview = SetupInterview(questionnaire);
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
                var interview = SetupInterview(questionnaire);

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
    }
}
