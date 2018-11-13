using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels
{
    [TestOf(typeof(PdfViewModel))]
    public class PdfViewModelTests
    {
        [Test]
        public void should_setup_view_model()
        {
            var staticTextIdentity = Create.Identity(Id.gB, 1);
            var location = "location";
            var attachmentContentId = "hash";

            var questionnaire = new Mock<IQuestionnaire>();
            questionnaire.Setup(x => x.GetAttachmentForEntity(staticTextIdentity.Id))
                .Returns(Create.Entity.Attachment(attachmentContentId));

            var questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire.Object);
            var interview = Create.AggregateRoot.StatefulInterview(questionnaireRepository: questionnaireRepository);
            var statefulInterviewRepository = Setup.StatefulInterviewRepository(interview);

            IAttachmentContentStorage attachmentContentStorage = Mock.Of<IAttachmentContentStorage>(x => x.GetFileCacheLocation(attachmentContentId) == location);

            // Act
            var viewModel = new PdfViewModel(statefulInterviewRepository, questionnaireRepository, 
                attachmentContentStorage, Create.ViewModel.DynamicTextViewModel(interviewRepository: statefulInterviewRepository));
            viewModel.ShouldLogInpc(false);
            viewModel.Configure(interview.Id.FormatGuid(), staticTextIdentity);

            Assert.That(viewModel.AttachmentPath, Is.EqualTo(location));
        }
    }
}
