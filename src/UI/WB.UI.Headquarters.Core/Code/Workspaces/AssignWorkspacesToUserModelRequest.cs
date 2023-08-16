using MediatR;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Models;

namespace WB.UI.Headquarters.Code.Workspaces
{
    public class AssignWorkspacesToUserModelRequest : IRequest<Unit>
    {
        public AssignWorkspacesToUserModelRequest(ModelStateDictionary modelState, AssignWorkspacesToUserModel assignModel)
        {
            ModelState = modelState;
            AssignModel = assignModel;
        }

        public ModelStateDictionary ModelState { get; }
        public AssignWorkspacesToUserModel AssignModel { get; }
    }
}
