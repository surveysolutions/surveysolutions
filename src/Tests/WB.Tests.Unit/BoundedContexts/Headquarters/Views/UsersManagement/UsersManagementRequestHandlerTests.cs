using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
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

            Subject = new UsersManagementRequestHandler(mock.Object, Mock.Of<IAuthorizedUser>(u => u.IsAdministrator == true));
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

        [Test]
        public async Task when_sorting_by_fullname_ascending_null_and_empty_string_sort_together()
        {
            var userNullFullName = Create.Entity.HqUser(Id.g1, role: UserRoles.Headquarter);
            userNullFullName.FullName = null;

            var userEmptyFullName = Create.Entity.HqUser(Id.g2, role: UserRoles.Headquarter);
            userEmptyFullName.FullName = "";

            var userAlice = Create.Entity.HqUser(Id.g3, role: UserRoles.Headquarter);
            userAlice.FullName = "Alice";

            var userZoe = Create.Entity.HqUser(Id.g4, role: UserRoles.Headquarter);
            userZoe.FullName = "Zoe";

            Users = new[] { userZoe, userNullFullName, userAlice, userEmptyFullName };

            var response = await Subject.Handle(new UsersManagementRequest
            {
                Length = 10,
                Order = new List<DataTableRequest.SortOrder>
                {
                    new DataTableRequest.SortOrder { Name = "FullName", Dir = OrderDirection.Asc }
                }
            });

            var resultNames = response.Data.Select(u => u.FullName).ToList();

            // null and "" both coalesce to "" so they sort before "Alice" and "Zoe"
            Assert.That(resultNames.IndexOf("Alice"), Is.LessThan(resultNames.IndexOf("Zoe")));
            Assert.That(resultNames.IndexOf("Alice"), Is.GreaterThan(resultNames.IndexOf(null)));
            Assert.That(resultNames.IndexOf("Alice"), Is.GreaterThan(resultNames.IndexOf("")));
        }

        [Test]
        public async Task when_sorting_by_fullname_descending_null_and_empty_string_sort_together()
        {
            var userNullFullName = Create.Entity.HqUser(Id.g1, role: UserRoles.Headquarter);
            userNullFullName.FullName = null;

            var userEmptyFullName = Create.Entity.HqUser(Id.g2, role: UserRoles.Headquarter);
            userEmptyFullName.FullName = "";

            var userAlice = Create.Entity.HqUser(Id.g3, role: UserRoles.Headquarter);
            userAlice.FullName = "Alice";

            var userZoe = Create.Entity.HqUser(Id.g4, role: UserRoles.Headquarter);
            userZoe.FullName = "Zoe";

            Users = new[] { userZoe, userNullFullName, userAlice, userEmptyFullName };

            var response = await Subject.Handle(new UsersManagementRequest
            {
                Length = 10,
                Order = new List<DataTableRequest.SortOrder>
                {
                    new DataTableRequest.SortOrder { Name = "FullName", Dir = OrderDirection.Desc }
                }
            });

            var resultNames = response.Data.Select(u => u.FullName).ToList();

            // descending: "Zoe" first, then "Alice", then null/"" at the end
            Assert.That(resultNames.IndexOf("Zoe"), Is.LessThan(resultNames.IndexOf("Alice")));
            Assert.That(resultNames.IndexOf("Alice"), Is.LessThan(resultNames.IndexOf(null)));
            Assert.That(resultNames.IndexOf("Alice"), Is.LessThan(resultNames.IndexOf("")));
        }

        [Test]
        public async Task when_sorting_by_username_ascending_should_order_by_username()
        {
            Users = new[]
            {
                Create.Entity.HqUser(Id.g1, userName: "charlie", role: UserRoles.Headquarter),
                Create.Entity.HqUser(Id.g2, userName: "alice",   role: UserRoles.Headquarter),
                Create.Entity.HqUser(Id.g3, userName: "bob",     role: UserRoles.Headquarter),
            };

            var response = await Subject.Handle(new UsersManagementRequest
            {
                Length = 10,
                Order = new List<DataTableRequest.SortOrder>
                {
                    new DataTableRequest.SortOrder { Name = "UserName", Dir = OrderDirection.Asc }
                }
            });

            var userNames = response.Data.Select(u => u.UserName).ToList();
            Assert.That(userNames, Is.EqualTo(new[] { "alice", "bob", "charlie" }));
        }

        [Test]
        public async Task when_sorting_by_username_descending_should_order_by_username_descending()
        {
            Users = new[]
            {
                Create.Entity.HqUser(Id.g1, userName: "charlie", role: UserRoles.Headquarter),
                Create.Entity.HqUser(Id.g2, userName: "alice",   role: UserRoles.Headquarter),
                Create.Entity.HqUser(Id.g3, userName: "bob",     role: UserRoles.Headquarter),
            };

            var response = await Subject.Handle(new UsersManagementRequest
            {
                Length = 10,
                Order = new List<DataTableRequest.SortOrder>
                {
                    new DataTableRequest.SortOrder { Name = "UserName", Dir = OrderDirection.Desc }
                }
            });

            var userNames = response.Data.Select(u => u.UserName).ToList();
            Assert.That(userNames, Is.EqualTo(new[] { "charlie", "bob", "alice" }));
        }

        [Test]
        public async Task when_no_sort_order_specified_should_return_results_without_sorting()
        {
            Users = new[]
            {
                Create.Entity.HqUser(Id.g1, userName: "charlie", role: UserRoles.Headquarter),
                Create.Entity.HqUser(Id.g2, userName: "alice",   role: UserRoles.Headquarter),
                Create.Entity.HqUser(Id.g3, userName: "bob",     role: UserRoles.Headquarter),
            };

            var response = await Subject.Handle(new UsersManagementRequest
            {
                Length = 10
            });

            Assert.That(response.Data.Count(), Is.EqualTo(3));
        }
    }
}
