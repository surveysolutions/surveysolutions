using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Designer.Implementation.Repositories
{
    public class PublicFoldersStorage : IPublicFoldersStorage
    {
        private readonly IPlainStorageAccessor<QuestionnaireListViewFolder> folderStorage;

        public PublicFoldersStorage(IPlainStorageAccessor<QuestionnaireListViewFolder> folderStorage )
        {
            this.folderStorage = folderStorage;
        }

        public IEnumerable<QuestionnaireListViewFolder> GetSubFolders(Guid folderId)
        {
            return folderStorage.Query(_ =>
            {
                return _.Where(f => f.Parent == folderId).OrderBy(i => i.Title);
            }).ToArray();
        }

        public IEnumerable<QuestionnaireListViewFolder> GetRootFolders()
        {
            return new QuestionnaireListViewFolder[]
            {
                new QuestionnaireListViewFolder()
                {
                    Title = Common.PublicQuestionnaires,
                }, 
            };
        }

        public QuestionnaireListViewFolder CreateFolder(Guid folderId, string title, Guid? parentId, Guid userId)
        {
            var folder = new QuestionnaireListViewFolder()
            {
                Id = folderId,
                Title = title,
                Parent = parentId,
                CreateDate = DateTime.UtcNow,
                CreatedBy = userId
            };
            folderStorage.Store(folder, folderId);
            return folder;
        }

        public void RenameFolder(Guid id, string newName)
        {
            var folder = folderStorage.GetById(id);
            folder.Title = newName;
            folderStorage.Store(folder, id);
        }

        public void RemoveFolder(Guid id)
        {
            folderStorage.Remove(id);
        }
    }
}