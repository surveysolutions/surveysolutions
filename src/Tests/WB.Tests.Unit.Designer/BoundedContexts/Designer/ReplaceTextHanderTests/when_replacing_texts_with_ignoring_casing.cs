using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.ReplaceTextHanderTests
{
    internal class when_replacing_texts_with_ignore_casing : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");
            questionnaire = CreateQuestionnaireWithOneGroup(responsibleId: responsibleId,
                groupId: chapterId);

            questionnaire.AddTextQuestion(
                questionId,
                responsibleId:responsibleId,
                title: $"question title with {searchFor}",
                parentId: chapterId,
                enablementCondition: $"question enablement {searchFor}",
                validationConditions: new List<ValidationCondition>
                {
                    Create.ValidationCondition($"q validation exp {searchFor}", message: $"q validation msg {searchFor}")
                });

            command = Create.Command.ReplaceTextsCommand(searchFor.ToLower(), replaceWith, userId: responsibleId);
        };

        Because of = () => questionnaire.ReplaceTexts(command);

        It should_replace_text_in_title_ignoring_casing = () => 
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId).GetTitle().ShouldEqual($"question title with {replaceWith}");

        static readonly Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        static Questionnaire questionnaire;

        static readonly Guid questionId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        static ReplaceTextsCommand command;
        private static Guid responsibleId;
        const string replaceWith = "replaCed";
        const string searchFor = "%To Replace%";
    }
}