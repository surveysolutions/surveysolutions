using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts.Membership;
using WB.Core.BoundedContexts.Designer.Services;
using WB.UI.Shared.Web.Filters;


namespace WB.UI.Designer.Api
{
    [Authorize]
    [ApiNoCache]
    //[Authorize(Roles = "Administrator")]
    public class PublicFoldersApiController : ApiController
    {
        private readonly IPublicFoldersStorage publicFoldersStorage;
        private readonly IMembershipUserService userService;

        public PublicFoldersApiController(IPublicFoldersStorage publicFoldersStorage,
            IMembershipUserService userService)
        {
            this.publicFoldersStorage = publicFoldersStorage;
            this.userService = userService;
        }

        public class TreeNode
        {
            public Guid key { get; set; }
            public string title { get; set; }
            public bool lazy { get; set; } = true;
            public bool folder { get; set; } = true;
        }

        [HttpGet]
        public List<TreeNode> GetFolders(Guid parentId)
        {
            return this.publicFoldersStorage.GetSubFolders(parentId)
                    .Select(i => new TreeNode()
                    {
                        key = i.Id,
                        title = i.Title
                    }).ToList();
        }

        [HttpGet]
        public List<TreeNode> GetRootFolders()
        {
            return this.publicFoldersStorage.GetRootFolders()
                .Select(i => new TreeNode()
                {
                    key = i.Id,
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
            var userId = userService.WebUser.UserId;
            var folder = this.publicFoldersStorage.CreateFolder(id, model.Title, model.ParentId, userId);
            return new TreeNode()
            {
                key = folder.Id,
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
    }
}