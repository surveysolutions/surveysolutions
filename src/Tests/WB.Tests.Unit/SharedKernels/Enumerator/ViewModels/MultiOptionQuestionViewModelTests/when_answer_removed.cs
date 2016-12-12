using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.MultiOptionQuestionViewModelTests
{
    internal class when_answer_removed : MultiOptionQuestionViewModelTestsContext
    {
        static MultiOptionQuestionViewModel viewModel;
        static Identity questionId;
        static Guid questionGuid;
        static readonly Guid userId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

        [OneTimeSetUp]
        public void Context()
        {
            questionGuid = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionId = Create.Entity.Identity(questionGuid, Empty.RosterVector);

            var questionnaire =
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.MultyOptionsQuestion(questionGuid, new List<Answer>()
                    {
                        Create.Entity.Answer("one", 1)
                    }, areAnswersOrdered: true));

            IQuestionnaire plainQuestionnaire = Create.Entity.PlainQuestionnaire(questionnaire, 1);

            StatefulInterview interview = Create.AggregateRoot.StatefulInterview(questionnaire: plainQuestionnaire);
            interview.AnswerMultipleOptionsQuestion(userId, questionGuid, RosterVector.Empty, DateTime.Now, new[] {1});

            viewModel = CreateViewModel(
                questionnaireStorage: Stub<IQuestionnaireStorage>.Returning(plainQuestionnaire),
                interviewRepository: Stub<IStatefulInterviewRepository>.Returning((IStatefulInterview)interview),
                filteredOptionsViewModel: Create.ViewModel.FilteredOptionsViewModel(questionId, questionnaire, interview));

            viewModel.Init("blah", questionId, Create.Other.NavigationState());

            // Act
            viewModel.Handle(Create.Event.AnswersRemoved(questionId));
        }

        [Test]
        public void should_remove_answer_with_its_order()
        {
            var option = viewModel.Options.First();

            option.Checked.Should().BeFalse();
            option.CheckedOrder.Should().NotHaveValue();
        }
    }
}