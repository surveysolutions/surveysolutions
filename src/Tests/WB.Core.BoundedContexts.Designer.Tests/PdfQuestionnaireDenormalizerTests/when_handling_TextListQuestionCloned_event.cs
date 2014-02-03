﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Designer.Tests.PdfQuestionnaireDenormalizerTests
{
    internal class when_handling_TextListQuestionCloned_event : PdfQuestionnaireDenormalizerTestContext
    {
        Establish context = () =>
        {
            var parentGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            @event = CreatePublishedEvent(new TextListQuestionCloned() { PublicKey = questionId, GroupId = parentGroupId, QuestionText = questionTitle });

            pdfQuestionnaireDocument = CreatePdfQuestionnaire(CreateGroup(parentGroupId));

            var documentStorage = Mock.Of<IReadSideRepositoryWriter<PdfQuestionnaireView>>(writer => writer.GetById(Moq.It.IsAny<Guid>()) == pdfQuestionnaireDocument);

            denormalizer = CreatePdfQuestionnaireDenormalizer(documentStorage: documentStorage);
        };

        Because of = () =>
            denormalizer.Handle(@event);

        It should_set_TextList_question_type_to_QuestionType_TextList = () =>
            pdfQuestionnaireDocument.GetQuestion(questionId).QuestionType.ShouldEqual(QuestionType.TextList);

        It should_set_TextList_question_title_be_equal_to_passed_title = () =>
            pdfQuestionnaireDocument.GetQuestion(questionId).Title.ShouldEqual(questionTitle);

        private static PdfQuestionnaireDenormalizer denormalizer;
        private static IPublishedEvent<TextListQuestionCloned> @event;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static string questionTitle = "someTitle";
        private static PdfQuestionnaireView pdfQuestionnaireDocument;
    }
}
