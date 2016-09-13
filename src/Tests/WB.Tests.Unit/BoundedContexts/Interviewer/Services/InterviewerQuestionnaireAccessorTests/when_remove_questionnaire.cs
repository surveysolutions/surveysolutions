using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using Nito.AsyncEx.Synchronous;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Tests.Unit.SharedKernels.SurveyManagement;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.InterviewerQuestionnaireAccessorTests
{
    internal class when_remove_questionnaire : InterviewerQuestionnaireAccessorTestsContext
    {
        Establish context = () =>
        {
            var interviewsAsyncPlainStorage =
                new SqliteInmemoryStorage<InterviewView>();
            interviewsAsyncPlainStorage.Store(
            new[]
                {
                    new InterviewView
                    {
                        QuestionnaireId = questionnaireIdentity.ToString(),
                        InterviewId = Guid.Parse("22222222222222222222222222222222"),
                        Id = Guid.Parse("22222222222222222222222222222222").FormatGuid()
                    },
                    new InterviewView
                    {
                        QuestionnaireId = questionnaireIdentity.ToString(),
                        InterviewId = Guid.Parse("33333333333333333333333333333333"),
                        Id = Guid.Parse("33333333333333333333333333333333").FormatGuid()
                    },
                });

            interviewerQuestionnaireAccessor = CreateInterviewerQuestionnaireAccessor(
                questionnaireViewRepository: mockOfQuestionnaireViewRepository.Object,
                questionnaireStorage: mockOfPlainQuestionnaireRepository.Object,
                questionnaireAssemblyFileAccessor: mockOfQuestionnaireAssemblyFileAccessor.Object,
                interviewViewRepository: interviewsAsyncPlainStorage,
                interviewFactory: mockOfInterviewAccessor.Object);
        };

        Because of = async () =>
            await interviewerQuestionnaireAccessor.RemoveQuestionnaireAsync(questionnaireIdentity);

        It should_remove_questionnaire_document_view_from_plain_storage = () =>
            mockOfPlainQuestionnaireRepository.Verify(x => x.DeleteQuestionnaireDocument(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version), Times.Once);

        It should_remove_questionnaire_view_from_plain_storage = () =>
            mockOfQuestionnaireViewRepository.Verify(x => x.Remove(questionnaireIdentity.ToString()), Times.Once);

        It should_remove_questionnaire_assembly_from_file_storage = () =>
            mockOfQuestionnaireAssemblyFileAccessor.Verify(x => x.RemoveAssemblyAsync(questionnaireIdentity), Times.Once);

        It should_remove_interviews_by_questionnaire_from_plain_storage = () =>
            mockOfInterviewAccessor.Verify(x => x.RemoveInterview(Moq.It.IsAny<Guid>()), Times.Exactly(2));

        private static readonly QuestionnaireIdentity questionnaireIdentity = new QuestionnaireIdentity(Guid.Parse("11111111111111111111111111111111"), 1);
        private static readonly Mock<IQuestionnaireStorage> mockOfPlainQuestionnaireRepository = new Mock<IQuestionnaireStorage>();
        private static readonly Mock<IPlainStorage<QuestionnaireView>> mockOfQuestionnaireViewRepository = new Mock<IPlainStorage<QuestionnaireView>>();
        private static readonly Mock<IQuestionnaireAssemblyFileAccessor> mockOfQuestionnaireAssemblyFileAccessor = new Mock<IQuestionnaireAssemblyFileAccessor>();
        private static readonly Mock<IInterviewerInterviewAccessor> mockOfInterviewAccessor = new Mock<IInterviewerInterviewAccessor>();
        private static InterviewerQuestionnaireAccessor interviewerQuestionnaireAccessor;
    }
}
