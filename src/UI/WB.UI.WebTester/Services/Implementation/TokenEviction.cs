using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using WB.Core.Infrastructure.CommandBus;
using WB.Enumerator.Native.WebInterview;

namespace WB.UI.WebTester.Services.Implementation
{
    public class TokenEviction : IEvictionNotifier, IEvictionObservable
    {
        private readonly Subject<Guid> subject;
        private readonly IWebInterviewInvoker webInterviewNotification;
        private readonly IAppdomainsPerInterviewManager appdomainsPerInterviewManager;
        private readonly IQuestionnaireImportService questionnaireImportService;
        private readonly ICacheStorage<List<ICommand>, Guid> executedCommandsStorage;
        private readonly IImportStatusStore importStatusStore;
        private readonly IWebTesterJwtStore jwtStore;
        private readonly IUserContextStore userContextStore;

        public TokenEviction(IWebInterviewInvoker webInterviewNotification,
            IAppdomainsPerInterviewManager appdomainsPerInterviewManager,
            IQuestionnaireImportService questionnaireImportService, 
            ICacheStorage<List<ICommand>, Guid> executedCommandsStorage,
            IImportStatusStore importStatusStore,
            IWebTesterJwtStore jwtStore,
            IUserContextStore userContextStore)
        {
            this.subject = new Subject<Guid>();

            this.webInterviewNotification = webInterviewNotification;
            this.appdomainsPerInterviewManager = appdomainsPerInterviewManager;
            this.questionnaireImportService = questionnaireImportService;
            this.executedCommandsStorage = executedCommandsStorage;
            this.importStatusStore = importStatusStore;
            this.jwtStore = jwtStore;
            this.userContextStore = userContextStore;
        }
        
        public void Evict(Guid token)
        {
            subject.OnNext(token);

            webInterviewNotification.ShutDown(token);
            appdomainsPerInterviewManager.TearDown(token);
            questionnaireImportService.RemoveQuestionnaire(token);
            executedCommandsStorage.Remove(token);
            // Do NOT remove the delegated JWT or user context here. They are TTL-bounded
            // authentication credentials that must survive interview-runtime eviction: the
            // error-recovery paths in ImportQuestionnaireAndCreateInterviewService call Evict and
            // then immediately re-import the questionnaire from Designer, which requires the token.
            // Removing it caused those re-imports (and any Designer call after a mid-run cache
            // eviction) to be sent without an Authorization header, yielding 401 Unauthorized.
            // The credentials self-expire via their absolute cache TTL (the JWT lifetime).

            // Remove the creation-status entry so abandoned / error runs
            // don't accumulate indefinitely in the static dictionary.
            importStatusStore.Remove(token);
        }

        public void Complete(Guid token)
        {
            Evict(token);
            jwtStore.Remove(token);
            userContextStore.Remove(token);
        }

        public IDisposable Subscribe(Action<Guid> action)
        {
            return subject.Subscribe(action);
        }
    }
}
