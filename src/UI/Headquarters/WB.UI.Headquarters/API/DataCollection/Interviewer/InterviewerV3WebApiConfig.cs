using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Web.Http;
using WB.Core.SharedKernel.Structures.TabletInformation;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.UI.Headquarters.API.DataCollection.Interviewer.v2;
using WB.UI.Headquarters.API.DataCollection.Interviewer.v3;

namespace WB.UI.Headquarters.API.DataCollection.Interviewer
{
    [Localizable(false)]
    public class InterviewerV3WebApiConfig
    {
#pragma warning disable 4014

        public static void Register(HttpConfiguration config)
        {
            config.TypedRoute("api/interviewer/v3/interviews/{id:guid}",
                c => c.Action<InterviewsApiV3Controller>(x => x.DetailsV3(Param.Any<Guid>())));

            config.TypedRoute("api/interviewer/v3/interviews/{id:guid}",
                c => c.Action<InterviewsApiV3Controller>(x => x.PostV3(Param.Any<InterviewPackageApiView>())));

            config.TypedRoute("api/interviewer/v3/interviews/CheckObsoleteInterviews",
                c => c.Action<InterviewsApiV3Controller>(x => x.CheckObsoleteInterviews(Param.Any<List<ObsoletePackageCheck>>())));
        }

#pragma warning restore 4014

    }
}
