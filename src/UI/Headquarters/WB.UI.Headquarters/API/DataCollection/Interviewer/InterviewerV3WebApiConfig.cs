using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Http;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.UI.Headquarters.API.DataCollection.Interviewer.v3;

namespace WB.UI.Headquarters.API.DataCollection.Interviewer
{
    [Localizable(false)]
    public class InterviewerV3WebApiConfig
    {
#pragma warning disable 4014

        public static void Register(HttpConfiguration config)
        {
            config.TypedRoute("api/interviewer/v3/interviews", 
                c => c.Action<InterviewsApiV3Controller>(x => x.Get()));
            config.TypedRoute("api/interviewer/v3/interviews/{id:guid}", 
                c => c.Action<InterviewsApiV3Controller>(x => x.Details(Param.Any<Guid>())));
            config.TypedRoute("api/interviewer/v3/interviews/{id:guid}",
                c => c.Action<InterviewsApiV3Controller>(x => x.Post(Param.Any<InterviewPackageApiView>())));
            config.TypedRoute("api/interviewer/v3/interviews/CheckObsoleteInterviews",
                c => c.Action<InterviewsApiV3Controller>(x => x.CheckObsoleteInterviews(Param.Any<List<ObsoletePackageCheck>>())));
            config.TypedRoute("api/interviewer/v3/interviews/{id:guid}/logstate",
                c => c.Action<InterviewsApiV3Controller>(x => x.LogInterviewAsSuccessfullyHandled(Param.Any<Guid>())));
            config.TypedRoute("api/interviewer/v3/interviews/{id:guid}/image",
                c => c.Action<InterviewsApiV3Controller>(x => x.PostImage(Param.Any<PostFileRequest>())));
            config.TypedRoute("api/interviewer/v3/interviews/{id:guid}/audio",
                c => c.Action<InterviewsApiV3Controller>(x => x.PostAudio(Param.Any<PostFileRequest>())));
            config.TypedRoute("api/interviewer/v3/interviews/{id:guid}/getInterviewUploadState",
                c => c.Action<InterviewsApiV3Controller>(x => x.GetInterviewUploadState(Param.Any<Guid>(), Param.Any<EventStreamSignatureTag>())));
        }

#pragma warning restore 4014

    }
}
