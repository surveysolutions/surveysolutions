using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.Substitutions
{
    internal class when_validation_message_contains_substitution : InterviewTestsContext
    {
        private Establish context = () =>
        {
            rosterSizeQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            staticTextId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            questionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

            var substitutedVariableName = "subst";
            QuestionnaireDocument questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericIntegerQuestion(id: rosterSizeQuestionId, variable: $"{substitutedVariableName}"), 

                Create.Entity.StaticText(publicKey: staticTextId,
                    validationConditions: new List<ValidationCondition> {
                        Create.Entity.ValidationCondition(message: $"error %{substitutedVariableName}%")
                    }), 
                
                Create.Entity.Roster(rosterSizeSourceType: RosterSizeSourceType.Question,
                    rosterSizeQuestionId: rosterSizeQuestionId,
                    title: "Roster",
                    children: new List<IComposite> {
                        Create.Entity.Question(
                            questionId: questionId,
                            validationConditions: new List<ValidationCondition> {
                                Create.Entity.ValidationCondition(message: $"error %{substitutedVariableName}%")
                            })
                    }));

            Guid questionnaireId = Guid.NewGuid();
            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId,
                new PlainQuestionnaire(questionnaire, 1));

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);

            events = new EventContext();
        };

        Because of = () => 
            interview.AnswerNumericIntegerQuestion(Guid.NewGuid(), rosterSizeQuestionId, Empty.RosterVector, DateTime.Now, 2);

        It should_raise_title_changed_event_for_group_after_answer = () =>
            events.ShouldContainEvent<SubstitutionTitlesChanged>(x => 
                x.StaticTexts[0].Equals(Create.Entity.Identity(staticTextId, RosterVector.Empty)) && 
                x.Questions[0].Equals(Create.Entity.Identity(questionId, Create.Entity.RosterVector(0))) &&
                x.Questions[1].Equals(Create.Entity.Identity(questionId, Create.Entity.RosterVector(1))) 
            );

        static EventContext events;
        static Interview interview;
        static Guid rosterSizeQuestionId;
        static Guid staticTextId;
        static Guid questionId;
    }
}