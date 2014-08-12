using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests;

namespace WB.Core.BoundedContexts.Designer.Tests.CloneGpsCoordinatesQuestionHandlerTests
{
    internal class when_cloning_gps_coordinates_question_and_title_contains_substitution_to_question_with_not_supported_type : QuestionnaireTestsContext
    {
        private Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(new NewQuestionAdded()
            {
                PublicKey = Guid.NewGuid(),
                GroupPublicKey = chapterId,
                QuestionType = QuestionType.GpsCoordinates,
                StataExportCaption = substitutionVariableName
            });
            questionnaire.Apply(new QRBarcodeQuestionAdded()
            {
                QuestionId = sourceQuestionId,
                ParentGroupId = chapterId,
                Title = "old title",
                VariableName = "old_variable_name",
                IsMandatory = false,
                Instructions = "old instructions",
                EnablementCondition = "old condition",
                ResponsibleId = responsibleId
            });
        };

        private Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.CloneGpsCoordinatesQuestion(
                    questionId: questionId,
                    title: titleWithSubstitution,
                    variableName: variableName,
                variableLabel: null,
                    isMandatory: isMandatory,
                    enablementCondition: enablementCondition,
                    instructions: instructions,
                    parentGroupId: parentGroupId,
                    sourceQuestionId: sourceQuestionId,
                    targetIndex: targetIndex,
                    responsibleId: responsibleId));

        private It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        private It should_throw_exception_with_message_containting__title___constains__substitution__with__illegal__type__ = () =>
            new[] { "title", "contains", "substitution", "illegal", "type" }.ShouldEachConformTo(
                keyword => exception.Message.ToLower().Contains(keyword));


        private static Questionnaire questionnaire;
        private static Exception exception;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid sourceQuestionId = Guid.Parse("22222222222222222222222222222222");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private const string substitutionVariableName = "substitution_var";
        private static string titleWithSubstitution = string.Format("title with substitution to - %{0}%", substitutionVariableName);
        private static Guid parentGroupId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static string variableName = "datetime_question";
        private static bool isMandatory = true;
        private static string instructions = "intructions";
        private static int targetIndex = 1;
        private static string enablementCondition = null;
    }
}