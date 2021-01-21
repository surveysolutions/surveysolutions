using System;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Tests.Abc;
using WB.UI.Headquarters.Code.UsersManagement;
using WB.UI.Headquarters.Models.Api;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Views.UsersManagement
{
    public class UsersManagementRequestHandlerTests
    {
        private HqUser[] Users = null;
        private UsersManagementRequestHandler Subject;

        [SetUp]
        public void Setup()
        {
            var mock = new Mock<IUserRepository>();
            mock.Setup(r => r.Users).Returns(() => Users.AsQueryable().GetNhQueryable());

            Subject = new UsersManagementRequestHandler(mock.Object);
        }

        [Test]
        public async Task should_return_filtering_and_total_users_count()
        {
            Users = new[] {
                Create.Entity.HqUser(role: UserRoles.Interviewer),
                Create.Entity.HqUser(role: UserRoles.Headquarter),
                Create.Entity.HqUser(role: UserRoles.Headquarter),
                Create.Entity.HqUser(role: UserRoles.ApiUser)
            };

            var response = await Subject.Handle(new UsersManagementRequest
            {
                Length = 2,
                Role = UserRoles.Headquarter
            });

            Assert.That(response.RecordsTotal, Is.EqualTo(4));
            Assert.That(response.RecordsFiltered, Is.EqualTo(2)); // filtered to HQ users
        }
        
 
        [Test]
        public async Task should_return_locked_users_by_request()
        {
            Users = new[] {
                Create.Entity.HqUser(role: UserRoles.Headquarter, lockedBySupervisor: true),
                Create.Entity.HqUser(role: UserRoles.Headquarter, isLockedByHQ: true),
                Create.Entity.HqUser(role: UserRoles.ApiUser)
            };

            var response = await Subject.Handle(new UsersManagementRequest
            {
                Filter = UserManagementFilter.Locked,
                Length = 10
            });
            
            Assert.That(response.RecordsFiltered, Is.EqualTo(2));
        }
        
        [Test]
        public async Task should_return_users_without_workspaces_only_by_request()
        {
            Users = new[] {
                Create.Entity.HqUser(role: UserRoles.Headquarter, workspaces: new [] {"test"}),
                Create.Entity.HqUser(role: UserRoles.Headquarter, workspaces: new [] {"test"}),
                Create.Entity.HqUser(userId: Id.g1, role: UserRoles.ApiUser, workspaces: Array.Empty<string>())
            };

            var response = await Subject.Handle(new UsersManagementRequest
            {
                Filter = UserManagementFilter.WithMissingWorkspace,
                Length = 10
            });
            
            Assert.That(response.RecordsFiltered, Is.EqualTo(1));
            Assert.That(response.Data.First().UserId, Is.EqualTo(Id.g1));
        }     
        
        [Test]
        public async Task should_return_users_filtered_by_workspace()
        {
            Users = new[] {
                Create.Entity.HqUser(role: UserRoles.Headquarter, workspaces: new [] {"test"}),
                Create.Entity.HqUser(Id.g1,role: UserRoles.Headquarter, workspaces: new [] {"mem"}),
                Create.Entity.HqUser(role: UserRoles.ApiUser)
            };

            var response = await Subject.Handle(new UsersManagementRequest
            {
                WorkspaceName = "mem",
                Length = 10
            });
            
            Assert.That(response.RecordsFiltered, Is.EqualTo(1));
            Assert.That(response.Data.First().UserId, Is.EqualTo(Id.g1));
        }

        [Test]
        public async Task paging_should_not_affect_filtered_count()
        {
            Users = new[] {
                Create.Entity.HqUser(role: UserRoles.Headquarter, workspaces: new [] {"test"}),
                Create.Entity.HqUser(role: UserRoles.Headquarter, workspaces: new [] {"test"}),
                Create.Entity.HqUser(role: UserRoles.Headquarter, workspaces: new [] {"test"}),
                Create.Entity.HqUser(role: UserRoles.Headquarter, workspaces: new [] {"test"}),
                Create.Entity.HqUser(Id.g1,role: UserRoles.Headquarter, workspaces: new [] {"mem"}),
                Create.Entity.HqUser(role: UserRoles.ApiUser)
            };

            var response = await Subject.Handle(new UsersManagementRequest
            {
                WorkspaceName = "test",
                Length = 2
            });

            Assert.That(response.RecordsTotal, Is.EqualTo(Users.Length));
            Assert.That(response.RecordsFiltered, Is.EqualTo(4));

            response = await Subject.Handle(new UsersManagementRequest
            {
                WorkspaceName = "test",
                Length = 2,
                Start = 2
            });

            Assert.That(response.RecordsTotal, Is.EqualTo(Users.Length));
            Assert.That(response.RecordsFiltered, Is.EqualTo(4));
        }

        [TestCase("tesla", ExpectedResult = 0, Description = "Search by login")]
        [TestCase("smoorphik", ExpectedResult = 1, Description = "Search by workspace name")]
        [TestCase("jenga", ExpectedResult = 2, Description = "Search by workspace display name")]
        public async Task<int> should_able_to_search_users(string search)
        {
            Users = new[] {
                Create.Entity.HqUser(Id.g1, userName: "teslaCoins", role: UserRoles.Headquarter),
                Create.Entity.HqUser(Id.g2, role: UserRoles.Headquarter, workspaces: new [] {"verysmoorphik"}),
                Create.Entity.HqUser(Id.g3, role: UserRoles.Headquarter),
            };

            foreach (var hqUser in Users)
            {
                hqUser.PhoneNumber = "";
                hqUser.Email = "";
            }

            Users[2].Workspaces.First().Workspace.DisplayName = "jengaworld";
            
            var response = await Subject.Handle(new UsersManagementRequest
            {
                Search = new DataTableRequest.SearchInfo() { Value = search },
                Length = 1
            });
            
            Assert.That(response.Data.Count(), Is.EqualTo(1));
            var user = response.Data.First();

            for (int i = 0; i < Users.Length; i++)
            {
                if (Users[i].Id == user.UserId)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
