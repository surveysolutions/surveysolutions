using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Enumerator.Native.Questionnaire.Impl;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.DataCollection.PlainQuestionnaireTests
{
    internal class when_getting_options_for_combobox
    {
        [NUnit.Framework.Test] public void should_return_options_from_repository ()
        {
            var answers = new List<Answer>()
                {new Answer() {AnswerCode = 1, AnswerText = "1"}, new Answer() {AnswerCode = 2, AnswerText = "2"}};

            questionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            
            var questionnaire = Create.Entity.QuestionnaireDocument(
                children: new List<IComposite>
                {
                    Create.Entity.SingleQuestion(id:questionId,isFilteredCombobox:true, options:answers)
                });

            //questionOptionsRepository.GetOptionsForQuestion(this.QuestionnaireIdentity,
            //    questionId, parentQuestionValue, searchFor, this.translation);

            var optionsFromRepository = new List<CategoricalOption>();
            var questionOptionsRepository = new Mock<IQuestionOptionsRepository>();
            questionOptionsRepository.Setup(x =>
                    x.GetOptionsForQuestion(Moq.It.IsAny<IQuestionnaire>(), questionId, null, String.Empty, null))
                .Returns(optionsFromRepository);


            plainQuestionnaire = Create.Entity.PlainQuestionnaire(document: questionnaire, 1, questionOptionsRepository: questionOptionsRepository.Object);

            // Act
            categoricalOptions = plainQuestionnaire.GetOptionsForQuestion(questionId, null, String.Empty);

            // assert
            categoricalOptions.Should().BeSameAs(optionsFromRepository);
        }

        private static PlainQuestionnaire plainQuestionnaire;
        private static Guid questionId;
        private static IEnumerable<CategoricalOption> categoricalOptions;
    }
}
