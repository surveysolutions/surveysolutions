using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.MultiOptionLinkedQuestionViewModelTests
{
    internal class when_initing_view_model : MultiOptionLinkedQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextListQuestion(questionId: RosterSizeQuestionId),
                Create.Entity.Roster(rosterSizeQuestionId: RosterSizeQuestionId, rosterSizeSourceType: RosterSizeSourceType.Question, children: new IComposite[]
                {
                    Create.Entity.TextQuestion(questionId: LinkedToQuestionId)
                }),
                Create.Entity.MultipleOptionsQuestion(questionId: linkedQuestionIdentity.Id, linkedToQuestionId: LinkedToQuestionId));

            var questionnaireRepository = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire);

            var interview = Create.AggregateRoot.StatefulInterview(questionnaireRepository: questionnaireRepository, userId: interviewerId);

            interview.AnswerTextListQuestion(interviewerId, RosterSizeQuestionId, RosterVector.Empty, DateTime.UtcNow,
                new[] { new Tuple<decimal, string>(1, "roster 1"), new Tuple<decimal, string>(2, "roster 2"), });

            interview.AnswerTextQuestion(interviewerId, LinkedToQuestionId, Create.Entity.RosterVector(1), DateTime.UtcNow, "answer 1");
            interview.AnswerTextQuestion(interviewerId, LinkedToQuestionId, Create.Entity.RosterVector(2), DateTime.UtcNow, "answer 2");

            var interviewRepository = Create.Fake.StatefulInterviewRepositoryWith(interview);

            questionViewModel = CreateViewModel(questionnaireStorage: questionnaireRepository, interviewRepository: interviewRepository);
        };

        Because of = () => 
            questionViewModel.Init(null, linkedQuestionIdentity, Create.Other.NavigationState());

        It should_fill_options_from_linked_question = () => questionViewModel.Options.Count.ShouldEqual(2);

        It should_add_linked_question_roster_vectors_as_values_for_answers = () => questionViewModel.Options.First().Value.ShouldContainOnly(1m);

        It should_use_question_answer_as_title = () => questionViewModel.Options.Second().Title.ShouldEqual("answer 2");

        static MultiOptionLinkedToRosterQuestionQuestionViewModel questionViewModel;
        static Identity linkedQuestionIdentity;
        static Guid interviewerId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid RosterSizeQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static readonly Guid LinkedToQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static readonly Identity Identity = linkedQuestionIdentity = Identity.Create(Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD"), RosterVector.Empty);
    }
}

