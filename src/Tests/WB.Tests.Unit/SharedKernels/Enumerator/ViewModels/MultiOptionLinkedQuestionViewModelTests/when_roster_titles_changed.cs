using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.MultiOptionLinkedQuestionViewModelTests
{
    internal class when_roster_titles_changed : MultiOptionLinkedQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            level1TriggerId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            level2triggerId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var linkToQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var linkedQuestionId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            topRosterId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
            questionIdentity = Create.Entity.Identity(linkedQuestionId, RosterVector.Empty);

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
                Create.Entity.MultipleOptionsQuestion(questionId: linkedQuestionId, linkedToQuestionId: linkToQuestionId)
                );
            var questionnaireRepository = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire);

            interview = Create.AggregateRoot.StatefulInterview(questionnaireRepository: questionnaireRepository, userId: interviewerId);
            var interviewRepository = Create.Fake.StatefulInterviewRepositoryWith(interview);

            viewModel = CreateViewModel(questionnaireStorage: questionnaireRepository, interviewRepository: interviewRepository);
            viewModel.Init(interview.Id.FormatGuid(), questionIdentity, Create.Other.NavigationState());
        };

        Because of = () =>
        {
            interview.AnswerTextListQuestion(interviewerId, level1TriggerId, RosterVector.Empty, DateTime.UtcNow,
                new [] {new Tuple<decimal, string>(1, "title"),});

            interview.AnswerTextListQuestion(interviewerId, level2triggerId, RosterVector.Empty, DateTime.UtcNow,
                new[] { new Tuple<decimal, string>(1, "subtitle"), });
        };

        It should_refresh_list_of_options = () => viewModel.Options.Count.ShouldEqual(1);

        It should_prefix_option_with_parent_title = () => viewModel.Options.First().Title.ShouldEqual("title: subtitle");

        static MultiOptionLinkedToQuestionQuestionViewModel viewModel;
        static StatefulInterview interview;
        static Guid topRosterId;
        static Guid interviewerId = Guid.Parse("11111111111111111111111111111111");
        static Guid level1TriggerId;
        static Guid level2triggerId;
        static Identity questionIdentity;
    }
}