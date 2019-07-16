﻿using System;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoMapper;
using Microsoft.AspNet.SignalR.Hosting;
using Microsoft.AspNet.SignalR.Hubs;
using Moq;
using Ncqrs.Eventing;
using NSubstitute;
using ReflectionMagic;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Enumerator.Native.WebInterview;
using WB.UI.Headquarters.API.WebInterview;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;

namespace WB.Tests.Abc.TestFactories
{
    internal class OtherFactory
    {
        public Fixture AutoFixture()
        {
            var autoFixture = new Fixture();
            autoFixture.Customize(new AutoMoqCustomization());
            return autoFixture;
        }

        public CommittedEvent CommittedEvent(UncommittedEvent evnt, Guid eventSourceId)
            => new CommittedEvent(Guid.NewGuid(),
                        evnt.Origin,
                        evnt.EventIdentifier,
                        eventSourceId,
                        evnt.EventSequence,
                        evnt.EventTimeStamp,
                        0,
                        evnt.Payload);

        public CommittedEvent CommittedEvent(string origin = null, Guid? eventSourceId = null, IEvent payload = null,
            Guid? eventIdentifier = null, int eventSequence = 1, Guid? commitId = null)
            => new CommittedEvent(
                commitId ?? Guid.NewGuid(),
                origin,
                eventIdentifier ?? Guid.Parse("44440000444440000004444400004444"),
                eventSourceId ?? Guid.Parse("55550000555550000005555500005555"),
                eventSequence,
                new DateTime(2014, 10, 22),
                0,
                payload ?? Mock.Of<IEvent>());

        public NavigationState NavigationState(IStatefulInterviewRepository interviewRepository = null,
            IQuestionnaireStorage questionnaireStorage = null,
            Identity currentGroup = null)
        {
            var navigationState = new NavigationState(
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                questionnaireStorage ?? Mock.Of<IQuestionnaireStorage>(x => x.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()) == Mock.Of<IQuestionnaire>()),
                Substitute.For<IViewModelNavigationService>()

                );
            if (currentGroup != null)
            {
                navigationState.AsDynamic().CurrentGroup = currentGroup;
            }
            return navigationState;
        }

        public UncommittedEvent UncommittedEvent(Guid? eventSourceId = null, IEvent payload = null, int sequence = 1, int initialVersion = 1)
            => new UncommittedEvent(Guid.NewGuid(), eventSourceId ?? Guid.NewGuid(), sequence, initialVersion, DateTime.Now, payload);

        public Stream TabDelimitedTextStream(string[] headers, params string[][] cells)
            => new MemoryStream(Encoding.UTF8.GetBytes(string.Join(Environment.NewLine,
                new[] { headers }.Union(cells).Select(x => string.Join(TabExportFile.Delimiter, x)))));

        public SupervisorIdentity SupervisorIdentity(string id = null,
            string userName = null,
            string passwordHash = null,
            Guid? userId = null)
        {
            return new SupervisorIdentity
            {
                Id = id ?? Guid.NewGuid().FormatGuid(),
                Name = userName ?? "name",
                UserId = userId?? Guid.NewGuid(),
                PasswordHash = passwordHash ?? "pswdHash"
            };
        }

        public IPrincipal SupervisorPrincipal(Guid? userId = null)
        {
            return Mock.Of<IPrincipal>(x => x.IsAuthenticated == true &&
                                            x.CurrentUserIdentity == Create.Other.SupervisorIdentity(null, null, null, userId));
        }

        public IPrincipal InterviewerPrincipal(Guid? userId = null)
        {
            return Mock.Of<IPrincipal>(x => x.IsAuthenticated == true &&
                                            x.CurrentUserIdentity == Create.Other.InterviewerIdentity(null, null, null, userId));
        }

        public InterviewerIdentity InterviewerIdentity(string id = null,
            string userName = null,
            string passwordHash = null,
            Guid? userId = null)
        {
            return new InterviewerIdentity
            {
                Id = id ?? Guid.NewGuid().FormatGuid(),
                Name = userName ?? "name",
                PasswordHash = passwordHash ?? "pswdHash",
                UserId = userId ?? Guid.NewGuid()
            };
        }

        public WebInterviewHub WebInterviewHub(IStatefulInterview statefulInterview, IQuestionnaireStorage questionnaire, string sectionId = null, IMapper mapper = null)
        {
            var statefulInterviewRepository = SetUp.StatefulInterviewRepository(statefulInterview);
            var questionnaireStorage = questionnaire;
            var webInterviewInterviewEntityFactory = Create.Service.WebInterviewInterviewEntityFactory(autoMapper: mapper);

            var serviceLocator = Mock.Of<IServiceLocator>(sl =>
                sl.GetInstance<IStatefulInterviewRepository>() == statefulInterviewRepository
                && sl.GetInstance<IQuestionnaireStorage>() == questionnaireStorage
                && sl.GetInstance<IWebInterviewInterviewEntityFactory>() == webInterviewInterviewEntityFactory
                && sl.GetInstance<IAuthorizedUser>() == Mock.Of<IAuthorizedUser>());

            var webInterviewHub = new WebInterviewHub();
            webInterviewHub.SetServiceLocator(serviceLocator);

            webInterviewHub.Context = Mock.Of<HubCallerContext>(h =>
                h.QueryString == Mock.Of<INameValueCollection>(p => 
                    p["interviewId"] == statefulInterview.Id.FormatGuid()
                )
            );

            if (!string.IsNullOrEmpty(sectionId))
            {
                dynamic mockCaller = new ExpandoObject();
                mockCaller.sectionId = sectionId;
                var mockClients = new Mock<IHubCallerConnectionContext<dynamic>>();
                mockClients.Setup(m => m.Caller).Returns((ExpandoObject)mockCaller);
                webInterviewHub.Clients = mockClients.Object;
            }

            return webInterviewHub;
        }
    }
}
