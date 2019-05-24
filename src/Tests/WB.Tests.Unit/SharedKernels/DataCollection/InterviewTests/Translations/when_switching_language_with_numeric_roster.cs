using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.Translations
{
    internal class when_switching_language_with_numeric_roster_having_title_question : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context ()
        {
            rosterId = Id.gA;
            rosterSizeQuestion = Id.g1;
            rosterTitleQuestionId = Id.g2;

            var options = new List<Answer>();
            options.Add(Create.Entity.Answer("Option1", 1));
            options.Add(Create.Entity.Answer("Option2", 2));

            var chapterId = Id.g3;

            var nonTranslatedQuestionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                chapterId,
                Create.Entity.NumericQuestion(questionId: rosterSizeQuestion, isInteger: true),
                Create.Entity.Roster(
                    rosterId: rosterId,
                    rosterSizeSourceType: RosterSizeSourceType.Question,
                    rosterSizeQuestionId: rosterSizeQuestion,
                    rosterTitleQuestionId : rosterTitleQuestionId,
                    children: new List<IComposite>()
                    {
                        Create.Entity.SingleQuestion(
                            id: rosterTitleQuestionId,
                            options: options)
                    }));

            nonTranslatedQuestionnaire.Translations.Add(
                    Create.Entity.Translation(translationId, targetLanguage));

            var optionsTranslated = new List<Answer>();
            optionsTranslated.Add(Create.Entity.Answer("Опция1", 1));
            optionsTranslated.Add(Create.Entity.Answer("Опция2", 2));

            var translatedQuestionnaire = Create.Entity.QuestionnaireDocumentWithOneChapterAndLanguages(
                chapterId,  new [] { targetLanguage },
                Create.Entity.NumericQuestion(questionId: rosterSizeQuestion, isInteger: true),
                Create.Entity.Roster(
                    rosterId: rosterId,
                    rosterSizeSourceType: RosterSizeSourceType.Question,
                    rosterSizeQuestionId: rosterSizeQuestion,
                    rosterTitleQuestionId: rosterTitleQuestionId,
                    children: new List<IComposite>()
                    {
                        Create.Entity.SingleQuestion(
                            id: rosterTitleQuestionId,
                            options: optionsTranslated)
                    }));
            
            IQuestionnaire nonTranslatedPlainQuestionnaire = Create.Entity.PlainQuestionnaire(nonTranslatedQuestionnaire);
            IQuestionnaire translatedPlainQuestionnaire = Create.Entity.PlainQuestionnaire(translatedQuestionnaire);

            IQuestionnaireStorage questionnaires = Moq.Mock.Of<IQuestionnaireStorage>(x =>
                x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), null) == nonTranslatedPlainQuestionnaire &&
                x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), targetLanguage) == translatedPlainQuestionnaire);

            interview = Create.AggregateRoot.StatefulInterview(questionnaireRepository: questionnaires);

            interview.AnswerNumericIntegerQuestion(Guid.NewGuid(), rosterSizeQuestion, RosterVector.Empty, DateTime.Now, 1);
            interview.AnswerSingleOptionQuestion(Guid.NewGuid(), rosterTitleQuestionId, new decimal[] {0}, DateTime.Now, 1);
            eventContext = new EventContext();
            BecauseOf();
        }

        public void BecauseOf() => 
            interview.SwitchTranslation(Create.Command.SwitchTranslation(language: targetLanguage));

        [NUnit.Framework.Test] public void should_raise_roster_titles_changed_event_for_roster () 
        {
            var @event = eventContext.GetSingleEvent<RosterInstancesTitleChanged>();
            @event.ChangedInstances.Length.Should().Be(1);
            @event.ChangedInstances.All(x => x.RosterInstance.GroupId == rosterId).Should().BeTrue();
            @event.ChangedInstances[0].RosterInstance.RosterInstanceId.Should().Be(0);
            @event.ChangedInstances[0].Title.Should().Be("Опция1");
        }

        static Interview interview;
        static EventContext eventContext;
        static Guid translationId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        static string targetLanguage = "ru";
        static Guid rosterId;
        static Guid rosterSizeQuestion;
        static Guid rosterTitleQuestionId;
    }
}
