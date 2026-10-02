#nullable enable
using System;
using System.Linq;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Views.Account;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer
{
    [TestFixture]
    [TestOf(typeof(AccountListViewFactory))]
    public class AccountListViewFactoryTests
    {
        private DesignerDbContext db = null!;
        private AccountListViewFactory factory = null!;

        [SetUp]
        public void SetUp()
        {
            db = Create.InMemoryDbContext();
            factory = new AccountListViewFactory(db);
        }

        private DesignerIdentityUser AddUser(string name, string email, bool confirmed = true, DateTime? created = null, bool withRole = true)
        {
            var user = new DesignerIdentityUser
            {
                Id = Guid.NewGuid(),
                UserName = name,
                Email = email,
                EmailConfirmed = confirmed,
                CreatedAtUtc = created ?? DateTime.UtcNow
            };
            db.Users.Add(user);
            if (withRole)
                db.UserRoles.Add(new IdentityUserRole<Guid> { UserId = user.Id, RoleId = Guid.NewGuid() });
            db.SaveChanges();
            return user;
        }

        [Test]
        public void when_no_filters_should_return_all_users_with_roles_newest_first()
        {
            AddUser("old", "old@x.com", created: new DateTime(2020, 1, 1));
            AddUser("new", "new@x.com", created: new DateTime(2024, 1, 1));

            var view = factory.Load(new AccountListViewInputModel());

            view.TotalCount.Should().Be(2);
            view.Items.Select(x => x.UserName).Should().Equal("new", "old");
        }

        [Test]
        public void when_user_has_no_role_should_not_be_listed()
        {
            AddUser("norole", "n@x.com", withRole: false);

            factory.Load(new AccountListViewInputModel()).TotalCount.Should().Be(0);
        }

        [Test]
        public void when_filtering_by_name_should_return_exact_match_only()
        {
            AddUser("john", "j@x.com");
            AddUser("johnny", "jy@x.com");

            var view = factory.Load(new AccountListViewInputModel { Name = "john" });

            view.Items.Select(x => x.UserName).Should().Equal("john");
        }

        [Test]
        public void when_filtering_by_email_should_return_exact_match_only()
        {
            AddUser("a", "a@x.com");
            AddUser("b", "b@x.com");

            var view = factory.Load(new AccountListViewInputModel { Email = "b@x.com" });

            view.Items.Select(x => x.UserName).Should().Equal("b");
        }

        [Test]
        public void when_new_only_should_return_unconfirmed_users()
        {
            AddUser("confirmed", "c@x.com", confirmed: true);
            AddUser("pending", "p@x.com", confirmed: false);

            var view = factory.Load(new AccountListViewInputModel { IsNewOnly = true });

            view.Items.Select(x => x.UserName).Should().Equal("pending");
        }

        [Test]
        public void when_text_filter_should_match_user_name_or_email_case_insensitively()
        {
            AddUser("Alice", "x@a.com");
            AddUser("bob", "ALICE@y.com");
            AddUser("carol", "c@z.com");

            var view = factory.Load(new AccountListViewInputModel { Filter = "  alice " });

            view.Items.Select(x => x.UserName).Should().BeEquivalentTo("Alice", "bob");
        }

        [Test]
        public void when_filtering_by_role_should_not_throw()
        {
            AddUser("admin", "a@x.com");
            Assert.DoesNotThrow(() => factory.Load(
                new AccountListViewInputModel { Role = WB.Core.BoundedContexts.Designer.MembershipProvider.Roles.SimpleRoleEnum.Administrator }));
        }
        [Test]
        public void when_paging_should_skip_and_take_requested_page()
        {
            for (var i = 0; i < 5; i++)
                AddUser("u" + i, $"u{i}@x.com", created: new DateTime(2020, 1, 1).AddDays(i));

            var view = factory.Load(new AccountListViewInputModel { Page = 2, PageSize = 2 });

            view.TotalCount.Should().Be(5);
            view.Page.Should().Be(2);
            view.PageSize.Should().Be(2);
            view.Items.Select(x => x.UserName).Should().Equal("u2", "u1");
        }
    }
}

