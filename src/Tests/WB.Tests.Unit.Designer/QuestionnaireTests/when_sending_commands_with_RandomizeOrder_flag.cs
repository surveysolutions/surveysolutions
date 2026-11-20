using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.StaticText;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_sending_commands_with_RandomizeOrder_flag : QuestionnaireTestsContext
    {
        private static readonly Guid ResponsibleId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid CoverId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static readonly Guid ChapterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly Guid CoverQuestionId = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid QuestionId = Guid.Parse("33333333333333333333333333333333");
        private static readonly Guid StaticTextId = Guid.Parse("44444444444444444444444444444444");

        private Core.BoundedContexts.Designer.Aggregates.Questionnaire CreateQuestionnaire()
        {
            var document = Create.QuestionnaireDocumentWithCoverPage(Guid.NewGuid(), coverId: CoverId,
                children: new[] { Create.TextQuestion(CoverQuestionId, variable: "cover_q") });
            var questionnaire = Create.Questionnaire(ResponsibleId, document);
            questionnaire.AddGroup(ChapterId, responsibleId: ResponsibleId);
            questionnaire.AddTextQuestion(QuestionId, ChapterId, ResponsibleId, variableName: "q");
            questionnaire.AddStaticText(StaticTextId, ChapterId, ResponsibleId);
            return questionnaire;
        }

        private static UpdateTextQuestion UpdateTextQuestion(Guid questionnaireId, Guid questionId, string variable, bool randomizeOrder)
            => new UpdateTextQuestion(questionnaireId, questionId, ResponsibleId,
                new CommonQuestionParameters { Title = "title", VariableName = variable, RandomizeOrder = randomizeOrder },
                isPreFilled: false, scope: QuestionScope.Interviewer, mask: null, validationConditions: new List<ValidationCondition>());

        [Test]
        public void should_be_false_by_default()
        {
            var questionnaire = CreateQuestionnaire();

            Assert.That(questionnaire.QuestionnaireDocument.Find<IQuestion>(QuestionId).RandomizeOrder, Is.False);
            Assert.That(questionnaire.QuestionnaireDocument.Find<IGroup>(ChapterId).RandomizeOrder, Is.False);
            Assert.That(questionnaire.QuestionnaireDocument.Find<IStaticText>(StaticTextId).RandomizeOrder, Is.False);
        }

        [Test]
        public void should_set_flag_for_question_static_text_and_section()
        {
            var questionnaire = CreateQuestionnaire();

            questionnaire.UpdateTextQuestion(UpdateTextQuestion(questionnaire.Id, QuestionId, "q", true));
            var updateStaticText = Create.Command.UpdateStaticText(questionnaire.Id, StaticTextId, "text", null, ResponsibleId, null);
            updateStaticText.RandomizeOrder = true;
            questionnaire.UpdateStaticText(updateStaticText);
            questionnaire.UpdateGroup(ChapterId, ResponsibleId, "title", "chapter", null, null, null, false, false,
                RosterSizeSourceType.FixedTitles, null, null, RosterDisplayMode.SubSection, randomizeOrder: true);

            Assert.That(questionnaire.QuestionnaireDocument.Find<IQuestion>(QuestionId).RandomizeOrder, Is.True);
            Assert.That(questionnaire.QuestionnaireDocument.Find<IStaticText>(StaticTextId).RandomizeOrder, Is.True);
            Assert.That(questionnaire.QuestionnaireDocument.Find<IGroup>(ChapterId).RandomizeOrder, Is.True);
        }

        [Test]
        public void should_not_set_flag_for_cover_and_its_children()
        {
            var questionnaire = CreateQuestionnaire();

            questionnaire.UpdateTextQuestion(UpdateTextQuestion(questionnaire.Id, CoverQuestionId, "cover_q", true));
            questionnaire.UpdateGroup(CoverId, ResponsibleId, "Cover", null, null, null, null, false, false,
                RosterSizeSourceType.FixedTitles, null, null, RosterDisplayMode.SubSection, randomizeOrder: true);

            Assert.That(questionnaire.QuestionnaireDocument.Find<IQuestion>(CoverQuestionId).RandomizeOrder, Is.False);
            Assert.That(questionnaire.QuestionnaireDocument.Find<IGroup>(CoverId).RandomizeOrder, Is.False);
        }
    }
}
