using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Commands.File
{
    public class UpdateFileMetaCommand : ICommand
    {
        public UpdateFileMetaCommand(string id, string title, string desc, UserLight executor)
        {
            Id = IdUtil.CreateFileId(id);
            Executor = executor;
            Description = desc;
            Title = title;
        }
        public string Id { get; set; }

        public string Title { get; private set; }

        public string Description { get; private set; }

        #region ICommand Members

        public UserLight Executor { get; set; }

        #endregion
    }
}