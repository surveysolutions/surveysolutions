using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Tests.Unit.BoundedContexts.Capi.InterviewViewModelDenormalizerTests
{
    internal class InterviewViewModelDenormalizerTests : InterviewViewModelDenormalizerTestsContext
    {
        [Test]
        public void HandleInterviewForTestingCreated_When_InterviewForTestingCreated_event_is_come_Then_ViewModel_Is_Stored()
        {
            // Arrange
            var questionnaireId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var userId = Guid.Parse("22222222-2222-2222-2222-222222222222");

            var questionnaireDocument = CreateQuestionnaireDocument(questionnaireId);

            InterviewForTestingCreated evnt = new InterviewForTestingCreated(userId, questionnaireId, 1);

            var interviewViewModelStub = new Mock<IReadSideRepositoryWriter<InterviewViewModel>>();
            
            var questionnaireDocumentVersionedStub = CreateQuestionnaireDocumentVersionedStorageStub(questionnaireDocument);

            var denormalizer = CreateInterviewViewModelDenormalizer(interviewViewModelStub, questionnaireDocumentVersionedStub);
            
            //Act
            denormalizer.Handle(CreatePublishedEvent(questionnaireId, evnt));

            //Assert
            interviewViewModelStub.Verify(
                x => x.Store(It.Is<InterviewViewModel>(i => i.PublicKey == questionnaireId), questionnaireId.FormatGuid()));
        }

        [Test]
        public void HandleSynchronizationMetadataApplied_When_SynchronizationMetadataApplied_event_is_come_Then_ViewModel_Is_Removed()
        {
            // Arrange
            var questionnaireId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var userId = Guid.Parse("22222222-2222-2222-2222-222222222222");

            var questionnaireDocument = CreateQuestionnaireDocument(questionnaireId);

            var evnt = new SynchronizationMetadataApplied(userId, questionnaireId,1, InterviewStatus.RejectedBySupervisor, null, false, null);
            
            var interviewViewModelStub = new Mock<IReadSideRepositoryWriter<InterviewViewModel>>();

            var questionnaireDocumentVersionedStub = CreateQuestionnaireDocumentVersionedStorageStub(questionnaireDocument);

            var denormalizer = CreateInterviewViewModelDenormalizer(interviewViewModelStub, questionnaireDocumentVersionedStub);

            //Act
            denormalizer.Handle(CreatePublishedEvent(questionnaireId, evnt));

            //Assert
            interviewViewModelStub.Verify(
                x => x.Remove(It.Is<string>(i => i == questionnaireId.FormatGuid())));
        }

        [Test]
        public void HandleTextListQuestionAnswered_When_TextListQuestionAnsweredApplied_then_AnswerList_is_updated()
        {
            // Arrange
            var questionnaireId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var userId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var questionId = Guid.Parse("11111111-3333-3333-3333-111111111111");
            decimal[] emptyRosterVector = new decimal[] {};
            var textListAnswer = new[] { Tuple.Create((decimal) 1, "one")};
            var groupId = Guid.Parse("22222222-3333-2222-2222-222222222222");

            var textListQuestion = new TextListQuestion("TextList") { PublicKey = questionId , QuestionType = QuestionType.TextList}; 
            var questionnaireDocument = CreateQuestionnaireDocument(questionnaireId);
            var group = new Group("group") { PublicKey = groupId };
            group.Children.Add(textListQuestion);
            questionnaireDocument.Children.Add(group);

            var evnt = new TextListQuestionAnswered(userId, questionId, emptyRosterVector, DateTime.Now, textListAnswer);

            InterviewViewModel interviewViewModel = 
                new InterviewViewModel(questionnaireId, questionnaireDocument, new QuestionnaireRosterStructure());

            var interviewViewModelStub = CreateInterviewViewModelDenormalizerStorageStub(interviewViewModel);
            var questionnaireDocumentVersionedStub = CreateQuestionnaireDocumentVersionedStorageStub(questionnaireDocument);

            var denormalizer = CreateInterviewViewModelDenormalizer(interviewViewModelStub, questionnaireDocumentVersionedStub);

            //Act
            denormalizer.Handle(CreatePublishedEvent(questionnaireId, evnt));

            //Assert
            var questionToCheck = ((TextListQuestionViewModel) interviewViewModel.FindQuestion(
                question => question.PublicKey == new InterviewItemId(questionId, new decimal[] {}))
                .FirstOrDefault());

            Assert.That(textListAnswer[0].Item2, Is.EqualTo(questionToCheck.ListAnswers.FirstOrDefault(a => a.Value == textListAnswer[0].Item1).Answer));

        }

    }
}
