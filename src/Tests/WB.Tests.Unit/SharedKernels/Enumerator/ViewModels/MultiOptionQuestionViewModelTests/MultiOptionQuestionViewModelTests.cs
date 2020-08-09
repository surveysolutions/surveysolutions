using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.MultiOptionQuestionViewModelTests
{
    internal class MultiOptionQuestionViewModelTests : MultiOptionQuestionViewModelTestsContext
    {
        [Test]
        public void when_filter_options_answered_max_answers_should_show_max_answers_count_in_hint()
        {
            var questionGuid = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var userId = Guid.Parse("11111111111111111111111111111111");
            var questionId = Create.Entity.Identity(questionGuid, Empty.RosterVector);
            var maxAllowedAnswers = 1;

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.MultyOptionsQuestion(questionGuid, new List<Answer>
                {
                    Create.Entity.Answer("1", 1),
                    Create.Entity.Answer("2", 2),
                    Create.Entity.Answer("3", 3),
                }, 
                areAnswersOrdered: true,
                maxAllowedAnswers: maxAllowedAnswers));

            StatefulInterview interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);
            interview.AnswerMultipleOptionsQuestion(userId, questionGuid, RosterVector.Empty, DateTime.Now, new[] { 1 });

            var viewModel = CreateViewModel(
                questionnaireStorage: SetUp.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire),
                interviewRepository: Stub<IStatefulInterviewRepository>.Returning((IStatefulInterview)interview),
                filteredOptionsViewModel: Create.ViewModel.FilteredOptionsViewModel(questionId, questionnaire, interview));

            // Act
            viewModel.Init("blah", questionId, Create.Other.NavigationState());

            var infoViewModel = (CategoricalMultiBottomInfoViewModel)viewModel.Children.Last();
            Assert.That(infoViewModel.MaxAnswersCountMessage, Is.Not.Empty);
            Assert.That(infoViewModel.MaxAnswersCountMessage.Contains(maxAllowedAnswers.ToString()), Is.True);
        }


        [Test]
        public void when_filter_options__on_unanswered_question_should_not_show_max_selected_options_in_hint()
        {
            var questionGuid = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var userId = Guid.Parse("11111111111111111111111111111111");
            var questionId = Create.Entity.Identity(questionGuid, Empty.RosterVector);
            var maxAllowedAnswers = 10;

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.MultyOptionsQuestion(questionGuid, new List<Answer>
                {
                    Create.Entity.Answer("1", 1),
                    Create.Entity.Answer("2", 2),
                    Create.Entity.Answer("3", 3),
                }, 
                areAnswersOrdered: true,
                maxAllowedAnswers: maxAllowedAnswers));

            StatefulInterview interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);

            var viewModel = CreateViewModel(
                questionnaireStorage: SetUp.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire),
                interviewRepository: Stub<IStatefulInterviewRepository>.Returning((IStatefulInterview)interview),
                filteredOptionsViewModel: Create.ViewModel.FilteredOptionsViewModel(questionId, questionnaire, interview));

            // Act
            viewModel.Init("blah", questionId, Create.Other.NavigationState());

            var infoViewModel = (CategoricalMultiBottomInfoViewModel)viewModel.Children.Last();
            Assert.That(infoViewModel.MaxAnswersCountMessage, Is.Empty);
        }
    }
}
