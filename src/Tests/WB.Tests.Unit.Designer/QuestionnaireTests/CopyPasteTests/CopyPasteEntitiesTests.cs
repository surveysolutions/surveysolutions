using System;
using System.Collections.Generic;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.CopyPasteTests
{
    [TestFixture]
    internal class CopyPasteEntitiesTests : QuestionnaireTestsContext
    {
        [Test]
        public void when_copy_linked_question_with_option_filter()
        {
            var responsibleId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var groupToPasteInId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var sourceQuestionaireId = Guid.Parse("DCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var questionId = Id.g7;
            var targetQuestionId = Id.g8;
            var targetQuestionnaireId = Guid.Parse("DCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCE");

            var targetQuestionnaire = CreateQuestionnaireWithOneGroup(questionnaireId: targetQuestionnaireId, 
                groupId: groupToPasteInId, 
                responsibleId: responsibleId);

            var sourceDocument = Create.QuestionnaireDocument(sourceQuestionaireId,
                Create.Chapter(children: new List<IComposite>
                {
                    Create.MultipleOptionsQuestion(questionId, optionsFilterExpression: "num == 7")
                }));

            var command = Create.Command.PasteInto(
                questionnaireId: targetQuestionnaireId,
                entityId: targetQuestionId,
                sourceItemId: questionId,
                responsibleId: responsibleId,
                sourceQuestionnaireId: sourceQuestionaireId,
                targetParentId: groupToPasteInId);

            command.SourceDocument = sourceDocument;
            
            targetQuestionnaire.PasteInto(command);

            var question = targetQuestionnaire.QuestionnaireDocument.Find<IQuestion>(targetQuestionId);
            Assert.That(question, Is.Not.Null);
            Assert.That(question.PublicKey, Is.EqualTo(targetQuestionId));
            Assert.That(question.Properties.OptionsFilterExpression, Is.EqualTo("num == 7"));
        }
    }
}
