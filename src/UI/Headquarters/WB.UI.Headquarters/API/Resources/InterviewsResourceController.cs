using System;
using System.Web.Http;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.UI.Headquarters.API.Resources
{
    [RoutePrefix("api/resources/interviews/v1")]
    public class InterviewsResourceController : ApiController
    {
        private readonly IReadSideRepositoryReader<ViewWithSequence<InterviewData>> interviewDataReader;
        private readonly IInterviewSynchronizationDtoFactory factory;

        public InterviewsResourceController(IReadSideRepositoryReader<ViewWithSequence<InterviewData>> interviewDataReader,
            IInterviewSynchronizationDtoFactory factory)
        {
            this.interviewDataReader = interviewDataReader;
            this.factory = factory;
        }

        [Route("{id}", Name = "api.interviewDetails")]
        public InterviewSynchronizationDto Get(string id)
        {
            var interviewData = this.interviewDataReader.GetById(id);

            InterviewData document = interviewData.Document;
            InterviewSynchronizationDto interviewSynchronizationDto = 
                factory.BuildFrom(document);
            
            return interviewSynchronizationDto;
        }
    }
}