﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Machine.Specifications;
using NSubstitute;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;

namespace WB.Tests.Unit.BoundedContexts.Tester.ViewModels.MultiOptionLinkedQuestionViewModelTests
{
    internal class when_linked_question_is_linked_to_question_in_roster_2_levels_deeper : MultiOptionLinkedQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            var firstRosterTitleQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var secondRosterTitlQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            linkedQuestionId = new Identity(Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC"), Empty.RosterVector);

            var questionnaireModel = Create.QuestionnaireModel(new BaseQuestionModel[]
            {
                Create.TextQuestionModel(questionId: firstRosterTitleQuestionId),
                Create.TextQuestionModel(questionId: secondRosterTitlQuestionId),
                Create.LinkedMultiOptionQuestionModel(questionId: linkedQuestionId.Id, linkedToQuestionId: secondRosterTitlQuestionId)
            });

            var interview = Substitute.For<IStatefulInterview>();
            interview.Answers.Returns(new ReadOnlyDictionary<string, BaseInterviewAnswer>(new Dictionary<string, BaseInterviewAnswer>()));

            var linkedAnswerRosterVector = new decimal[]{1, 1};
            interview.FindAnswersOfReferencedQuestionForLinkedQuestion(secondRosterTitlQuestionId, Arg.Any<Identity>())
                .Returns(new List<BaseInterviewAnswer> {
                    Create.TextAnswer("hamster", secondRosterTitlQuestionId, linkedAnswerRosterVector),
                    Create.TextAnswer("parrot", secondRosterTitlQuestionId, new decimal[]{1, 2}), 
                    Create.TextAnswer("hamster", secondRosterTitlQuestionId, new decimal[]{2, 1}),
                    Create.TextAnswer("parrot", secondRosterTitlQuestionId, new decimal[]{2, 2})
                });

            interview.GetParentRosterTitlesWithoutLast(secondRosterTitlQuestionId, linkedAnswerRosterVector)
                .Returns(new List<string> {
                    "nastya"
                });

            viewModel = CreateViewModel(questionnaireModel, interview);
        };

        Because of = () => viewModel.Init("interview", linkedQuestionId, Create.NavigationState());

        It should_substitute_titles_from_both_questions = () => viewModel.Options.First().Title.ShouldEqual("nastya: hamster");

        It should_substitute_titles_all_roster_combinations = () => viewModel.Options.Count.ShouldEqual(4);

        static MultiOptionLinkedQuestionViewModel viewModel;
        static Identity linkedQuestionId;
    }
}

