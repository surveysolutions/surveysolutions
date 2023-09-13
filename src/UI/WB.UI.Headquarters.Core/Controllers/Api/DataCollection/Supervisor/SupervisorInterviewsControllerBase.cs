using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.Synchronization.MetaInfo;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection.Supervisor
{
    [Authorize(Roles = "Supervisor")]
    public class SupervisorInterviewsControllerBase : InterviewsControllerBase
    {
        public SupervisorInterviewsControllerBase(IImageFileStorage imageFileStorage, 
            IAudioFileStorage audioFileStorage, IAuthorizedUser authorizedUser, 
            IInterviewInformationFactory interviewsFactory, IInterviewPackagesService packagesService, 
            ICommandService commandService, IMetaInfoBuilder metaBuilder, 
            IJsonAllTypesSerializer synchronizationSerializer, IHeadquartersEventStore eventStore, 
            IAudioAuditFileStorage audioAuditFileStorage, IUserToDeviceService userToDeviceService, 
            IWebHostEnvironment webHostEnvironment) 
            : base(imageFileStorage, audioFileStorage, authorizedUser, interviewsFactory, packagesService, commandService, 
                metaBuilder, synchronizationSerializer, eventStore, audioAuditFileStorage,userToDeviceService, 
                webHostEnvironment)
        {
        }

        protected override string ProductName => "org.worldbank.solutions.supervisor";


        protected override IEnumerable<InterviewInformation> GetInProgressInterviewsForResponsible(Guid supervisorId)
        {
            return this.interviewsFactory.GetInProgressInterviewsForSupervisor(supervisorId);
        }
    }
}
