using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.Synchronization.MetaInfo;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.DataCollection.Supervisor
{
    [ApiBasicAuth(new[] { UserRoles.Supervisor })]
    public class SupervisorInterviewsControllerBase : InterviewsControllerBase
    {
        public SupervisorInterviewsControllerBase(IImageFileStorage imageFileStorage, IAudioFileStorage audioFileStorage, IAuthorizedUser authorizedUser, IInterviewInformationFactory interviewsFactory, IInterviewPackagesService packagesService, ICommandService commandService, IMetaInfoBuilder metaBuilder, IJsonAllTypesSerializer synchronizationSerializer, IHeadquartersEventStore eventStore) : base(imageFileStorage, audioFileStorage, authorizedUser, interviewsFactory, packagesService, commandService, metaBuilder, synchronizationSerializer, eventStore)
        {
        }

        protected override IEnumerable<InterviewInformation> GetInProgressInterviewsForResponsible(Guid supervisorId)
        {
            return this.interviewsFactory.GetInProgressInterviewsForSupervisor(supervisorId);
        }
    }
}
