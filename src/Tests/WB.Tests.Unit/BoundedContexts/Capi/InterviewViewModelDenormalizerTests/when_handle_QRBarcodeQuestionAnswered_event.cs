using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Capi.EventHandler;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Tests.Unit.BoundedContexts.Capi.InterviewViewModelDenormalizerTests
{
    internal class when_handle_QRBarcodeQuestionAnswered_event : InterviewViewModelDenormalizerTestsContext
    {
        Establish context = () =>
        {
            var questionnaireDocument = CreateQuestionnaireDocument(questionnaireId);
            questionnaireDocument.Children.Add(new Group()
            {
                Children = new List<IComposite>() { new QRBarcodeQuestion() { PublicKey = qrBarcodeQuestionId } }
            });

            evnt = new QRBarcodeQuestionAnswered(userId: userId, questionId: qrBarcodeQuestionId, propagationVector: new decimal[0],
                answerTime: DateTime.Now, answer: answerforQRBarcodeQuestion);

            interviewViewModel =
                new InterviewViewModel(questionnaireId, questionnaireDocument, new QuestionnaireRosterStructure());

            var interviewViewModelStub = CreateInterviewViewModelDenormalizerStorageStub(interviewViewModel);
            var questionnaireDocumentVersionedStub = CreateQuestionnaireDocumentVersionedStorageStub(questionnaireDocument);

             denormalizer = CreateInterviewViewModelDenormalizer(interviewViewModelStub, questionnaireDocumentVersionedStub);
        };

        Because of = () =>
            denormalizer.Handle(CreatePublishedEvent(questionnaireId, evnt));

        It should_set_qr_barcode_view_model_answer_to_specified_value = () =>
            interviewViewModel.FindQuestion(question => question.PublicKey == new InterviewItemId(qrBarcodeQuestionId, new decimal[] { }))
                .FirstOrDefault()
                .AnswerString.ShouldEqual(answerforQRBarcodeQuestion);

        private static InterviewViewModel interviewViewModel;
        private static QRBarcodeQuestionAnswered evnt;
        private static InterviewViewModelDenormalizer denormalizer;
        private static Guid questionnaireId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static Guid userId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        private static Guid qrBarcodeQuestionId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        private static string answerforQRBarcodeQuestion = "answer for qr barcode question";
    }
}
