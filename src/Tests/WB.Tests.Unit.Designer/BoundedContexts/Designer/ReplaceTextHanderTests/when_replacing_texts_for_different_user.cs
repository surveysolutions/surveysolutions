using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.ReplaceTextHanderTests
{
    internal class when_replacing_texts_for_different_user : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            var responsibleId = Guid.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");
            questionnaire = CreateQuestionnaireWithOneGroup(responsibleId: responsibleId,
                groupId: chapterId);

            questionnaire.AddMultiOptionQuestion(questionId, chapterId, responsibleId,
                title: $"filter with {searchFor}");
        };

        Because of = () => exception = Catch.Only<QuestionnaireException>(() => 
                questionnaire.ReplaceTexts(Create.Command.ReplaceTextsCommand(searchFor, replaceWith, userId: Guid.NewGuid())));

        It should_not_allow_to_edit_questionnaire = () => exception.ErrorType.ShouldEqual(DomainExceptionType.DoesNotHavePermissionsForEdit);

        It should_not_change_questionnaire = () => 
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId).QuestionText.ShouldEqual($"filter with {searchFor}");

        static Questionnaire questionnaire;

        const string searchFor = "to_search";
        const string replaceWith = "replaced";
        static readonly Guid chapterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        static readonly Guid questionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        static QuestionnaireException exception;
    }
}