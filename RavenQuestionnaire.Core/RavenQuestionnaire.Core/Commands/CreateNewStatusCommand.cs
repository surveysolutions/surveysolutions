using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Commands
{
    public class CreateNewStatusCommand : ICommand
    {
        public int Status { get; private set; }
        public Dictionary<string, IEnumerable<int>> RoleStatusList { get; private set; }

        public string Title { get; private set; }

        public bool IsInitial { get; private set; }

        public UserLight Executor { get; set; }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="isInitial"> </param>
        public CreateNewStatusCommand(string title, bool isInitial, UserLight executor)
        {
            Title = title;
            IsInitial = isInitial;
            Executor = executor;

        }


        public CreateNewStatusCommand(Dictionary<string, IEnumerable<int>> roleStatusList, int status, UserLight executer)
        {
            Status = status;
            RoleStatusList = roleStatusList;
            Executor = executer;
        }
    }
}
