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
    internal class when_linked_question_is_linked_to_question_in_roster_2_levels_deeper : MultiOptionLinkedQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            var level1TriggerId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var level2triggerId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var linkToQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var topRosterId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");

            linkedQuestionIdentity = Identity.Create(Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD"), RosterVector.Empty);

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextListQuestion(questionId: level1TriggerId),
                Create.Entity.Roster(rosterSizeQuestionId: level1TriggerId, rosterId: topRosterId, rosterSizeSourceType: RosterSizeSourceType.Question, children: new IComposite[]
                {
                    Create.Entity.TextListQuestion(questionId: level2triggerId),
                    Create.Entity.Roster(rosterSizeQuestionId: level2triggerId, rosterSizeSourceType: RosterSizeSourceType.Question, children: new IComposite[]
                    {
                        Create.Entity.TextQuestion(questionId: linkToQuestionId)
                    })
                }),
                Create.Entity.MultipleOptionsQuestion(questionId: linkedQuestionIdentity.Id, linkedToQuestionId: linkToQuestionId)
                );
            var questionnaireRepository = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire);

            var interview = Create.AggregateRoot.StatefulInterview(questionnaireRepository: questionnaireRepository, userId: interviewerId);
            interview.AnswerTextListQuestion(interviewerId, level1TriggerId, RosterVector.Empty, DateTime.UtcNow,
                new[] { new Tuple<decimal, string>(1, "person 1"), new Tuple<decimal, string>(2, "person 2"), });

            interview.AnswerTextListQuestion(interviewerId, level2triggerId, Create.Entity.RosterVector(1), DateTime.UtcNow,
                new[] { new Tuple<decimal, string>(1, "child 1"), new Tuple<decimal, string>(2, "child 2") });

            interview.AnswerTextListQuestion(interviewerId, level2triggerId, Create.Entity.RosterVector(2), DateTime.UtcNow,
                new[] { new Tuple<decimal, string>(1, "child 3"), new Tuple<decimal, string>(2, "child 4") });

            interview.AnswerTextQuestion(interviewerId, linkToQuestionId, Create.Entity.RosterVector(1, 1), DateTime.UtcNow, "pet 1");
            interview.AnswerTextQuestion(interviewerId, linkToQuestionId, Create.Entity.RosterVector(1, 2), DateTime.UtcNow, "pet 2");
            interview.AnswerTextQuestion(interviewerId, linkToQuestionId, Create.Entity.RosterVector(2, 1), DateTime.UtcNow, "pet 3");
            interview.AnswerTextQuestion(interviewerId, linkToQuestionId, Create.Entity.RosterVector(2, 2), DateTime.UtcNow, "pet 4");

            var interviewRepository = Create.Fake.StatefulInterviewRepositoryWith(interview);

            questionViewModel = CreateViewModel(questionnaireStorage: questionnaireRepository, interviewRepository: interviewRepository);
            
        };

        Because of = () => questionViewModel.Init(null, linkedQuestionIdentity, Create.Other.NavigationState());

        It should_substitute_titles_from_both_questions = () => questionViewModel.Options.First().Title.ShouldEqual("person 1: child 1: pet 1");

        It should_substitute_titles_all_roster_combinations = () => questionViewModel.Options.Count.ShouldEqual(4);

        static MultiOptionLinkedToRosterQuestionQuestionViewModel questionViewModel;
        static Identity linkedQuestionIdentity;
        static Guid interviewerId = Guid.Parse("11111111111111111111111111111111");
    }
}

