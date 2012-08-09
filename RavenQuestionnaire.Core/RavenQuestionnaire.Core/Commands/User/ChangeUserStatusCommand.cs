using System;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using RavenQuestionnaire.Core.Domain;

namespace RavenQuestionnaire.Core.Commands.User
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(UserAR), "SetUserLockState")]
    public class ChangeUserStatusCommand : CommandBase
    {
        [AggregateRootId]
        public Guid PublicKey { get; set; }

        public bool IsLocked { get; set; }

        public ChangeUserStatusCommand (){}

        public ChangeUserStatusCommand(Guid publicKey,  bool isLocked)
            : base(publicKey)
        {
            PublicKey = publicKey;
            IsLocked = isLocked;
        }
    }
}
