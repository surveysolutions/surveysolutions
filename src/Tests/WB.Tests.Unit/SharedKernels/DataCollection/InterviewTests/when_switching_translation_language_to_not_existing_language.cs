using System;
using FluentAssertions;
using Ncqrs.Spec;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_switching_translation_language_to_not_existing_language : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(Guid.NewGuid(), questionnaire
                => questionnaire.GetTranslationLanguages() == new [] { "English" });

            interview = CreateInterview(questionnaireRepository: questionnaireRepository);
            BecauseOf();
        }

        public void BecauseOf() =>
            interviewException =  NUnit.Framework.Assert.Throws<InterviewException>(() =>
                interview.SwitchTranslation(new SwitchTranslation(Guid.NewGuid(), language, Guid.NewGuid())));

        [NUnit.Framework.Test] public void should_throw_InterviewException () =>
            interviewException.Should().NotBeNull();

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containing__translation____language__ () =>
            interviewException.Message.ToLower().ToSeparateWords().Should().Contain("translation", "language");

        private static string language = "Afrikaans";
        private static Interview interview;
        private static InterviewException interviewException;
    }
}
