using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Ncqrs.Eventing.Storage;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection.Enumerator;

[AuthorizeByRole(UserRoles.Interviewer, UserRoles.Supervisor)]
[Route("api/enumerator/v3/interviews")]
public class EnumeratorInterviewsApiV1Controller: ControllerBase
{
    private readonly IHeadquartersEventStore eventStore;

    public EnumeratorInterviewsApiV1Controller(IHeadquartersEventStore eventStore)
    {
        this.eventStore = eventStore;
    }

    [Route("{interviewId}/getSyncInfoPackage")]
    public SyncInfoPackageResponse GetSyncInfoPackage(Guid interviewId, InterviewSyncInfoPackage package)
    {
        var hqEvents = eventStore.Read(interviewId).ToList();
        if (hqEvents.Count == 0)
            return new SyncInfoPackageResponse() { HasInterview = false, NeedSendFullStream = true };

        var infoPackageResponse = new SyncInfoPackageResponse() { HasInterview = true, NeedSendFullStream = false };

        if (package.LastEventIdFromPreviousSync.HasValue)
            infoPackageResponse.NeedSendFullStream = hqEvents.All(e => e.EventIdentifier != package.LastEventIdFromPreviousSync.Value);

        return infoPackageResponse;
    }
}