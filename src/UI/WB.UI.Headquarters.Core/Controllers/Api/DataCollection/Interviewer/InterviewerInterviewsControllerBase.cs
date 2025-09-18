using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.Synchronization.MetaInfo;
using WB.Enumerator.Native.WebInterview;
using WB.UI.Shared.Web.Services;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection.Interviewer
{
    public class InterviewerInterviewsControllerBase : InterviewsControllerBase
    {
        public InterviewerInterviewsControllerBase(IImageFileStorage imageFileStorage, 
            IAudioFileStorage audioFileStorage, IAuthorizedUser authorizedUser, 
            IInterviewInformationFactory interviewsFactory, IInterviewPackagesService packagesService, 
            ICommandService commandService, IMetaInfoBuilder metaBuilder, 
            IJsonAllTypesSerializer synchronizationSerializer, IHeadquartersEventStore eventStore, 
            IAudioAuditFileStorage audioAuditFileStorage, IUserToDeviceService userToDeviceService,
            IWebHostEnvironment webHostEnvironment, IImageProcessingService imageProcessingService,
            IBrokenImageFileStorage brokenImageFileStorage,
            IBrokenAudioFileStorage brokenAudioFileStorage,
            IBrokenAudioAuditFileStorage brokenAudioAuditFileStorage) 
            : base(imageFileStorage, audioFileStorage, authorizedUser, interviewsFactory, packagesService, 
                commandService, metaBuilder, synchronizationSerializer, eventStore, audioAuditFileStorage, 
                userToDeviceService, webHostEnvironment, imageProcessingService, brokenImageFileStorage,
                brokenAudioFileStorage, brokenAudioAuditFileStorage)
        {
        }

        protected override string ProductName => "org.worldbank.solutions.interviewer";
        
        protected override bool AllowWorkWithInterview(Guid interviewId)
        {
            var interview = interviewsFactory.GetInterviewsByIds([interviewId]).SingleOrDefault();

            if (interview == null)
                return true; // new interview

            if (interview.ResponsibleId == authorizedUser.Id)
                return true;

            return false;
        }

        protected override IEnumerable<InterviewInformation> GetInProgressInterviewsForResponsible(Guid responsibleId)
        {
            return this.interviewsFactory.GetInProgressInterviewsForInterviewer(responsibleId);
        }
    }
}
