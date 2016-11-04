using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Tests.Unit.SharedKernels.DataCollection.PlainQuestionnaireTests
{
    internal class when_getting_options_for_combobox
    {
        Establish context = () =>
        {
            var answers = new List<Answer>() {new Answer() { AnswerCode = 1, AnswerText = "1" } , new Answer() {AnswerCode = 2, AnswerText = "2"} }; 

            questionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            
            var questionnaire = Create.Entity.QuestionnaireDocument(
                children: new List<IComposite>
                {
                    Create.Entity.SingleQuestion(id:questionId,isFilteredCombobox:true, options:answers)
                });

            plainQuestionnaire = Create.Entity.PlainQuestionnaire(document: questionnaire);
        };  

        Because of = () => categoricalOptions = plainQuestionnaire.GetOptionsForQuestion(questionId, null, String.Empty);

        It should_find_2_options = () =>
            categoricalOptions.Count().ShouldEqual(2);

        private static PlainQuestionnaire plainQuestionnaire;
        private static Guid questionId;
        private static IEnumerable<CategoricalOption> categoricalOptions;
    }
}