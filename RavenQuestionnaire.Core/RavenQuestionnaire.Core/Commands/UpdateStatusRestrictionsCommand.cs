using System.Collections.Generic;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Commands
{
    public class UpdateStatusRestrictionsCommand : ICommand
    {
        public string StatusId { get; private set; }

        public Dictionary<string, List<string>> StatusRoles { get; private set; }


        public UpdateStatusRestrictionsCommand(string statusId, Dictionary<string, List<string>> statusRoles)
        {
            StatusId = IdUtil.CreateStatusId(statusId);
            StatusRoles = statusRoles;
        }
    }
}
