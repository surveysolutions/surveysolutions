using System;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Ncqrs.Eventing;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.StaticTextViewModelTests
{
    internal class when_variable_value_and_substitution_title_changed : StaticTextViewModelTestsContext
    {
        Establish context = () =>
        {
            var staticTextWithSubstitutionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var substitutedVariable1Name = "var1";
            var substitutedVariable2Name = "var2";
            staticTextIdentity = new Identity(staticTextWithSubstitutionId, Empty.RosterVector);
            var substitutedVariableIdentity = new Identity(Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC"), RosterVector.Empty);
            
            var questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(new QuestionnaireIdentity(Guid.NewGuid(), 1),
                Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
                {
                    Create.Entity.StaticText(publicKey: staticTextWithSubstitutionId,
                        text: $"Your answer on question is %{substitutedVariable1Name}% and variable is %{substitutedVariable2Name}%"),
                    Create.Entity.NumericRealQuestion(variable: substitutedVariable1Name, id: substitutedQuestionId),
                    Create.Entity.Variable(variableName: substitutedVariable2Name,
                        id: substitutedVariableIdentity.Id, expression: $"({substitutedVariable1Name}*100)/20")
                }));

            statefullInterview = Create.AggregateRoot.StatefulInterview(questionnaireRepository: questionnaireRepository);
            var interviewRepository = Setup.StatefulInterviewRepository(statefullInterview);

            ILiteEventRegistry registry = Create.Service.LiteEventRegistry();
            liteEventBus = Create.Service.LiteEventBus(registry);

            viewModel = CreateViewModel(questionnaireRepository, interviewRepository, registry);
            viewModel.Init(statefullInterview.EventSourceId.FormatGuid(), staticTextIdentity, null);

            statefullInterview.AnswerNumericRealQuestion(interviewerId, substitutedQuestionId, RosterVector.Empty, DateTime.UtcNow, answerOnNumericQuestion);
            statefullInterview.Apply(Create.Event.VariablesChanged(new[]
            {
                new ChangedVariable(substitutedVariableIdentity,  (answerOnNumericQuestion*100)/20)
            }));
            statefullInterview.Apply(Create.Event.SubstitutionTitlesChanged(staticTexts: new[] { staticTextIdentity }));
        };

        Because of = () =>
            liteEventBus.PublishCommittedEvents(new CommittedEventStream(statefullInterview.EventSourceId,
                Create.Other.CommittedEvent(
                    payload: Create.Event.SubstitutionTitlesChanged(staticTexts: new[] {staticTextIdentity}),
                    eventSourceId: statefullInterview.EventSourceId)));

        It should_change_item_title = () => 
            viewModel.Text.PlainText.ShouldEqual($"Your answer on question is {answerOnNumericQuestion} and variable is {(answerOnNumericQuestion * 100)/20}");

        static StaticTextViewModel viewModel;
        static StatefulInterview statefullInterview;
        static ILiteEventBus liteEventBus;
        static int answerOnNumericQuestion = 2;
        static Identity staticTextIdentity;
        static Guid interviewerId = Guid.Parse("11111111111111111111111111111111");
        static Guid substitutedQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
    }
}

