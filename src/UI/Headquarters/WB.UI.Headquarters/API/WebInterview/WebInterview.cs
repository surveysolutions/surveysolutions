using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
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
        private readonly ILiteEventRegistry eventRegistry;
        private readonly IMapper autoMapper;
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IPlainTransactionManager plainTransactionManager;
        
        private string CallerInterviewId
        {
            get { return this.Clients.Caller.interviewId; }
            set { this.Clients.Caller.interviewId = value; }
        }
        
        private IStatefulInterview currentInterview => this.statefulInterviewRepository.Get(this.CallerInterviewId);

        private IQuestionnaire currentQuestionnaire => this.questionnaireRepository.GetQuestionnaire(this.currentInterview.QuestionnaireIdentity, this.currentInterview.Language);

        public WebInterview(
            IStatefulInterviewRepository statefulInterviewRepository, 
            ICommandDeserializer commandDeserializer,
            ICommandService commandService,
            ILogger logger,
            IUserViewFactory usersRepository,
            ILiteEventRegistry eventRegistry,
            IMapper autoMapper,
            IQuestionnaireStorage questionnaireRepository,
            IPlainTransactionManager plainTransactionManager)
        {
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.commandDeserializer = commandDeserializer;
            this.commandService = commandService;
            this.logger = logger;
            this.usersRepository = usersRepository;
            this.eventRegistry = eventRegistry;
            this.autoMapper = autoMapper;
            this.questionnaireRepository = questionnaireRepository;
            this.plainTransactionManager = plainTransactionManager;
        }

        public void CreateInterview(string questionnaireId, string interviewerName)
        {
            var questionnaireIdentity = QuestionnaireIdentity.Parse(questionnaireId);

            var interviewer = this.usersRepository.Load(new UserViewInputModel(UserName: interviewerName, UserEmail: null));

            var createInterviewOnClientCommand = new CreateInterviewOnClientCommand(Guid.NewGuid(),
                interviewer.PublicKey, questionnaireIdentity, DateTime.UtcNow,
                interviewer.Supervisor.Id);

            this.commandService.Execute(createInterviewOnClientCommand);
            this.StartInterview(createInterviewOnClientCommand.Id.FormatGuid());
        }

        public void StartInterview(string interviewId)
        {
            this.CallerInterviewId = interviewId;
            this.eventRegistry.Subscribe(this, interviewId);
            this.Groups.Add(this.Context.ConnectionId, interviewId);
            this.Clients.Group(interviewId).startInterview(interviewId);
        }


        public override Task OnDisconnected(bool stopCalled)
        {
            // statefull interview can be removed from cache here
            this.eventRegistry.Unsubscribe(this);
            return base.OnDisconnected(stopCalled);
        }
    }
}