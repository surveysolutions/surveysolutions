﻿using System.ComponentModel;
using System.Web.Http;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.UI.Headquarters.API.DataCollection.Enumerator;

namespace WB.UI.Headquarters.API.DataCollection.Interviewer
{
    [Localizable(false)]
    public class InterviewerCommonWebApiConfig
    {
#pragma warning disable 4014

        public static void Register(HttpConfiguration config)
        {
            config.TypedRoute("api/enumerator/logs", c => c.Action<LogsApiController>(x => x.Post()));

            config.TypedRoute(@"api/interviewer", c => c.Action<InterviewerApiController>(x => x.Get()));
            config.TypedRoute(@"api/interviewer/patch/{deviceVersion}", c => c.Action<InterviewerApiController>(x => x.Patch(Param.Any<int>())));
            config.TypedRoute(@"api/interviewer/latestversion", c => c.Action<InterviewerApiController>(x => x.GetLatestVersion()));

            config.TypedRoute(@"api/interviewer/extended", c => c.Action<InterviewerApiController>(x => x.GetExtended()));
            config.TypedRoute(@"api/interviewer/extended/patch/{deviceVersion}", c => c.Action<InterviewerApiController>(x => x.PatchExtended(Param.Any<int>())));
            config.TypedRoute(@"api/interviewer/extended/latestversion", c => c.Action<InterviewerApiController>(x => x.GetLatestExtendedVersion()));
            
            config.TypedRoute(@"api/interviewer/v2/tabletInfo", c => c.Action<InterviewerApiController>(x => x.PostTabletInformation()));

            config.TypedRoute(@"api/interviewer/compatibility/{deviceid}/{deviceSyncProtocolVersion}",
                c => c.Action<InterviewerApiController>(x => x.CheckCompatibility(Param.Any<string>(), 
                    Param.Any<int>(),
                    Param.Any<string>())));
        }

#pragma warning restore 4014

    }

}
