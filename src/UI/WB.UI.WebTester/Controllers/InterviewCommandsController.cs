using System;
using System.Web.Http;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Controllers;
using WB.Enumerator.Native.WebInterview.Models;
using WB.UI.WebTester.Services;

namespace WB.UI.WebTester.Controllers
{
    public class InterviewCommandsController : CommandsController
    {
        private readonly IEvictionNotifier evictionNotify;

        public InterviewCommandsController(ICommandService commandService, IImageFileStorage imageFileStorage, IAudioFileStorage audioFileStorage, 
            IQuestionnaireStorage questionnaireRepository, IStatefulInterviewRepository statefulInterviewRepository, 
            IWebInterviewNotificationService webInterviewNotificationService, IEvictionNotifier evictionNotify) 
            : base(commandService, imageFileStorage, audioFileStorage, questionnaireRepository, statefulInterviewRepository, webInterviewNotificationService)
        {
            this.evictionNotify = evictionNotify;
        }

        public override IHttpActionResult CompleteInterview(CompleteInterviewRequest completeInterviewRequest)
        {
            evictionNotify.Evict(completeInterviewRequest.InterviewId);
            return Ok();
        }
    }
}
