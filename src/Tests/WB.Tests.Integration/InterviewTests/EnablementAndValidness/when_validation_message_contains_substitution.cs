using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_validation_message_contains_substitution : InterviewTestsContext
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            rosterSizeQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            staticTextId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            questionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

            var substitutedVariableName = "subst";
            QuestionnaireDocument questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericIntegerQuestion(id: rosterSizeQuestionId, variable: $"{substitutedVariableName}"),
                Create.Entity.StaticText(staticTextId, validationConditions: new List<ValidationCondition>
                    {
                        Create.Entity.ValidationCondition("var1 < 25",  $"error %{substitutedVariableName}%")
                    }),
                Create.Entity.Variable(variableName: "var1", type: VariableType.LongInteger, expression: "numr.Sum(x => x.num2 ?? 0)"),
                Create.Entity.NumericRoster(rosterSizeQuestionId: rosterSizeQuestionId, variable: "numr", children: new List<IComposite>
                {
                    Create.Entity.NumericIntegerQuestion(questionId, "num2", validationConditions: new List<ValidationCondition>
                        {
                            Create.Entity.ValidationCondition("numr.Sum(x => x.num2 ?? 0) < 25", message: $"error %var1%")
                        })
                }));

            interview = SetupStatefullInterview(questionnaire);

            interview.AnswerNumericIntegerQuestion(Guid.NewGuid(), rosterSizeQuestionId, RosterVector.Empty, DateTime.Now, 2);
            interview.AnswerNumericIntegerQuestion(Guid.NewGuid(), questionId, Create.RosterVector(0), DateTime.Now, 10);
            
            events = new EventContext();

            interview.AnswerNumericIntegerQuestion(Guid.NewGuid(), questionId, Create.RosterVector(1), DateTime.Now, 20);
        }


        [Test]
        public void should_substitute_2_into_error_message() =>
            interview.GetFailedValidationMessages(Create.Identity(staticTextId), String.Empty).First().Should().Be("error 2");

        [Test]
        public void should_substitute_30_into_question0_error_message() =>
            interview.GetFailedValidationMessages(Create.Identity(questionId, 0), String.Empty).First().Should().Be("error 30");

        [Test]
        public void should_substitute_30_into_question1_error_message() =>
            interview.GetFailedValidationMessages(Create.Identity(questionId, 1), String.Empty).First().Should().Be("error 30");

        [Test]
        public void should_mark_static_text_and_2_questions_as_invalid() =>
            interview.GetInvalidEntitiesInInterview().Should().BeEquivalentTo(
                Create.Identity(staticTextId),
                Create.Identity(questionId, 0),
                Create.Identity(questionId, 1));

        [Test]
        public void should_raise_title_changed_event_for_questions_0_and_1() =>
            events.GetSingleEvent<SubstitutionTitlesChanged>().Questions.Should().BeEquivalentTo(
                Create.Identity(questionId, 0),
                Create.Identity(questionId, 1));

        static EventContext events;
        static StatefulInterview interview;
        static Guid rosterSizeQuestionId;
        static Guid staticTextId;
        static Guid questionId;
    }
}
