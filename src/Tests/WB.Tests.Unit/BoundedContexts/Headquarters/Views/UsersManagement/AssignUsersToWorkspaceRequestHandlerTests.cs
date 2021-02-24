using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Core.Infrastructure.PlainStorage;
using WB.Tests.Abc;
using WB.UI.Headquarters.Code.Workspaces;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Models;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Views.UsersManagement
{
    [TestOf(typeof(AssignWorkspacesToUserModelHandler))]
    public class AssignUsersToWorkspaceRequestHandlerTests
    {
        private HqUser[] Users = null;
        private AssignWorkspacesToUserModelHandler Subject;
        private IPlainStorageAccessor<Workspace> workspaces;
        private ModelStateDictionary modelState = null;
        private Mock<IWorkspacesService> workspacesService;
        private List<Workspace> assignedWorkspaces = null;
        
        [SetUp]
        public void Setup()
        {
            Users = null;
            assignedWorkspaces = null;
            
            this.workspaces = Create.Storage.InMemoryPlainStorage<Workspace>();
            
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.Users).Returns(() => Users.AsQueryable().GetNhQueryable());
            userRepo.Setup(r => r.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns((Guid userId, CancellationToken token) =>
                {
                    return Task.FromResult(Users?.FirstOrDefault(u => u.Id == userId));
                });
                
            workspacesService = new Mock<IWorkspacesService>();
            workspacesService.Setup(w => w.AssignWorkspaces(It.IsAny<HqUser>(),
                It.IsAny<List<AssignUserWorkspace>>()
            )).Callback((HqUser user, List<AssignUserWorkspace> workspaces) =>
            {
                assignedWorkspaces = workspaces.Select(s => s.Workspace).ToList();
            });

            Subject = new AssignWorkspacesToUserModelHandler(
                workspaces,
                userRepo.Object, workspacesService.Object);

            modelState = new ModelStateDictionary();
        }

        [Test]
        public async Task should_return_error_for_non_existing_workspace()
        {
            StoreWorkspaces(Create.Entity.Workspace("test"), Create.Entity.Workspace("test2"));
            
            await Subject.Handle(new AssignWorkspacesToUserModelRequest(modelState,
                new AssignWorkspacesToUserModel
                {
                    Workspaces = new [] { new AssignWorkspaceInfo("abra") },
                    UserIds = new [] { Guid.NewGuid()}
                    
                }));
            
            Assert.That(modelState, Has.Property(nameof(ModelStateDictionary.IsValid)).EqualTo(false));
            Assert.That(modelState["Workspaces"].Errors[0].ErrorMessage.Contains("abra"));
        }

        [Test]
        public async Task should_not_return_error_for_disabled_workspace()
        {
            StoreWorkspaces(Create.Entity.Workspace("abra", disabled: true));

            Users = new[] { Create.Entity.HqUser(Id.g1,
                role: UserRoles.Headquarter,
                workspaces: new [] { "test" }) };

            await Subject.Handle(new AssignWorkspacesToUserModelRequest(modelState,
                new AssignWorkspacesToUserModel
                {
                    Workspaces = new [] { new AssignWorkspaceInfo("abra") },
                    UserIds = new[] { Id.g1 }
                }));
            
            Assert.That(modelState, Has.Property(nameof(ModelStateDictionary.IsValid)).EqualTo(true));
        }
        
        [Test]
        public async Task should_return_error_for_not_found_user()
        {
            StoreWorkspaces(Create.Entity.Workspace("abra"));
            
            await Subject.Handle(new AssignWorkspacesToUserModelRequest(modelState,
                new AssignWorkspacesToUserModel
                {
                    Workspaces = new [] { new AssignWorkspaceInfo("abra") },
                    UserIds = new [] { Id.g1}
                }));
            
            Assert.That(modelState, Has.Property(nameof(ModelStateDictionary.IsValid)).EqualTo(false));
            Assert.That(modelState["UserIds"].Errors.Any(e => e.ErrorMessage.Contains("User not found")));
        }

        [Test]
        public async Task should_return_error_for_archived_user()
        {
            StoreWorkspaces(Create.Entity.Workspace("abra"));
            Users = new[]
            {
                Create.Entity.HqUser(Id.g1, lockedBySupervisor: true)
            };

            await Subject.Handle(new AssignWorkspacesToUserModelRequest(modelState,
                new AssignWorkspacesToUserModel
                {
                    Workspaces = new[] { new AssignWorkspaceInfo("abra") },
                    UserIds = new[] { Id.g1 }
                }));

            Assert.That(modelState, Has.Property(nameof(ModelStateDictionary.IsValid)).EqualTo(false));
            Assert.That(modelState["UserIds"].Errors[0].ErrorMessage.Contains("User is locked"));
        }

        [TestCase(UserRoles.Interviewer)]
        [TestCase(UserRoles.Supervisor)]
        [TestCase(UserRoles.Administrator)]
        public async Task should_return_error_for_role(UserRoles role)
        {
            StoreWorkspaces(Create.Entity.Workspace("abra"));
            
            Users = new[] { Create.Entity.HqUser(Id.g1, role: role) };
            
            await Subject.Handle(new AssignWorkspacesToUserModelRequest(modelState,
                new AssignWorkspacesToUserModel
                {
                    Workspaces = new[] { new AssignWorkspaceInfo("abra") },
                    UserIds = new[] { Id.g1 }
                }));

            Assert.That(modelState, Has.Property(nameof(ModelStateDictionary.IsValid)).EqualTo(false));
            Assert.That(modelState["UserIds"].Errors.Count, Is.EqualTo(1));
        }
        
        [Test]
        public async Task should_assign_workspaces_replacing_existing()
        {
            StoreWorkspaces(Create.Entity.Workspace("puckl"), 
                Create.Entity.Workspace("test"), 
                Create.Entity.Workspace("cyber"));

            Users = new[] { Create.Entity.HqUser(Id.g1, 
                role: UserRoles.Headquarter,
                workspaces: new [] { "test" }) };

            await Subject.Handle(new AssignWorkspacesToUserModelRequest(modelState,
                new AssignWorkspacesToUserModel
                {
                    Workspaces = new[] { new AssignWorkspaceInfo("cyber"), new AssignWorkspaceInfo("puckl") },
                    UserIds = new[] { Id.g1 }
                }));

            Assert.That(modelState, Has.Property(nameof(ModelStateDictionary.IsValid)).EqualTo(true));
            
            this.workspacesService.Verify(w => w.AssignWorkspaces(It.IsAny<HqUser>(),
                It.IsAny<List<AssignUserWorkspace>>()), Times.Once);
            
            Assert.That(assignedWorkspaces.Select(w => w.Name).OrderBy(n => n), Is.EqualTo(new []
            {
                "cyber", "puckl"
            }.OrderBy(n => n)));
        }
  
        [Test]
        public async Task should_assign_workspaces_in_add_mode_add_workspace_to_existing()
        {
            StoreWorkspaces(
                Create.Entity.Workspace("puckl"), 
                Create.Entity.Workspace("test"), 
                Create.Entity.Workspace("cyber"));

            Users = new[] { Create.Entity.HqUser(Id.g1, 
                role: UserRoles.Headquarter,
                workspaces: new [] { "test" }) };

            await Subject.Handle(new AssignWorkspacesToUserModelRequest(modelState,
                new AssignWorkspacesToUserModel
                {
                    Mode = AssignWorkspacesMode.Add,
                    Workspaces = new[] { new AssignWorkspaceInfo("cyber") },
                    UserIds = new[] { Id.g1 }
                }));

            Assert.That(modelState, Has.Property(nameof(ModelStateDictionary.IsValid)).EqualTo(true));
            
            this.workspacesService.Verify(w => w.AssignWorkspaces(It.IsAny<HqUser>(),
                It.IsAny<List<AssignUserWorkspace>>()), Times.Once);
            
            Assert.That(assignedWorkspaces.Select(w => w.Name).OrderBy(n => n), Is.EqualTo(new []
            {
                "cyber", "test"
            }.OrderBy(n => n)));
        }  

        [Test]
        public async Task should_assign_workspaces_in_remove_mode_remove_workspace_to_existing()
        {
            StoreWorkspaces(Create.Entity.Workspace("puckl"), 
                Create.Entity.Workspace("test"), 
                Create.Entity.Workspace("cyber"));

            Users = new[] { Create.Entity.HqUser(Id.g1, 
                role: UserRoles.Headquarter,
                workspaces: new [] { "test", "cyber" }) };

            await Subject.Handle(new AssignWorkspacesToUserModelRequest(modelState,
                new AssignWorkspacesToUserModel
                {
                    Mode = AssignWorkspacesMode.Remove,
                    Workspaces = new[] { new AssignWorkspaceInfo("cyber") },
                    UserIds = new[] { Id.g1 }
                }));

            Assert.That(modelState, Has.Property(nameof(ModelStateDictionary.IsValid)).EqualTo(true));
            
            this.workspacesService.Verify(w => w.AssignWorkspaces(It.IsAny<HqUser>(),
                It.IsAny<List<AssignUserWorkspace>>()), Times.Once);
            
            Assert.That(assignedWorkspaces.Select(w => w.Name), Is.EqualTo(new []
            {
                "test"
            }));
        }

        private void StoreWorkspaces(params Workspace[] workspaces)
        {
            foreach (var workspace in workspaces)
            {
                this.workspaces.Store(workspace, workspace.Name);
            }
        }
    }
}
