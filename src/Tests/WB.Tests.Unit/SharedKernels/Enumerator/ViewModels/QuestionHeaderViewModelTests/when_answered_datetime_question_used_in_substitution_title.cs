using System;
using FluentAssertions;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.QuestionHeaderViewModelTests
{
    internal class when_answered_datetime_question_used_in_substitution_title : QuestionHeaderViewModelTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.StaticText(publicKey: staticTextIdentity.Id, text: "%date_time%"),
                Create.Entity.DateTimeQuestion(questionId: questionId, variable: "date_time"),
            });

            statefullInterview = Create.AggregateRoot.StatefulInterview(
                questionnaireRepository: Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireDocument));
            BecauseOf();
        }

        public void BecauseOf() => statefullInterview.AnswerDateTimeQuestion(Guid.NewGuid(), questionId, RosterVector.Empty, DateTime.UtcNow, new DateTime(2000, 10, 10));

        [NUnit.Framework.Test] public void should_put_time_opening_tag_to_browser_ready_title_html () => statefullInterview.GetBrowserReadyTitleHtml(staticTextIdentity)
            .Should().Contain("<time date=");

        [NUnit.Framework.Test] public void should_put_time_closing_tag_to_browser_ready_title_html () => statefullInterview.GetBrowserReadyTitleHtml(staticTextIdentity)
            .Should().Contain("</time>");

        [NUnit.Framework.Test] public void should_not_put_time_opening_tag_to_title () => statefullInterview.GetTitleText(staticTextIdentity)
            .Should().NotContain("<time");

        [NUnit.Framework.Test] public void should_not_put_time_closing_tag_to_title () => statefullInterview.GetTitleText(staticTextIdentity)
            .Should().NotContain("</time>");

        static StatefulInterview statefullInterview;
        static Guid questionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        static Identity staticTextIdentity = Identity.Create(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), RosterVector.Empty);
    }
}
