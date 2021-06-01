using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Designer;
using WB.Core.BoundedContexts.Designer.Services;

namespace WB.UI.Designer.Areas.Admin.Controllers
{
    [ResponseCache(NoStore = true)]
    [Authorize(Roles = "Administrator")]
    [Area("Admin")]
    public class PublicFoldersController : Controller
    {
        private readonly IPublicFoldersStorage publicFoldersStorage;

        public PublicFoldersController(IPublicFoldersStorage publicFoldersStorage)
        {
            this.publicFoldersStorage = publicFoldersStorage;
        }

        public class TreeNode
        {
            public string? key { get; set; }
            public string? title { get; set; }
            public bool lazy { get; set; } = true;
            public bool folder { get; set; } = true;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<List<TreeNode>> GetFolders(Guid? parentId)
        {
            var subFoldersAsync = await this.publicFoldersStorage.GetSubFoldersAsync(parentId);
            return subFoldersAsync
                    .Select(i => new TreeNode
                    {
                        key = i.PublicId.ToString(),
                        title = i.Title
                    }).ToList();
        }

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
            public string? Title { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
            public string? NewTitle { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RenameFolder(RenameFolderModel model)
        {
            this.publicFoldersStorage.RenameFolder(model.Id, model.NewTitle ?? "");
            return Ok();
        }

        public class RemoveFolderModel
        {
            public Guid Id { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveFolder(RemoveFolderModel model)
        {
            this.publicFoldersStorage.RemoveFolder(model.Id);
            return Ok();
        }

        public class AssignFolderToQuestionnaireModel
        {
            public Guid QuestionnaireId { get; set; }
            public Guid? Id { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssignFolderToQuestionnaire(AssignFolderToQuestionnaireModel model)
        {
            this.publicFoldersStorage.AssignFolderToQuestionnaire(model.QuestionnaireId, model.Id);
            return Ok();
        }
    }
}
