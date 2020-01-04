using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.MultiOptionQuestionViewModelTests
{
    internal class when_answer_removed : MultiOptionQuestionViewModelTestsContext
    {
        [Test]
        public void should_remove_answer_with_its_order()
        {
            var questionGuid = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var questionId = Create.Entity.Identity(questionGuid, Empty.RosterVector);
            var userId = Guid.NewGuid();

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.MultyOptionsQuestion(questionGuid, new List<Answer> { Create.Entity.Answer("one", 1) }, areAnswersOrdered: true));

            StatefulInterview interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);
            interview.AnswerMultipleOptionsQuestion(userId, questionGuid, RosterVector.Empty, DateTime.Now, new[] { 1 });

            var eventRegistry = Create.Service.LiteEventRegistry();

            var viewModel = CreateViewModel(
                questionnaireStorage: SetUp.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire),
                interviewRepository: Stub<IStatefulInterviewRepository>.Returning((IStatefulInterview)interview),
                filteredOptionsViewModel: Create.ViewModel.FilteredOptionsViewModel(questionId, questionnaire, interview),
                eventRegistry: eventRegistry);

            viewModel.Init(interview.Id.FormatGuid(), questionId, Create.Other.NavigationState());

            // Act
            interview.RemoveAnswer(questionGuid, RosterVector.Empty, userId, DateTimeOffset.UtcNow);
            SetUp.ApplyInterviewEventsToViewModels(interview, eventRegistry, interview.Id);

            // assert
            var option = viewModel.Options.First();

            option.Checked.Should().BeFalse();
            option.CheckedOrder.Should().NotHaveValue();
        }
    }
}
