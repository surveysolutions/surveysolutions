using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Designer.Services;
using WB.UI.Designer1.Extensions;


namespace WB.UI.Designer.Api
{
    [ResponseCache(NoStore = true)]
    [Authorize(Roles = "Administrator")]
    public class PublicFoldersApiController : Controller
    {
        private readonly IPublicFoldersStorage publicFoldersStorage;

        public PublicFoldersApiController(IPublicFoldersStorage publicFoldersStorage)
        {
            this.publicFoldersStorage = publicFoldersStorage;
        }

        public class TreeNode
        {
            public string key { get; set; }
            public string title { get; set; }
            public bool lazy { get; set; } = true;
            public bool folder { get; set; } = true;
        }

        [HttpGet]
        public List<TreeNode> GetFolders(Guid? parentId)
        {
            return this.publicFoldersStorage.GetSubFolders(parentId)
                    .Select(i => new TreeNode()
                    {
                        key = i.PublicId.ToString(),
                        title = i.Title
                    }).ToList();
        }

        [HttpGet]
        public List<TreeNode> GetRootFolders()
        {
            return this.publicFoldersStorage.GetRootFolders()
                .Select(i => new TreeNode()
                {
                    key = "root",
                    title = i.Title
                }).ToList();
        }

        public class CreateFolderModel
        {
            public Guid? ParentId { get; set; }
            public string Title { get; set; }
        }

        [HttpPost]
        public TreeNode CreateFolder(CreateFolderModel model)
        {
            var id = Guid.NewGuid();
            var userId = User.GetId();
            var folder = this.publicFoldersStorage.CreateFolder(id, model.Title, model.ParentId, userId);
            return new TreeNode()
            {
                key = folder.PublicId.ToString(),
                title = folder.Title
            };
        }

        public class RenameFolderModel
        {
            public Guid Id { get; set; }
            public string NewTitle { get; set; }
        }

        [HttpPost]
        public void RenameFolder(RenameFolderModel model)
        {
            this.publicFoldersStorage.RenameFolder(model.Id, model.NewTitle);
        }

        public class RemoveFolderModel
        {
            public Guid Id { get; set; }
        }

        [HttpPost]
        public void RemoveFolder(RemoveFolderModel model)
        {
            this.publicFoldersStorage.RemoveFolder(model.Id);
        }

        public class AssignFolderToQuestionnaireModel
        {
            public Guid QuestionnaireId { get; set; }
            public Guid? Id { get; set; }
        }

        [HttpPost]
        public void AssignFolderToQuestionnaire(AssignFolderToQuestionnaireModel model)
        {
            this.publicFoldersStorage.AssignFolderToQuestionnaire(model.QuestionnaireId, model.Id);
        }
    }
}
