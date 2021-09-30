using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.BoundedContexts.Designer.Implementation.Repositories
{
    public class PublicFoldersStorage : IPublicFoldersStorage
    {
        private readonly DesignerDbContext dbContext;

        readonly QuestionnaireListViewFolder publicQuestionnairesFolder = new QuestionnaireListViewFolder()
        {
            Title = Common.PublicQuestionnaires,
        };


        public PublicFoldersStorage(DesignerDbContext dbContext)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<IEnumerable<QuestionnaireListViewFolder>> GetSubFoldersAsync(Guid? folderId)
        {
            return await dbContext.QuestionnaireFolders.Where(f => f.Parent == folderId).OrderBy(i => i.Title).ToListAsync();
        }

        public IEnumerable<QuestionnaireListViewFolder> GetRootFolders()
        {
            return new[] {publicQuestionnairesFolder};
        }

        public QuestionnaireListViewFolder CreateFolder(Guid folderId, string? title, Guid? parentId,
            Guid userId)
        {
            var parentFolder = parentId.HasValue
                ? dbContext.QuestionnaireFolders.Find(parentId.Value)
                : null;
            string path = (parentFolder?.Path ?? String.Empty) + "/" + folderId;
            int depth = parentFolder?.Depth + 1 ?? 0;

            var folder = new QuestionnaireListViewFolder()
            {
                PublicId = folderId,
                Title = title ?? "",
                Parent = parentId,
                CreateDate = DateTime.UtcNow,
                CreatedBy = userId,
                CreatorName = dbContext.Users.Find(userId)?.UserName,
                Depth = depth,
                Path = path
            };

            dbContext.QuestionnaireFolders.Add(folder);
            dbContext.SaveChanges();
            return folder;
        }

        public void RenameFolder(Guid id, string newName)
        {
            var folder = dbContext.QuestionnaireFolders.Find(id);
            folder.Title = newName;
            dbContext.SaveChanges();
        }

        public void RemoveFolder(Guid id)
        {
            var folder = dbContext.QuestionnaireFolders.Find(id);
            dbContext.Remove(folder);
            dbContext.SaveChanges();
        }

        public void AssignFolderToQuestionnaire(Guid questionnaireId, Guid? folderId)
        {
            var item = dbContext.Questionnaires.Find(questionnaireId.FormatGuid());
            item.FolderId = folderId;
            dbContext.SaveChanges();
        }

        public IEnumerable<QuestionnaireListViewFolder> GetFoldersPath(Guid? folderId)
        {
            List<QuestionnaireListViewFolder> folders = new List<QuestionnaireListViewFolder>();
            while (folderId.HasValue)
            {
                var folder = dbContext.QuestionnaireFolders.Find(folderId.Value);
                folders.Add(folder);
                folderId = folder.Parent;
            }

            folders.Add(publicQuestionnairesFolder);
            folders.Reverse();
            return folders;
        }

        public Task<List<QuestionnaireListViewFolder>> GetAllFoldersAsync()
        {
            var allFolders = dbContext.QuestionnaireFolders.OrderBy(i => i.Title).ToList();
            
            var stack = new Stack<QuestionnaireListViewFolder>(allFolders.Where(x => x.Parent == null).OrderByDescending(x => x.Title));

            var result = new List<QuestionnaireListViewFolder>();
            while (stack.Count != 0)
            {
                var folder = stack.Pop();
                result.Add(folder);
                allFolders.Where(x => x.Parent == folder.PublicId).OrderByDescending(x=>x.Title).ForEach(x => stack.Push(x));
            }

            return Task.FromResult(result);
        }
    }
}
