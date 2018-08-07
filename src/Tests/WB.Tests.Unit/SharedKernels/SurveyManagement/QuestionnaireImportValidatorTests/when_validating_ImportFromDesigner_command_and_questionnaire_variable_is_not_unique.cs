using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionnaireImportValidatorTests
{
    internal class QuestionnaireNameValidatorTests 
    {
        [Test] 
        public void when_questionnaire_variable_is_not_unique() {
            var variable = "qvar";

            var questionnaireBrowseItemStorage = Setup.QuestionnaireBrowseItemRepository(
                Create.Entity.QuestionnaireBrowseItem(variable: variable, questionnaireId: Id.g2),
                Create.Entity.QuestionnaireBrowseItem(variable: null, questionnaireId: Id.g3)
                );

            var validator = Create.Service.QuestionnaireNameValidator(questionnaireBrowseItemStorage);

            var exception = Assert.Throws<QuestionnaireException>(() 
                => validator.Validate(null, Create.Command.ImportFromDesigner(variable: variable, questionnaireId: Id.g1)));

            Assert.That(exception.Message, Is.EqualTo(string.Format(CommandValidatorsMessages.QuestionnaireVariableUniqueFormat, variable)));
        }

        [Test] 
        public void when_questionnaire_variable_is_unique() {
            var questionnaireBrowseItemStorage = Setup.QuestionnaireBrowseItemRepository(
                Create.Entity.QuestionnaireBrowseItem(variable: "another_var", questionnaireId: Id.g2));

            var validator = Create.Service.QuestionnaireNameValidator(questionnaireBrowseItemStorage);

            Assert.DoesNotThrow(() => validator.Validate(null, Create.Command.ImportFromDesigner(variable: "qvar", questionnaireId: Id.g1)));
        }

        [Test] 
        public void when_questionnaire_variable_is_not_unique_but_questionnaire_is_deleted() {
            var questionnaireBrowseItemStorage = Setup.QuestionnaireBrowseItemRepository(
                Create.Entity.QuestionnaireBrowseItem(variable: "qvar", questionnaireId: Id.g2, deleted: true));

            var validator = Create.Service.QuestionnaireNameValidator(questionnaireBrowseItemStorage);

            Assert.DoesNotThrow(() => validator.Validate(null, Create.Command.ImportFromDesigner(variable: "qvar", questionnaireId: Id.g1)));
        }
    }
}
