using System;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_completing_interview_with_substitutions_changed : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            Guid substitutionId = Guid.Parse("77777777777777777777777777777777");
            Guid userId = Guid.Parse("88888888888888888888888888888888");
            string substitutionVar = "subst";

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[] {
                Create.Entity.TextQuestion(questionId: questionId, text: $"substitution is %{substitutionVar}%"),
                Create.Entity.StaticText(publicKey: staticTextId, text: $"substitution is %{substitutionVar}%"),
                Create.Entity.Group(groupId: groupId, title: $"substitution is %{substitutionVar}%"),
                Create.Entity.TextQuestion(questionId: substitutionId, variable: substitutionVar),
            });

            interview = Setup.StatefulInterview(questionnaireDocument: questionnaire);

            interview.AnswerTextQuestion(userId, substitutionId, RosterVector.Empty, DateTime.UtcNow, "substitution text");

            eventContext = Create.Other.EventContext();
        };

        Because of = () => interview.Complete(Guid.NewGuid(), string.Empty, DateTime.UtcNow);

        It should_raise_substitutions_changed_event_with_all_changed_entities = () =>
        {
            var @event = eventContext.GetEvent<SubstitutionTitlesChanged>();
            @event.Groups.ShouldContainOnly(Create.Entity.Identity(groupId, RosterVector.Empty));
            @event.StaticTexts.ShouldContainOnly(Create.Entity.Identity(staticTextId, RosterVector.Empty));
            @event.Questions.ShouldContainOnly(Create.Entity.Identity(questionId, RosterVector.Empty));
        };

        static StatefulInterview interview;
        static EventContext eventContext;
        static Guid questionId = Guid.Parse("DDDDDDDDDDDDDDDD1111111111111111");
        static Guid staticTextId = Guid.Parse("DDDDDDDDDDDDDDDD2222222222222222");
        static Guid groupId = Guid.Parse("DDDDDDDDDDDDDDDD3333333333333333");
    }
}