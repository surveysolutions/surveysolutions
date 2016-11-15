using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.Translations
{
    [Ignore("KP-8159")]
    internal class when_switching_language_with_numeric_roster_having_title_question : InterviewTestsContext
    {
        Establish context = () =>
        {
            rosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            rosterSizeQuestion = Guid.Parse("11111111111111111111111111111111");
            rosterTitleQuestionId = Guid.Parse("21111111111111111111111111111111");

            var options = new List<Answer>();
            options.Add(Create.Entity.Answer(1.ToString(), 1));
            options.Add(Create.Entity.Answer(2.ToString(), 2));

            var nonTranslatedQuestionnaire = CreateQuestionnaireDocumentWithOneChapter(
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
                    }

                    ));

            nonTranslatedQuestionnaire.Translations.Add(
                    Create.Entity.Translation(translationId, targetLanguage));

            var optionsTranslated = new List<Answer>();
            optionsTranslated.Add(Create.Entity.Answer(11.ToString(), 1));
            optionsTranslated.Add(Create.Entity.Answer(22.ToString(), 2));

            var translatedQuestionnaire = CreateQuestionnaireDocumentWithOneChapter(
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
                    }
                    ));

            IQuestionnaire nonTranslatedPlainQuestionnaire = Create.Entity.PlainQuestionnaire(nonTranslatedQuestionnaire);
            IQuestionnaire translatedPlainQuestionnaire = Create.Entity.PlainQuestionnaire(translatedQuestionnaire);

            IQuestionnaireStorage questionnaires = Moq.Mock.Of<IQuestionnaireStorage>(x =>
                x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), null) == nonTranslatedPlainQuestionnaire &&
                x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), targetLanguage) == translatedPlainQuestionnaire);

            interview = Create.AggregateRoot.Interview(questionnaireRepository: questionnaires);

            interview.AnswerNumericIntegerQuestion(Guid.NewGuid(), rosterSizeQuestion, RosterVector.Empty, DateTime.Now, 1);
            interview.AnswerSingleOptionQuestion(Guid.NewGuid(), rosterTitleQuestionId, new decimal[] {0}, DateTime.Now, 1);
            eventContext = new EventContext();
        };


        Because of = () => interview.SwitchTranslation(Create.Command.SwitchTranslation(language: targetLanguage));

        It should_raise_roster_titles_changed_event_for_roster = () =>
        {
            var @event = eventContext.GetSingleEvent<RosterInstancesTitleChanged>();
            @event.ChangedInstances.Length.ShouldEqual(1);
            @event.ChangedInstances.All(x => x.RosterInstance.GroupId == rosterId).ShouldBeTrue();
            @event.ChangedInstances[0].RosterInstance.RosterInstanceId.ShouldEqual(0);
            @event.ChangedInstances[0].Title.ShouldEqual("11");
        };

        static Interview interview;
        static EventContext eventContext;
        static Guid translationId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        static string targetLanguage = "ru";
        static Guid rosterId;
        static Guid rosterSizeQuestion;
        static Guid rosterTitleQuestionId;
    }
}