using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Nito.AsyncEx.Synchronous;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Tests.Unit.BoundedContexts.Interviewer.SynchronizationViewModelTests;
using WB.Tests.Unit.SharedKernels.SurveyManagement;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.SynchronizationProcessTests
{
    internal class when_synchronize_and_need_download_missing_attachments : SynchronizationProcessTestsContext
    {
        Establish context = () =>
        {
            var principal = Setup.InterviewerPrincipal("name", "pass");

            var emptyInterviewViews = new List<InterviewView>().ToReadOnlyCollection();
            var interviewViewRepository = new SqliteInmemoryStorage<InterviewView>();

            var newCensusInterviewIdentities = new List<QuestionnaireIdentity>()
            {
                new QuestionnaireIdentity(Guid.NewGuid(), 1),
                new QuestionnaireIdentity(Guid.NewGuid(), 3),
            };

            var attachmentContentIds1 = new List<string>() { "1", "2", "3" };
            var attachmentContentIds2 = new List<string>() { "2", "3", "5" };

            synchronizationService = Mock.Of<ISynchronizationService>(
                x => x.GetCensusQuestionnairesAsync(Moq.It.IsAny<CancellationToken>()) == Task.FromResult(newCensusInterviewIdentities)
                && x.GetInterviewsAsync(Moq.It.IsAny<CancellationToken>()) == Task.FromResult(new List<InterviewApiView>())
                && x.GetQuestionnaireAsync(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<Action<decimal, long, long>>(), Moq.It.IsAny<CancellationToken>()) == Task.FromResult(new QuestionnaireApiView())
                && x.GetAttachmentContentsAsync(newCensusInterviewIdentities[0], Moq.It.IsAny<Action<decimal, long, long>>(), Moq.It.IsAny<CancellationToken>()) == Task.FromResult(attachmentContentIds1)
                && x.GetAttachmentContentsAsync(newCensusInterviewIdentities[1], Moq.It.IsAny<Action<decimal, long, long>>(), Moq.It.IsAny<CancellationToken>()) == Task.FromResult(attachmentContentIds2)
                && x.GetAttachmentContentAsync("1", Moq.It.IsAny<Action<decimal, long, long>>(), Moq.It.IsAny<CancellationToken>()) == Task.FromResult(Create.Entity.AttachmentContent_Enumerator("1"))
                && x.GetAttachmentContentAsync("5", Moq.It.IsAny<Action<decimal, long, long>>(), Moq.It.IsAny<CancellationToken>()) == Task.FromResult(Create.Entity.AttachmentContent_Enumerator("5"))
                && x.GetQuestionnaireTranslationAsync(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<CancellationToken>()) == Task.FromResult(new List<TranslationDto>())
                );

            interviewerQuestionnaireAccessor = Mock.Of<IInterviewerQuestionnaireAccessor>(
                x => x.GetCensusQuestionnaireIdentities() == new List<QuestionnaireIdentity>()
                && x.IsQuestionnaireAssemblyExists(Moq.It.IsAny<QuestionnaireIdentity>()) == true
                );

            attachmentContentStorage = Mock.Of<IAttachmentContentStorage>(
                x => x.Exists("1") == false
                && x.Exists("2") == true
                && x.Exists("3") == true
                && x.Exists("5") == false
                );

            viewModel = CreateSynchronizationProcess(principal: principal,
                interviewViewRepository: interviewViewRepository,
                attachmentContentStorage: attachmentContentStorage,
                synchronizationService: synchronizationService,
                questionnaireFactory: interviewerQuestionnaireAccessor
                );
        };

        Because of = () => viewModel.SyncronizeAsync(new Progress<SyncProgressInfo>(), CancellationToken.None).WaitAndUnwrapException();

        It should_download_attachment_content_for_id_1 = () =>
            Mock.Get(synchronizationService).Verify(s => s.GetAttachmentContentAsync("1", Moq.It.IsAny<Action<decimal, long, long>>(), Moq.It.IsAny<CancellationToken>()), Times.Once());

        It should_download_attachment_content_for_id_2 = () =>
            Mock.Get(synchronizationService).Verify(s => s.GetAttachmentContentAsync("2", Moq.It.IsAny<Action<decimal, long, long>>(), Moq.It.IsAny<CancellationToken>()), Times.Never());

        It should_download_attachment_content_for_id_3 = () =>
            Mock.Get(synchronizationService).Verify(s => s.GetAttachmentContentAsync("3", Moq.It.IsAny<Action<decimal, long, long>>(), Moq.It.IsAny<CancellationToken>()), Times.Never());

        It should_download_attachment_content_for_id_5 = () =>
            Mock.Get(synchronizationService).Verify(s => s.GetAttachmentContentAsync("5", Moq.It.IsAny<Action<decimal, long, long>>(), Moq.It.IsAny<CancellationToken>()), Times.Once());

        It should_store_attachment_content_for_id_1 = () =>
            Mock.Get(attachmentContentStorage).Verify(s => s.Store(Moq.It.Is<AttachmentContent>(ac => ac.Id == "1")), Times.Once());

        It should_store_attachment_content_for_id_2 = () =>
            Mock.Get(attachmentContentStorage).Verify(s => s.Store(Moq.It.Is<AttachmentContent>(ac => ac.Id == "2")), Times.Never());

        It should_store_attachment_content_for_id_3 = () =>
            Mock.Get(attachmentContentStorage).Verify(s => s.Store(Moq.It.Is<AttachmentContent>(ac => ac.Id == "3")), Times.Never());

        It should_store_attachment_content_for_id_5 = () =>
            Mock.Get(attachmentContentStorage).Verify(s => s.Store(Moq.It.Is<AttachmentContent>(ac => ac.Id == "5")), Times.Once());


        static SynchronizationProcess viewModel;
        static IAttachmentContentStorage attachmentContentStorage;
        static ISynchronizationService synchronizationService;
        static IInterviewerQuestionnaireAccessor interviewerQuestionnaireAccessor;
    }
}
