using System;
using System.Collections.Generic;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal partial class StatefullInterviewTests
    {
        [Test]
        public void When_question_is_implausuble()
        {
            var questionIdentity = Create.Entity.Identity(Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"), RosterVector.Empty);

            var plainQuestionnaireRepository = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                {
                    Create.Entity.TextQuestion(questionIdentity.Id,
                        validationConditions:
                        new List<ValidationCondition>
                        {
                            new ValidationCondition("1=1", "implausible"){Severity = ValidationSeverity.Warning}
                        })
                }));
            var statefulInterview = Create.AggregateRoot.StatefulInterview(questionnaireRepository: plainQuestionnaireRepository);
            statefulInterview.Apply(Create.Event.TextQuestionAnswered(questionIdentity.Id, questionIdentity.RosterVector, "test"));

            // act
            statefulInterview.Apply(Create.Event.AnswersDeclaredImplausible(questionIdentity, new[] {0}));

            // assert
            Assert.That(statefulInterview.IsEntityPlausible(questionIdentity), Is.False);
        }

        [Test]
        public void When_question_is_plausuble()
        {
            var questionIdentity = Create.Entity.Identity(Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"), RosterVector.Empty);

            var plainQuestionnaireRepository = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                {
                    Create.Entity.TextQuestion(questionIdentity.Id,
                        validationConditions:
                        new List<ValidationCondition>
                        {
                            new ValidationCondition("1=1", "plausible"){Severity = ValidationSeverity.Warning}
                        })
                }));
            var statefulInterview = Create.AggregateRoot.StatefulInterview(questionnaireRepository: plainQuestionnaireRepository);
            statefulInterview.Apply(Create.Event.TextQuestionAnswered(questionIdentity.Id, questionIdentity.RosterVector, "test"));

            // act
            statefulInterview.Apply(Create.Event.AnswersDeclaredPlausible(questionIdentity));

            // assert
            Assert.That(statefulInterview.IsEntityPlausible(questionIdentity), Is.True);
        }

        [Test]
        public void When_static_text_is_implausuble()
        {
            var staticTextIdentity = Create.Entity.Identity(Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"), RosterVector.Empty);

            var plainQuestionnaireRepository = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                {
                    Create.Entity.StaticText(staticTextIdentity.Id,
                        validationConditions:
                        new List<ValidationCondition>
                        {
                            new ValidationCondition("1=1", "implausible"){Severity = ValidationSeverity.Warning}
                        })
                }));
            var statefulInterview = Create.AggregateRoot.StatefulInterview(questionnaireRepository: plainQuestionnaireRepository);
            statefulInterview.Apply(Create.Event.TextQuestionAnswered(staticTextIdentity.Id, staticTextIdentity.RosterVector, "test"));

            // act
            statefulInterview.Apply(Create.Event.StaticTextsDeclaredImplausible(staticTextIdentity, new[] { 0 }));

            // assert
            Assert.That(statefulInterview.IsEntityPlausible(staticTextIdentity), Is.False);
        }

        [Test]
        public void When_static_text_is_plausuble()
        {
            var staticTextIdentity = Create.Entity.Identity(Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"), RosterVector.Empty);

            var plainQuestionnaireRepository = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                {
                    Create.Entity.StaticText(staticTextIdentity.Id,
                        validationConditions:
                        new List<ValidationCondition>
                        {
                            new ValidationCondition("1=1", "plausible"){Severity = ValidationSeverity.Warning}
                        })
                }));
            var statefulInterview = Create.AggregateRoot.StatefulInterview(questionnaireRepository: plainQuestionnaireRepository);
            statefulInterview.Apply(Create.Event.TextQuestionAnswered(staticTextIdentity.Id, staticTextIdentity.RosterVector, "test"));

            // act
            statefulInterview.Apply(Create.Event.StaticTextsDeclaredPlausible(staticTextIdentity));

            // assert
            Assert.That(statefulInterview.IsEntityPlausible(staticTextIdentity), Is.True);
        }
    }
}