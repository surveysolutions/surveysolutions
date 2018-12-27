﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IPublicFoldersStorage
    {
        IEnumerable<QuestionnaireListViewFolder> GetSubFolders(Guid? folderId);

        IEnumerable<QuestionnaireListViewFolder> GetRootFolders();

        QuestionnaireListViewFolder CreateFolder(Guid id, string title, Guid? parentId, Guid userId);

        void RenameFolder(Guid id, string newName);

        void RemoveFolder(Guid id);

        void AssignFolderToQuestionnaire(Guid questionnaireId, Guid? folderId);

        IEnumerable<QuestionnaireListViewFolder> GetFoldersPath(Guid? folderId);

        Task<List<QuestionnaireListViewFolder>> GetAllFoldersAsync();
    }
}
