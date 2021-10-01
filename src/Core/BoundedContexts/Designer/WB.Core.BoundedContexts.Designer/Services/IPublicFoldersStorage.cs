using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IPublicFoldersStorage
    {
        Task<IEnumerable<QuestionnaireListViewFolder>> GetSubFoldersAsync(Guid? folderId);

        IEnumerable<QuestionnaireListViewFolder> GetRootFolders();

        QuestionnaireListViewFolder CreateFolder(Guid folderId, string? title, Guid? parentId, Guid userId);

        void RenameFolder(Guid id, string newName);

        void RemoveFolder(Guid id);

        void AssignFolderToQuestionnaire(Guid questionnaireId, Guid? folderId);

        IEnumerable<QuestionnaireListViewFolder> GetFoldersPath(Guid? folderId);

        Task<List<QuestionnaireListViewFolder>> GetAllFoldersAsync();
    }
}
