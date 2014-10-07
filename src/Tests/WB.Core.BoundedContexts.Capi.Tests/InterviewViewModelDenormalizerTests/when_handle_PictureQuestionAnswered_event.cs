using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Capi.EventHandler;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Capi.Tests.InterviewViewModelDenormalizerTests
{
    internal class when_handle_PictureQuestionAnswered_event : InterviewViewModelDenormalizerTestsContext
    {
        Establish context = () =>
        {
            var questionnaireDocument = CreateQuestionnaireDocument(questionnaireId);
            questionnaireDocument.Children.Add(new Group()
            {
                Children = new List<IComposite>() { new MultimediaQuestion() { PublicKey = pictureQuestionId } }
            });

            evnt = new PictureQuestionAnswered(userId: userId, questionId: pictureQuestionId, propagationVector: new decimal[0],
                answerTime: DateTime.Now, pictureFileName: answerforPictureQuestion);

            interviewViewModel =
                new InterviewViewModel(questionnaireId, questionnaireDocument, new QuestionnaireRosterStructure());

            var interviewViewModelStub = CreateInterviewViewModelDenormalizerStorageStub(interviewViewModel);
            var questionnaireDocumentVersionedStub = CreateQuestionnaireDocumentVersionedStorageStub(questionnaireDocument);

            denormalizer = CreateInterviewViewModelDenormalizer(interviewViewModelStub, questionnaireDocumentVersionedStub);
        };

        Because of = () =>
            denormalizer.Handle(CreatePublishedEvent(questionnaireId, evnt));

        It should_set_qr_barcode_view_model_answer_to_specified_value = () =>
            interviewViewModel.FindQuestion(question => question.PublicKey == new InterviewItemId(pictureQuestionId, new decimal[] { }))
                .FirstOrDefault()
                .AnswerString.ShouldEqual(answerforPictureQuestion);

        private static InterviewViewModel interviewViewModel;
        private static PictureQuestionAnswered evnt;
        private static InterviewViewModelDenormalizer denormalizer;
        private static Guid questionnaireId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static Guid userId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        private static Guid pictureQuestionId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        private static string answerforPictureQuestion = "answer.jpg";
    }
}
