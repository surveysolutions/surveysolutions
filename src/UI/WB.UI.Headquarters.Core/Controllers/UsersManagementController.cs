#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Infrastructure.Native.Utils;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models.Api;

namespace WB.UI.Headquarters.Controllers
{
    public class UserManagementListItem
    {
        public Guid UserId { get; set; }
        public string? UserName { get; set; }
        public DateTime CreationDate { get; set; }
        public string? Email { get; set; }
        public List<Workspace>? Workspaces { get; set; }
        public bool IsLocked { get; set; }
        public string? FullName { get; set; }
        public bool IsArchived { get; set; }
    }

    public class UsersManagementRequest : DataTableRequest
    {
        public string? WorkspaceName { get; set; }
        public UserRoles? Role { get; set; }
        public bool ShowArchived { get; set; }
        public bool ShowLocked { get; set; }
        public bool MissingWorkspace { get; set; }
    }
    
    [Authorize(Roles = "Administrator")]
    public class UsersManagementController : Controller
    {
        private readonly IUserRepository userRepository;

        public UsersManagementController(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        [ActivePage(MenuItem.UsersManagement)]
        [Authorize(Roles = "Administrator")]
        public IActionResult Index() => View();

        [Authorize(Roles = "Administrator")]
        public IActionResult List(UsersManagementRequest request)
        {
            var selectedRoleId = new [] { UserRoles.ApiUser, UserRoles.Headquarter}
                .Select(x => x.ToUserId()).ToArray();

            var query = this.userRepository.Users
                .Where(x => x.Roles.Any(r => selectedRoleId.Contains(r.Id)));

            var total = query.Count();

            if (request.Search?.Value != null)
            {
                query = query.Where(u => u.UserName.Contains(request.Search.Value));
            }
            
            if (request.Role != null)
            {
                var roleId = request.Role.Value.ToUserId();
                query = query.Where(u => u.Roles.Any(r => r.Id == roleId));
            }

            if (!request.ShowArchived)
            {
                query = query.Where(u => u.IsArchived == false);
            }

            if (!request.ShowLocked)
            {
                query = query.Where(u => !u.IsLockedByHeadquaters && !u.IsLockedBySupervisor);
            }

            if (request.WorkspaceName != null && request.MissingWorkspace == false)
            {
                query = query.Where(u => u.Workspaces.Any(w => w.Workspace.Name == request.WorkspaceName));
            }
            else if(request.MissingWorkspace)
            {
                query = query.Where(u => u.Workspaces.Count == 0);
            }
            
            var sortOrder = request.Order.IsNullOrEmpty() 
                ? $"{nameof(UserManagementListItem.UserName)} Desc" 
                : request.GetSortOrder();

            var list = query
                .OrderUsingSortExpression(sortOrder)
                .Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize)
                .Select(u => new UserManagementListItem
            {
                UserName = u.UserName,
                FullName = u.FullName,
                Email = u.Email,
                UserId = u.Id,
                CreationDate = u.CreationDate,
                Workspaces = u.Workspaces.Select(w => w.Workspace).ToList(),
                IsLocked = u.IsLockedByHeadquaters || u.IsLockedBySupervisor,
                IsArchived = u.IsArchived
            }).ToList();

            return new JsonResult(new DataTableResponse<UserManagementListItem>
            {
                Draw = request.Draw + 1,
                RecordsTotal = total,
                RecordsFiltered = query.Count(),
                Data = list
            });
        }
    }
}
