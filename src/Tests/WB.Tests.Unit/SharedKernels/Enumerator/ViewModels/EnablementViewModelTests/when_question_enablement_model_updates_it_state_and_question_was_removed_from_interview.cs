using System;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.EnablementViewModelTests
{
    internal class when_question_enablement_model_updates_it_state_and_question_was_removed_from_interview : EnablementViewModelTestsContext
    {
        Establish context = () =>
        {
            var plainQuestionnaire = new PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
             {
                Create.Entity.MultyOptionsQuestion(multiQuestionId, options: new [] { Create.Entity.Option("1")}),
                Create.Entity.Roster(rosterSizeQuestionId:multiQuestionId, children: new IComposite[]
                {
                    Create.Entity.NumericIntegerQuestion(id: numericQuestionId)
                })
             }), 0);

            var questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(plainQuestionnaire);

            interview = Create.AggregateRoot.StatefulInterview(questionnaire: plainQuestionnaire);
            interview.AnswerMultipleOptionsQuestion(Guid.NewGuid(), multiQuestionIdentity.Id, multiQuestionIdentity.RosterVector, DateTime.Now, new [] { 1 });

            var interviewRepository = Setup.StatefulInterviewRepository(interview);

            viewModel = CreateViewModel(questionnaireRepository, interviewRepository);
            viewModel.Init(interview.EventSourceId.FormatGuid(), numericQuestionIdentity);
        };

        Because of = () =>
        {
            interview.RemoveAnswer(multiQuestionIdentity.Id, multiQuestionIdentity.RosterVector, Guid.NewGuid(), DateTime.Now);
            viewModel.Handle(Create.Event.QuestionsEnabled(multiQuestionIdentity,numericQuestionIdentity));
        };

        It should_disable_model = () => 
            viewModel.Enabled.ShouldBeFalse();

        static EnablementViewModel viewModel;
        static StatefulInterview interview;
        private static readonly Guid multiQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static readonly Guid numericQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly Identity multiQuestionIdentity = Create.Entity.Identity(multiQuestionId, RosterVector.Empty);
        private static readonly Identity numericQuestionIdentity = Create.Entity.Identity(numericQuestionId, Create.Entity.RosterVector(1));
    }
}

