using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.Synchronization.MetaInfo;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.DataCollection.Interviewer
{
    public class InterviewerInterviewsControllerBase : InterviewsControllerBase
    {
        public InterviewerInterviewsControllerBase(IImageFileStorage imageFileStorage, IAudioFileStorage audioFileStorage, IAuthorizedUser authorizedUser, IInterviewInformationFactory interviewsFactory, IInterviewPackagesService interviewPackagesService, ICommandService commandService, IMetaInfoBuilder metaBuilder, IJsonAllTypesSerializer synchronizationSerializer, IHeadquartersEventStore eventStore, IInterviewPackagesService packagesService) : base(imageFileStorage, audioFileStorage, authorizedUser, interviewsFactory, interviewPackagesService, commandService, metaBuilder, synchronizationSerializer, eventStore, packagesService)
        {
        }

        protected override IEnumerable<InterviewInformation> GetInProgressInterviewsForResponsible(Guid responsibleId)
        {
            return this.interviewsFactory.GetInProgressInterviewsForInterviewer(responsibleId);
        }
    }
}
