using System;
using Machine.Specifications;
using Ncqrs.Spec;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_switching_translation_language_to_not_existing_language : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(Guid.NewGuid(), questionnaire
                => questionnaire.GetTranslationLanguages() == new [] { new Translation { Id = Guid.NewGuid(), Name= "English" }});

            interview = CreateInterview(questionnaireRepository: questionnaireRepository);

            eventContext = new EventContext();
        };

        Because of = () =>
            interviewException = Catch.Only<InterviewException>(() =>
                interview.SwitchTranslation(new SwitchTranslation(Guid.NewGuid(), translationId, Guid.NewGuid())));

        It should_throw_InterviewException = () =>
            interviewException.ShouldNotBeNull();

        It should_throw_exception_with_message_containing__translation____language__translationId = () =>
            interviewException.Message.ToLower().ToSeparateWords().ShouldContain("translation", translationId.FormatGuid());

        private static readonly Guid translationId = Guid.Parse("11111111111111111111111111111111");
        private static Interview interview;
        private static EventContext eventContext;
        private static InterviewException interviewException;
    }
}