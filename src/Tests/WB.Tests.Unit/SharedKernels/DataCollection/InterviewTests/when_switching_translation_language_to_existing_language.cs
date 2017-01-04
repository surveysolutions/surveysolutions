using System;
using Machine.Specifications;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_switching_translation_language_to_existing_language : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(Guid.NewGuid(), questionnaire
                => questionnaire.GetTranslationLanguages() == new [] { language });

            interview = CreateInterview(questionnaireRepository: questionnaireRepository);

            eventContext = new EventContext();
        };

        Because of = () =>
            interview.SwitchTranslation(new SwitchTranslation(Guid.NewGuid(), language, Guid.NewGuid()));

        It should_publish_TranslationSwitched_event_with_target_language = () =>
            eventContext.ShouldContainEvent<TranslationSwitched>(@event => @event.Language == language);

        private static string language = "French";
        private static Interview interview;
        private static EventContext eventContext;
    }
}