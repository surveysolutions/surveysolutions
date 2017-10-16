using System;
using WB.Core.BoundedContexts.Designer.Resources;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;
using QuestionnaireException = WB.Core.BoundedContexts.Designer.Exceptions.QuestionnaireException;

namespace WB.Tests.Unit.Designer.QuestionnaireTests.CascadigOptions
{
    internal class when_updating_cascading_combobox_options_and_1_option_not_number : QuestionnaireTestsContext
    {
        [Test]
        public void should_throw_QuestionnaireException()
        {
            Guid questionId = Id.g1;
            Guid parentQuestionId = Id.g2;
            Guid chapterId = Id.gC;
            Guid responsibleId = Id.gD;

            var questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId: responsibleId);
            questionnaire.AddSingleOptionQuestion(
                parentQuestionId,
                chapterId,
                title: "text",
                variableName: "var",

                responsibleId: responsibleId,
                options: new Option[]
                {
                    new Option() { Title= "Option 1", Value = "1" },
                    new Option(){ Title = "Option 2", Value = "2" }
                }
            );

            questionnaire.AddSingleOptionQuestion(
                questionId,
                chapterId,
                title: "text",
                variableName: "q2",
                isFilteredCombobox: false,
                responsibleId: responsibleId,
                cascadeFromQuestionId: parentQuestionId
            );

            var exception = Assert.Throws<QuestionnaireException>(() =>
                questionnaire.UpdateCascadingComboboxOptions(questionId: questionId, responsibleId: responsibleId,
                    options: new[]
                    {
                        new Option(Guid.NewGuid(), "1", "Option 1"),
                        new Option(Guid.NewGuid(), "not number value", "Option 2")
                    }));
            Assert.That(exception.Message, Is.EqualTo(ExceptionMessages.OptionValuesShouldBeNumbers));
        }
    }
}