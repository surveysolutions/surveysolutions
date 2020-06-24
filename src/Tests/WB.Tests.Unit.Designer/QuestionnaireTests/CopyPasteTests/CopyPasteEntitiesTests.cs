﻿using System;
using System.Collections.Generic;
using System.Linq;
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

        [Test]
        public void when_copy_group_with_entities_to_cover_page()
        {
            var responsibleId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var questionnaireId = Guid.Parse("DCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var chapterId = Guid.Parse("FCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            

            var questionnaire = CreateQuestionnaireWithOneGroup(questionnaireId: questionnaireId, 
                responsibleId: responsibleId);
            questionnaire.QuestionnaireDocument.Add(
                Create.Chapter(chapterId: chapterId, children: new List<IComposite>
                {
                    Create.TextQuestion(),
                    Create.NumericIntegerQuestion(),
                    Create.MultipleOptionsQuestion(),
                    Create.TextListQuestion(),
                    Create.QRBarcodeQuestion(),
                    Create.StaticText(),
                    
                    Create.Variable(),
                    Create.Group(children: new []{ Create.TextQuestion() }),
                    Create.Roster(children: new []{ Create.TextQuestion() }),
                }), questionnaireId);

            var command = Create.Command.PasteInto(
                questionnaireId: questionnaireId,
                entityId: Guid.NewGuid(), 
                sourceItemId: chapterId,
                responsibleId: responsibleId,
                sourceQuestionnaireId: questionnaireId,
                targetParentId: questionnaire.QuestionnaireDocument.CoverPageSectionId);

            command.SourceDocument = questionnaire.QuestionnaireDocument;
            
            questionnaire.PasteInto(command);

            var cover = questionnaire.QuestionnaireDocument.Children.First();
            Assert.That(cover, Is.Not.Null);
            Assert.That(cover.Children.Count, Is.EqualTo(6));
            Assert.That(cover.Children.Count(e => e is IQuestion), Is.EqualTo(5));
            Assert.That(cover.Children.Count(e => e is IStaticText), Is.EqualTo(1));
            Assert.That(cover.Children.OfType<IQuestion>().Select(q => q.Featured), Is.All.True);
        }

        [Test]
        public void when_copy_question_to_cover_page()
        {
            var responsibleId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var questionnaireId = Guid.Parse("DCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var questionId = Guid.Parse("FCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

            var questionnaire = CreateQuestionnaireWithOneGroup(questionnaireId: questionnaireId, 
                responsibleId: responsibleId);
            questionnaire.QuestionnaireDocument.Add(
                Create.Chapter(children: new List<IComposite>
                {
                    Create.TextQuestion(questionId),
                }), questionnaireId);

            var command = Create.Command.PasteInto(
                questionnaireId: questionnaireId,
                entityId: Guid.NewGuid(), 
                sourceItemId: questionId,
                responsibleId: responsibleId,
                sourceQuestionnaireId: questionnaireId,
                targetParentId: questionnaire.QuestionnaireDocument.CoverPageSectionId);

            command.SourceDocument = questionnaire.QuestionnaireDocument;
            
            questionnaire.PasteInto(command);

            var cover = questionnaire.QuestionnaireDocument.Children.First();
            Assert.That(cover, Is.Not.Null);
            Assert.That(cover.Children.Count, Is.EqualTo(1));
            Assert.That(cover.Children.Count(e => e is IQuestion), Is.EqualTo(1));
            Assert.That(cover.Children.OfType<IQuestion>().Single().Featured, Is.True);
        }
    }
}
