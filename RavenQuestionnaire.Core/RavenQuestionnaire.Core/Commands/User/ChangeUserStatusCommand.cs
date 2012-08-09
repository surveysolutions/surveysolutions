using System;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using RavenQuestionnaire.Core.Domain;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Commands.User
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(UserAR), "ChangeUserStatus")]
    public class ChangeUserStatusCommand : CommandBase
    {
        public string UserId { get; private set; }
        public bool IsLocked { get; private set; }

        public ChangeUserStatusCommand(string userId, bool isLocked)
        {
            this.UserId = IdUtil.CreateUserId(userId);
            this.IsLocked = isLocked;
        }
    }
}
