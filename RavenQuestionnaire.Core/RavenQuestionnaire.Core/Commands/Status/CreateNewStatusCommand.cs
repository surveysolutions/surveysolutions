using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Commands.Status
{
    public class CreateNewStatusCommand : ICommand
    {
        public int Status { get; private set; }

        public Dictionary<string, IEnumerable<int>> RoleStatusList { get; private set; }

        public string Title { get; private set; }

        public bool IsInitial { get; private set; }

        public UserLight Executor { get; set; }

        public string QuestionnaireId { get; set; }

        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="isInitial"> </param>
        /// <param name="questionnaireId"> </param>
        /// <param name="executor"> </param>
        public CreateNewStatusCommand(string title, bool isInitial, string questionnaireId,  UserLight executor)
        {
            Title = title;
            IsInitial = isInitial;
            Executor = executor;
            QuestionnaireId = questionnaireId;
        }
    }
}
