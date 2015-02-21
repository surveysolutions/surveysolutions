using System;
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

namespace WB.Tests.Unit.BoundedContexts.Designer.PdfQuestionnaireDenormalizerTests
{
    internal class when_handling_TextListQuestionChanged_event : PdfQuestionnaireDenormalizerTestContext
    {
        Establish context = () =>
        {
            var parentGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            @event = CreatePublishedEvent(new TextListQuestionChanged() { PublicKey = questionId, QuestionText = questionTitle });

            pdfQuestionnaireDocument = CreatePdfQuestionnaire(CreateGroup(parentGroupId,children:new List<PdfEntityView>{CreateQuestion(questionId,"some title",QuestionType.TextList)}));

            var documentStorage = Mock.Of<IReadSideKeyValueStorage<PdfQuestionnaireView>>(writer => writer.GetById(Moq.It.IsAny<string>()) == pdfQuestionnaireDocument);

            denormalizer = CreatePdfQuestionnaireDenormalizer(documentStorage: documentStorage);
        };

        Because of = () =>
            denormalizer.Handle(@event);

        It should_set_TextList_question_type_to_QuestionType_TextList = () =>
            pdfQuestionnaireDocument.GetEntityById<PdfQuestionView>(questionId).QuestionType.ShouldEqual(QuestionType.TextList);

        It should_set_TextList_question_title_be_equal_to_passed_title = () =>
            pdfQuestionnaireDocument.GetEntityById<PdfQuestionView>(questionId).Title.ShouldEqual(questionTitle);

        private static PdfQuestionnaireDenormalizer denormalizer;
        private static IPublishedEvent<TextListQuestionChanged> @event;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static string questionTitle = "new Title";
        private static PdfQuestionnaireView pdfQuestionnaireDocument;
    }
}
