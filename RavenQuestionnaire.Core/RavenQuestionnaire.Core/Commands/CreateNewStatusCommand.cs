using System.Collections.Generic;

namespace RavenQuestionnaire.Core.Commands
{
    public class CreateNewStatusCommand : ICommand
    {
        public int Status { get; private set; }
        public Dictionary<string, IEnumerable<int>> RoleStatusList { get; private set; }

        public string Title { get; private set; }


        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="title"></param>
        public CreateNewStatusCommand(string title)
        {
            Title = title;
        }


        public CreateNewStatusCommand(Dictionary<string, IEnumerable<int>> roleStatusList, int status)
        {
            Status = status;
            RoleStatusList = roleStatusList;
        }
    }
}
