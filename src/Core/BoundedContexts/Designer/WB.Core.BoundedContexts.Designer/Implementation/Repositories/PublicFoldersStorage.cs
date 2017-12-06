using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Designer.Implementation.Repositories
{
    public class PublicFoldersStorage : IPublicFoldersStorage
    {
        private readonly IPlainStorageAccessor<QuestionnaireListViewFolder> folderStorage;
        private readonly IPlainStorageAccessor<QuestionnaireListViewItem> questionnaireStorage;
        private readonly IPlainStorageAccessor<User> accountStorage;

        QuestionnaireListViewFolder publicQuestionnairesFolder = new QuestionnaireListViewFolder()
        {
            Title = Common.PublicQuestionnaires,
        };


        public PublicFoldersStorage(IPlainStorageAccessor<QuestionnaireListViewFolder> folderStorage,
            IPlainStorageAccessor<QuestionnaireListViewItem> questionnaireStorage,
            IPlainStorageAccessor<User> accountStorage)
        {
            this.folderStorage = folderStorage;
            this.questionnaireStorage = questionnaireStorage;
            this.accountStorage = accountStorage;
        }

        public IEnumerable<QuestionnaireListViewFolder> GetSubFolders(Guid? folderId)
        {
            return folderStorage.Query(_ =>
            {
                return _.Where(f => f.Parent == folderId).OrderBy(i => i.Title);
            }).ToArray();
        }

        public IEnumerable<QuestionnaireListViewFolder> GetRootFolders()
        {
            return new[]  { publicQuestionnairesFolder };
        }

        public QuestionnaireListViewFolder CreateFolder(Guid folderId, string title, Guid? parentId, Guid userId)
        {
            var parentFolder = parentId.HasValue
                ? folderStorage.GetById(parentId.Value)
                : null;
            string path = (parentFolder?.Path ?? String.Empty) + "/" + folderId;
            int depth = parentFolder?.Depth + 1 ?? 0;

            var folder = new QuestionnaireListViewFolder()
            {
                PublicId = folderId,
                Title = title,
                Parent = parentId,
                CreateDate = DateTime.UtcNow,
                CreatedBy = userId,
                CreatorName = this.accountStorage.GetById(userId.FormatGuid())?.UserName,
                Depth = depth,
                Path = path
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

        public void AssignFolderToQuestionnaire(Guid questionnaireId, Guid? folderId)
        {
            var item = questionnaireStorage.GetById(questionnaireId.FormatGuid());
            item.FolderId = folderId;
            questionnaireStorage.Store(item, item.QuestionnaireId);
        }

        public IEnumerable<QuestionnaireListViewFolder> GetFoldersPath(Guid? folderId)
        {
            List<QuestionnaireListViewFolder> folders = new List<QuestionnaireListViewFolder>();
            while (folderId.HasValue)
            {
                var folder = folderStorage.GetById(folderId.Value);
                folders.Add(folder);
                folderId = folder.Parent;
            }
            folders.Add(publicQuestionnairesFolder);
            folders.Reverse();
            return folders;
        }
    }
}