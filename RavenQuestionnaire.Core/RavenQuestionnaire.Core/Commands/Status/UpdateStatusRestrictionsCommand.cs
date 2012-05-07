using System;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Commands.Status
{
    public class UpdateStatusRestrictionsCommand : ICommand
    {
        public string QId { get; private set; }

        public string StatusId { get; private set; }


        public Dictionary<string, List<SurveyStatus>> StatusRoles { get; private set; }

        public Guid PublicKey { set; get; }

        public UserLight Executor { get; set; }
        
        public UpdateStatusRestrictionsCommand(string qid,string statusId,  Guid publicKey,  Dictionary<string, List<SurveyStatus>> statusRoles, UserLight executor)
        {
            QId = IdUtil.CreateQuestionnaireId(qid);
            StatusId = IdUtil.CreateStatusId(statusId);

            StatusRoles = statusRoles;
            PublicKey = publicKey;
            Executor = executor;
        }
    }
}
