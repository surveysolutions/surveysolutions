using System;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Resources;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.QuestionnaireTests
{
    class when_updating_options_for_filtered_combo_and_one_option_is_not_number : QuestionnaireTestsContext
    {
        [Test]
        public void should_throw_exception()
        {
            Guid responsibleId = Id.g1;
            Guid chapterId = Id.gA;
            Guid questionId = Id.gB;

            var questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId: responsibleId);
            questionnaire.AddSingleOptionQuestion(
                questionId,
                chapterId,
                responsibleId,
                title: "text",
                variableName: "var",
                isFilteredCombobox: true);

            var exception = Assert.Throws<QuestionnaireException>(() =>
                questionnaire.UpdateFilteredComboboxOptions(questionId: questionId, responsibleId: responsibleId,
                    options: new[]
                    {
                        new Option(Guid.NewGuid(), "1", "Option 1"),
                        new Option(Guid.NewGuid(), "not number value", "Option 2")
                    }));

            Assert.That(exception.Message, Is.EqualTo(ExceptionMessages.OptionValuesShouldBeNumbers));
        }
    }
}
