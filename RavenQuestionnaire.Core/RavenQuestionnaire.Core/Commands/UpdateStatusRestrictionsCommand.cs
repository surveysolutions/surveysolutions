using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Commands
{
    public class UpdateStatusRestrictionsCommand : ICommand
    {
        public string StatusId { get; private set; }

        public Dictionary<string, List<SurveyStatus>> StatusRoles { get; private set; }

        
        public UpdateStatusRestrictionsCommand(string statusId, Dictionary<string, List<SurveyStatus>> statusRoles)
        {
            StatusId = IdUtil.CreateStatusId(statusId);
            StatusRoles = statusRoles;
        }
    }
}
