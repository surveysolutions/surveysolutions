﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.Synchronization.MetaInfo;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.DataCollection.Supervisor
{
    [ApiBasicAuth(new[] { UserRoles.Supervisor })]
    public class SupervisorInterviewsControllerBase : InterviewsControllerBase
    {
        public SupervisorInterviewsControllerBase(IImageFileStorage imageFileStorage, IAudioFileStorage audioFileStorage, IAuthorizedUser authorizedUser, IInterviewInformationFactory interviewsFactory, IInterviewPackagesService interviewPackagesService, ICommandService commandService, IMetaInfoBuilder metaBuilder, IJsonAllTypesSerializer synchronizationSerializer) : base(imageFileStorage, audioFileStorage, authorizedUser, interviewsFactory, interviewPackagesService, commandService, metaBuilder, synchronizationSerializer)
        {
        }

        protected override IEnumerable<InterviewInformation> GetInProgressInterviewsForResponsible(Guid supervisorId)
        {
            return this.interviewsFactory.GetInProgressInterviewsForSupervisor(supervisorId);
        }
    }
}
