using System;
using Moq;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.InterviewerQuestionnaireAccessorTests
{
    internal class when_remove_questionnaire : InterviewerQuestionnaireAccessorTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            interviewerQuestionnaireAccessor = CreateInterviewerQuestionnaireAccessor(
                questionnaireViewRepository: mockOfQuestionnaireViewRepository.Object,
                questionnaireStorage: mockOfPlainQuestionnaireRepository.Object,
                questionnaireAssemblyFileAccessor: mockOfQuestionnaireAssemblyFileAccessor.Object);
            BecauseOf();
        }

        public void BecauseOf() => interviewerQuestionnaireAccessor.RemoveQuestionnaire(questionnaireIdentity);

        [NUnit.Framework.Test] public void should_remove_questionnaire_document_view_from_plain_storage () =>
            mockOfPlainQuestionnaireRepository.Verify(x => x.DeleteQuestionnaireDocument(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version), Times.Once);

        [NUnit.Framework.Test] public void should_remove_questionnaire_view_from_plain_storage () =>
            mockOfQuestionnaireViewRepository.Verify(x => x.Remove(questionnaireIdentity.ToString()), Times.Once);

        [NUnit.Framework.Test] public void should_remove_questionnaire_assembly_from_file_storage () =>
            mockOfQuestionnaireAssemblyFileAccessor.Verify(x => x.RemoveAssembly(questionnaireIdentity), Times.Once);

        private static readonly QuestionnaireIdentity questionnaireIdentity = new QuestionnaireIdentity(Guid.Parse("11111111111111111111111111111111"), 1);
        private static readonly Mock<IQuestionnaireStorage> mockOfPlainQuestionnaireRepository = new Mock<IQuestionnaireStorage>();
        private static readonly Mock<IPlainStorage<QuestionnaireView>> mockOfQuestionnaireViewRepository = new Mock<IPlainStorage<QuestionnaireView>>();
        private static readonly Mock<IQuestionnaireAssemblyAccessor> mockOfQuestionnaireAssemblyFileAccessor = new Mock<IQuestionnaireAssemblyAccessor>();
        private static readonly Mock<IInterviewerInterviewAccessor> mockOfInterviewAccessor = new Mock<IInterviewerInterviewAccessor>();
        private static InterviewerQuestionnaireAccessor interviewerQuestionnaireAccessor;
    }
}
