using System;
using FluentAssertions;
using Main.Core.Entities.Composite;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.StaticTextViewModelTests
{
    internal class when_substitution_title_changed : StaticTextViewModelTestsContext
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            interviewId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
            var staticTextWithSubstitutionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            substitedQuestionIdentity = Identity.Create(Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"), RosterVector.Empty);
            var substitutionIdentity = Create.Identity(staticTextWithSubstitutionId, Empty.RosterVector);
            answerOnDoubleQuestion = 122;
            interviewerId = Guid.Parse("11111111111111111111111111111111");

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.StaticText(publicKey: staticTextWithSubstitutionId, text: "Old title %substitute%"),
                Create.Entity.NumericRealQuestion(variable: "substitute", id: substitedQuestionIdentity.Id)
            }));

            var questionnaireRepository = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(Guid.NewGuid(), questionnaire);

            interview = Create.AggregateRoot.StatefulInterview(questionnaireRepository: questionnaireRepository, userId: interviewerId);
            var interviewRepository = Create.Fake.StatefulInterviewRepositoryWith(interview);
            
            eventRegistry = Create.Service.LiteEventRegistry();

            viewModel = CreateViewModel(questionnaireRepository, interviewRepository, eventRegistry);
            viewModel.Init(interviewId.FormatGuid(), substitutionIdentity, Create.Other.NavigationState());
        
            interview.AnswerNumericRealQuestion(interviewerId, substitedQuestionIdentity.Id,
                substitedQuestionIdentity.RosterVector, DateTime.UtcNow, answerOnDoubleQuestion);

            Setup.ApplyInterviewEventsToViewModels(interview, eventRegistry, interviewId);
        }

        [Test]
        public void should_change_item_title() => viewModel.Text.PlainText.Should().Be($"Old title {answerOnDoubleQuestion}");

        static StaticTextViewModel viewModel;
        static StatefulInterview interview;
        static Guid interviewId;
        static Guid interviewerId;
        static Identity substitedQuestionIdentity;
        static double answerOnDoubleQuestion;
        static ILiteEventRegistry eventRegistry;
    }
}

