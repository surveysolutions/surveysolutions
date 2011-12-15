using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Commands
{
    public class DeleteUserCommand : ICommand
    {
           public string UserId { get; set; }

           public DeleteUserCommand(string userId)
         {
             this.UserId = IdUtil.CreateUserId(userId);
        }
    }
}
