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
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Tests.Unit.SharedKernels.SurveyManagement;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.SynchronizationProcessTests
{
    internal class when_questionniare_has_translations : SynchronizationProcessTestsContext
    {
        internal class when_synchronize_and_need_download_missing_attachments : SynchronizationProcessTestsContext
        {
            Establish context = () =>
            {
                var principal = Setup.InterviewerPrincipal("name", "pass");

                translationsStorage = new InMemoryAsyncPlainStorage<TranslationInstance>();
                var interviewViewRepository = new SqliteInmemoryStorage<InterviewView>();

                var newCensusInterviewIdentities = new List<QuestionnaireIdentity>()
                {
                    new QuestionnaireIdentity(Guid.NewGuid(), 1),
                };

                List<TranslationDto> translations = new List<TranslationDto>();
                translations.Add(new TranslationDto());

                synchronizationService = Mock.Of<ISynchronizationService>(
                    x => x.GetCensusQuestionnairesAsync(Moq.It.IsAny<CancellationToken>()) == Task.FromResult(newCensusInterviewIdentities)
                    && x.GetInterviewsAsync(Moq.It.IsAny<CancellationToken>()) == Task.FromResult(new List<InterviewApiView>())
                    && x.GetQuestionnaireAsync(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<Action<decimal, long, long>>(), Moq.It.IsAny<CancellationToken>()) == Task.FromResult(new QuestionnaireApiView())
                    && x.GetQuestionnaireTranslationAsync(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<CancellationToken>()) == Task.FromResult(translations)
                    );

                interviewerQuestionnaireAccessor = Mock.Of<IInterviewerQuestionnaireAccessor>(
                    x => x.GetCensusQuestionnaireIdentities() == new List<QuestionnaireIdentity>()
                    && x.IsQuestionnaireAssemblyExists(Moq.It.IsAny<QuestionnaireIdentity>()) == true
                    );

                viewModel = CreateSynchronizationProcess(principal: principal,
                    interviewViewRepository: interviewViewRepository,
                    synchronizationService: synchronizationService,
                    questionnaireFactory: interviewerQuestionnaireAccessor,
                    translations: translationsStorage
                    );
            };

            Because of = () => viewModel.SyncronizeAsync(new Progress<SyncProgressInfo>(), CancellationToken.None).WaitAndUnwrapException();

            It should_store_translations = () =>
                translationsStorage.inMemroyStorage.Count.ShouldEqual(1);

            static SynchronizationProcess viewModel;
            static ISynchronizationService synchronizationService;
            static IInterviewerQuestionnaireAccessor interviewerQuestionnaireAccessor;
            static InMemoryAsyncPlainStorage<TranslationInstance> translationsStorage;
        }
    }
}