using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.ReplaceTextHanderTests
{
    internal class when_searching_all_texts_in_questionnaire : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            var responsibleId = Guid.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");
            questionnaire = CreateQuestionnaireWithOneGroup(responsibleId: responsibleId,
                groupId: chapterId);

            questionnaire.AddStaticText(Create.Event.StaticTextAdded(entityId: staticTextId,
                text: $"static text title with {searchFor}",
                parentId: chapterId,
                enablementCondition: $"static text enablement {searchFor}",
                validationConditions: new List<ValidationCondition>
                {
                    Create.ValidationCondition($"st validation exp {searchFor}", message: $"st validation msg {searchFor}"),
                    Create.ValidationCondition($"st validation exp {searchFor}", message: $"st validation msg {searchFor}")
                }));

            questionnaire.AddQuestion(Create.Event.NewQuestionAdded(questionId,
                questionText: $"question title with {searchFor}",
                groupPublicKey: chapterId,
                conditionExpression: $"question enablement {searchFor}",
                validationConditions: new List<ValidationCondition>
                {
                    Create.ValidationCondition($"q validation exp {searchFor}", message: $"q validation msg {searchFor}")
                }));

            questionnaire.AddGroup(Create.Event.NewGroupAddedEvent(groupId.FormatGuid(),
                parentGroupId: chapterId.FormatGuid(),
                groupTitle: $"group title with {searchFor}",
                enablementCondition: $"group enablement {searchFor}"
                ));

            questionnaire.AddMacro(Create.Event.MacroAdded(questionnaire.Id, macroId, responsibleId));
            questionnaire.UpdateMacro(Create.Event.MacroUpdated(questionId, macroId, "macro_name",
                $"macro content {searchFor}", "desc", responsibleId));
        };

        Because of = () => foundReferences = questionnaire.FindAllTexts(searchFor, true, false);

        It should_find_text_in_static_text = () =>
            foundReferences.ShouldContain(x => x.Id == staticTextId && x.Type == QuestionnaireVerificationReferenceType.StaticText);

        It should_find_text_in_question = () =>
            foundReferences.ShouldContain(x => x.Id == questionId && x.Type == QuestionnaireVerificationReferenceType.Question);

        It should_find_text_in_group = () =>
            foundReferences.ShouldContain(x => x.Id == groupId && x.Type == QuestionnaireVerificationReferenceType.Group);

        It should_not_duplicate_found_entities = () =>
            foundReferences.Count(x => x.Id == staticTextId).ShouldEqual(1);

        static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        static Questionnaire questionnaire;

        static readonly Guid staticTextId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        static readonly Guid questionId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        static readonly Guid groupId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        static readonly Guid macroId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
        const string searchFor = "%to replace%";

        private static IEnumerable<QuestionnaireNodeReference> foundReferences;
    }
}