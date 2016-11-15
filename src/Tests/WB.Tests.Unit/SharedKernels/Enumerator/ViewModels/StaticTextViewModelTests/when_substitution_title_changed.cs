using System;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Moq;
using Ncqrs.Eventing;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.StaticTextViewModelTests
{
    [Ignore("KP-8159")]
    internal class when_substitution_title_changed : StaticTextViewModelTestsContext
    {
        Establish context = () =>
        {
            var interviewId = "interviewId";
            var staticTextWithSubstitutionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var substitedQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            substitutionIdentity = new Identity(staticTextWithSubstitutionId, Empty.RosterVector);

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocument(children: new IComposite[]
            {
                Create.Entity.StaticText(publicKey: staticTextWithSubstitutionId, text: "Old title %substitute%"),
                Create.Entity.NumericRealQuestion(variable: "substitute", id: substitedQuestionId)
            }));

            var questionnaireRepository = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(Guid.NewGuid(), questionnaire);

            interview = Create.AggregateRoot.StatefulInterview(questionnaireRepository: questionnaireRepository, userId: interviewerId);
            var interviewRepository = Create.Fake.StatefulInterviewRepositoryWith(interview);

            viewModel = CreateViewModel(questionnaireRepository, interviewRepository, Create.Service.LiteEventRegistry());
            viewModel.Init(interviewId, substitutionIdentity, null);
        };

        Because of = () => interview.AnswerNumericRealQuestion(interviewerId, substitutionIdentity.Id, substitutionIdentity.RosterVector, DateTime.UtcNow, answerOnDoubleQuestion);

        It should_change_item_title = () => viewModel.Text.PlainText.ShouldEqual($"Old title {answerOnDoubleQuestion}");

        static StaticTextViewModel viewModel;
        static StatefulInterview interview;
        static Guid interviewerId = Guid.Parse("11111111111111111111111111111111");
        static Identity substitutionIdentity;
        static double answerOnDoubleQuestion;
    }
}

