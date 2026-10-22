using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.UI.Headquarters.Code.Authentication;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection
{
    [Authorize(AuthenticationSchemes = AuthType.Basic + "," + AuthType.AuthToken)]
    [IgnoreAntiforgeryToken]
    public abstract class DataCollectionControllerBase : ControllerBase
    {
    }
}

