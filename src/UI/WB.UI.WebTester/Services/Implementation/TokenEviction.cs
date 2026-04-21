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
        private readonly IWebTesterJwtStore jwtStore;
        private readonly IUserContextStore userContextStore;

        public TokenEviction(IWebInterviewInvoker webInterviewNotification,
            IAppdomainsPerInterviewManager appdomainsPerInterviewManager,
            IQuestionnaireImportService questionnaireImportService, 
            ICacheStorage<List<ICommand>, Guid> executedCommandsStorage,
            IWebTesterJwtStore jwtStore,
            IUserContextStore userContextStore)
        {
            this.subject = new Subject<Guid>();

            this.webInterviewNotification = webInterviewNotification;
            this.appdomainsPerInterviewManager = appdomainsPerInterviewManager;
            this.questionnaireImportService = questionnaireImportService;
            this.executedCommandsStorage = executedCommandsStorage;
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
            jwtStore.Remove(token);
            userContextStore.Remove(token);
        }

        public IDisposable Subscribe(Action<Guid> action)
        {
            return subject.Subscribe(action);
        }
    }
}
