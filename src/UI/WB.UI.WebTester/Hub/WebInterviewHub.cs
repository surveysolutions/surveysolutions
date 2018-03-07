using System;
using Microsoft.AspNet.SignalR.Hubs;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Models;
using WB.UI.WebTester.Services;

namespace WB.UI.WebTester.Hub
{
    [HubName(@"interview")]
    public class WebInterviewHub : WebInterview
    {
        private readonly IEvictionNotifier evictionNotify;

        public WebInterviewHub(IStatefulInterviewRepository statefulInterviewRepository, 
            ICommandService commandService, 
            IQuestionnaireStorage questionnaireRepository, 
            IWebInterviewNotificationService webInterviewNotificationService, 
            IEvictionNotifier evictionNotify,
            IWebInterviewInterviewEntityFactory interviewEntityFactory,
            IInterviewBrokenPackagesService interviewBrokenPackagesService) : 
            base(statefulInterviewRepository, commandService, questionnaireRepository, webInterviewNotificationService, interviewEntityFactory, interviewBrokenPackagesService)
            IEvictionNotifier evictionNotify, 
            IImageFileStorage imageFileStorage,
            IAudioFileStorage audioFileStorage) : 
            base(statefulInterviewRepository, commandService, questionnaireRepository, 
                webInterviewNotificationService, interviewEntityFactory, imageFileStorage, 
                audioFileStorage)
        {
            this.evictionNotify = evictionNotify;
        }

        public override void CompleteInterview(CompleteInterviewRequest completeInterviewRequest)
        {
            var interviewId = Guid.Parse(this.CallerInterviewId);
            evictionNotify.Evict(interviewId);
        }
    }
}
