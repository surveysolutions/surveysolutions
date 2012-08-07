using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Commands.User
{
    public class ChangeUserStatusCommand : ICommand
    {
        public string UserId { get; private set; }
        public bool IsLocked { get; private set; }
        public UserLight Executor { get; set; }

        public ChangeUserStatusCommand(string userId, bool isLocked, UserLight executor)
        {
            this.UserId = IdUtil.CreateUserId(userId);
            this.IsLocked = isLocked;
            Executor = executor;
        }
    }
}
