using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Enumerator.Native.Questionnaire.Impl;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.DataCollection.PlainQuestionnaireTests
{
    internal class when_getting_options_for_combobox
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var answers = new List<Answer>() {new Answer() { AnswerCode = 1, AnswerText = "1" } , new Answer() {AnswerCode = 2, AnswerText = "2"} }; 

            questionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            
            var questionnaire = Create.Entity.QuestionnaireDocument(
                children: new List<IComposite>
                {
                    Create.Entity.SingleQuestion(id:questionId,isFilteredCombobox:true, options:answers)
                });

            plainQuestionnaire = Create.Entity.PlainQuestionnaire(document: questionnaire);

            var questionnaireRepository = Mock.Of<IQuestionnaireStorage>(repository
                => repository.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<string>()) == Create.Entity.PlainQuestionnaire(questionnaire, 1));

            Setup.InstanceToMockedServiceLocator<IQuestionnaireStorage>(questionnaireRepository);
            Setup.InstanceToMockedServiceLocator<IQuestionOptionsRepository>(new QuestionnaireQuestionOptionsRepository());
            BecauseOf();
        }  

        public void BecauseOf() => categoricalOptions = plainQuestionnaire.GetOptionsForQuestion(questionId, null, String.Empty);

        [NUnit.Framework.Test] public void should_find_2_options () =>
            categoricalOptions.Count().Should().Be(2);

        private static PlainQuestionnaire plainQuestionnaire;
        private static Guid questionId;
        private static IEnumerable<CategoricalOption> categoricalOptions;
    }
}
