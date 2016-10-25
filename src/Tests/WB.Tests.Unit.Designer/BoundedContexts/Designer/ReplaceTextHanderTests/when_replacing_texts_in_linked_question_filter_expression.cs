using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.ReplaceTextHanderTests
{
    internal class when_replacing_texts_in_linked_question_filter_expression : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");
            questionnaire = CreateQuestionnaireWithOneGroup(responsibleId: responsibleId,
                groupId: chapterId);

            questionnaire.AddMultiOptionQuestion(filteredQuestionId, chapterId, responsibleId,
                linkedFilterExpression: $"filter with {searchFor}");
        };

        Because of = () => questionnaire.ReplaceTexts(Create.Command.ReplaceTextsCommand(searchFor, replaceWith, userId: responsibleId));

        It should_replace_text_in_filter_expression = () =>
                questionnaire.QuestionnaireDocument.Find<IQuestion>(filteredQuestionId).LinkedFilterExpression.ShouldEqual($"filter with {replaceWith}");

        It should_record_number_of_affected_entities = () =>
                questionnaire.GetLastReplacedEntriesCount().ShouldEqual(1);

        static Questionnaire questionnaire;

        const string searchFor = "to_search";
        const string replaceWith = "replaced";
        static readonly Guid chapterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        static readonly Guid filteredQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        static Guid responsibleId;
    }
}