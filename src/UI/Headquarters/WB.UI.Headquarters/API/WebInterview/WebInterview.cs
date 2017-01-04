using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.UI.Shared.Web.CommandDeserialization;

namespace WB.UI.Headquarters.API.WebInterview
{
    [HubName(@"interview")]
    public partial class WebInterview : Hub
    {
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private readonly ICommandDeserializer commandDeserializer;
        private readonly ICommandService commandService;
        private readonly ILogger logger;
        private readonly IUserViewFactory usersRepository;

        private string interviewId;
        private IStatefulInterview CurrentInterview => this.statefulInterviewRepository.Get(this.interviewId);

        public WebInterview(
            IStatefulInterviewRepository statefulInterviewRepository, 
            ICommandDeserializer commandDeserializer,
            ICommandService commandService,
            ILogger logger,
            IUserViewFactory usersRepository)
        {
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.commandDeserializer = commandDeserializer;
            this.commandService = commandService;
            this.logger = logger;
            this.usersRepository = usersRepository;
        }

        public void CreateInterview(string questionnaireId, string interviewerName)
        {
            var questionnaireIdentity = QuestionnaireIdentity.Parse(questionnaireId);

            var interviewer = this.usersRepository.Load(new UserViewInputModel(UserName: interviewerName, UserEmail: null));

            var createInterviewOnClientCommand = new CreateInterviewOnClientCommand(Guid.NewGuid(),
                interviewer.PublicKey, questionnaireIdentity, DateTime.UtcNow,
                interviewer.Supervisor.Id);

            this.commandService.Execute(createInterviewOnClientCommand);

            this.interviewId = createInterviewOnClientCommand.Id.FormatGuid();
            this.Clients.Caller.startInterview(this.interviewId);
        }

        public void StartInterview(string interviewId)
        {
            this.interviewId = interviewId;
        }


        public override Task OnDisconnected(bool stopCalled)
        {
            // statefull interview can be removed from cache here

            return base.OnDisconnected(stopCalled);
        }
    }
}