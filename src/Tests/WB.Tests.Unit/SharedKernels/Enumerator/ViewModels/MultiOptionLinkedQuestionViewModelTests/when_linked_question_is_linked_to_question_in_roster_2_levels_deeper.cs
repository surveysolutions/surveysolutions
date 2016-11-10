using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Machine.Specifications;
using Moq;
using Nito.AsyncEx.Synchronous;
using NSubstitute;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.Enumerator.Aggregates;

using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.MultiOptionLinkedQuestionViewModelTests
{
    internal class when_linked_question_is_linked_to_question_in_roster_2_levels_deeper : MultiOptionLinkedQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            var secondRosterTitlQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            linkedQuestionId = new Identity(Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC"), Empty.RosterVector);

            var questionnaire = Mock.Of<IQuestionnaire>(_
                => _.GetQuestionReferencedByLinkedQuestion(linkedQuestionId.Id) == secondRosterTitlQuestionId
                && _.ShouldQuestionRecordAnswersOrder(linkedQuestionId.Id) == false);

            var interview = Substitute.For<IStatefulInterview>();
            interview.Answers.Returns(new ReadOnlyDictionary<string, BaseInterviewAnswer>(new Dictionary<string, BaseInterviewAnswer>()));

            var linkedAnswerRosterVector = new decimal[]{1, 1};
            interview.FindAnswersOfReferencedQuestionForLinkedQuestion(secondRosterTitlQuestionId, Arg.Any<Identity>())
                .Returns(new List<BaseInterviewAnswer> {
                    Create.Entity.InterviewTreeTextQuestion("hamster", secondRosterTitlQuestionId, linkedAnswerRosterVector),
                    Create.Entity.InterviewTreeTextQuestion("parrot", secondRosterTitlQuestionId, new decimal[]{1, 2}), 
                    Create.Entity.InterviewTreeTextQuestion("hamster", secondRosterTitlQuestionId, new decimal[]{2, 1}),
                    Create.Entity.InterviewTreeTextQuestion("parrot", secondRosterTitlQuestionId, new decimal[]{2, 2})
                });

            interview.GetParentRosterTitlesWithoutLast(TODO)
                .Returns(new List<string> {
                    "nastya"
                });

            questionViewModel = CreateViewModel(questionnaire, interview);
        };

        Because of = () => questionViewModel.Init("interview", linkedQuestionId, Create.Other.NavigationState());

        It should_substitute_titles_from_both_questions = () => questionViewModel.Options.First().Title.ShouldEqual("nastya: hamster");

        It should_substitute_titles_all_roster_combinations = () => questionViewModel.Options.Count.ShouldEqual(4);

        static MultiOptionLinkedToQuestionQuestionViewModel questionViewModel;
        static Identity linkedQuestionId;
    }
}

