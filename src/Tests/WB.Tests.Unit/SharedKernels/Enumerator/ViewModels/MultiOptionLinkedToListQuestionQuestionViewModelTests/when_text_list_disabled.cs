﻿using System;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.MultiOptionLinkedToListQuestionQuestionViewModelTests
{
    [TestOf(typeof(CategoricalMultiLinkedToListViewModel))]
    public class when_text_list_disabled
    {
        [Test]
        public void should_view_model_has_empty_options()
        {
            //arrange
            var textListQuestionId = Guid.Parse("11111111111111111111111111111111");
            var multiOptionQuestionId = Guid.Parse("22222222222222222222222222222222");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextListQuestion(textListQuestionId),
                Create.Entity.MultyOptionsQuestion(multiOptionQuestionId, linkedToQuestionId: textListQuestionId, areAnswersOrdered: true));

            var interview = SetUp.StatefulInterview(questionnaire);
            interview.AnswerTextListQuestion(Guid.NewGuid(), textListQuestionId, RosterVector.Empty, DateTime.UtcNow, new []
            {
                new Tuple<decimal, string>(1, "option 1"),
                new Tuple<decimal, string>(2, "option 2"),
                new Tuple<decimal, string>(3, "option 3")
            });

            var viewModel = Create.ViewModel.MultiOptionLinkedToListQuestionQuestionViewModel(Create.Entity.PlainQuestionnaire(questionnaire), interview);
            viewModel.Init(null, Identity.Create(multiOptionQuestionId, RosterVector.Empty), Create.Other.NavigationState());

            interview.AnswerMultipleOptionsQuestion(Guid.NewGuid(), multiOptionQuestionId, RosterVector.Empty, DateTime.UtcNow, new[] { 3, 1 });

            viewModel.Handle(Create.Event.MultipleOptionsQuestionAnswered(multiOptionQuestionId, RosterVector.Empty, new[] { 3m, 1m }));
            interview.Apply(Create.Event.QuestionsDisabled(textListQuestionId, RosterVector.Empty));

            //act
            viewModel.Handle(Create.Event.QuestionsDisabled(textListQuestionId, RosterVector.Empty));
            //assert
            Assert.That(viewModel.Options, Is.Empty);
        }
        
    }
}
