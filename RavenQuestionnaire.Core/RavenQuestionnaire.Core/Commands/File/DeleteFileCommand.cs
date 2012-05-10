using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Commands.File
{
    public class DeleteFileCommand : ICommand
    {
        public DeleteFileCommand(string id, UserLight executor)
        {
            Executor = executor;
            Id = Utility.IdUtil.CreateFileId(id);
        }

        public string Id { get; private set; }

        #region ICommand Members

        public UserLight Executor { get; set; }

        #endregion
    }
}