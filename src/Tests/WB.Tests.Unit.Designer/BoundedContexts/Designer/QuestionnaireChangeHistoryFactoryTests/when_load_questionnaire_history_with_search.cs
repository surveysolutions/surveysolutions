using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using FluentAssertions;
using Main.Core.Documents;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.MembershipProvider.Roles;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireChangeHistoryFactoryTests
{
    internal class when_load_questionnaire_history_with_search : QuestionnaireChangeHistoryFactoryTestContext
    {
        [OneTimeSetUp]
        public void Context()
        {
            var questionnaireDocument = Create.QuestionnaireDocument(children: new[]
            {
                Create.Group(children: new[]
                {
                    Create.Question(questionId: questionId)
                })
            });

            db = Create.InMemoryDbContext();

            db.Add(Create.QuestionnaireChangeRecord(
                questionnaireId: questionnaireId.FormatGuid(),
                targetId: questionId,
                targetType: QuestionnaireItemType.Question,
                action: QuestionnaireActionType.Update,
                targetTitle: "myvar"));

            db.Add(Create.QuestionnaireChangeRecord(
                questionnaireId: questionnaireId.FormatGuid(),
                targetId: Guid.NewGuid(),
                targetType: QuestionnaireItemType.Question,
                action: QuestionnaireActionType.Update,
                targetTitle: "othervar"));

            db.Add(Create.QuestionnaireChangeRecord(
                questionnaireId: questionnaireId.FormatGuid(),
                targetId: Guid.NewGuid(),
                targetType: QuestionnaireItemType.Section,
                action: QuestionnaireActionType.Add,
                targetTitle: "section1",
                reference: new[] { Create.QuestionnaireChangeReference(referenceTitle: "myvar") }));

            db.SaveChanges();

            var userManagerMock = new Mock<IUserManager>();
            userManagerMock
                .Setup(m => m.GetUsersInRoleAsync(SimpleRoleEnum.Administrator))
                .Returns(Task.FromResult((IList<DesignerIdentityUser>)new List<DesignerIdentityUser>()));

            factory = CreateQuestionnaireChangeHistoryFactory(
                db,
                Mock.Of<IPlainKeyValueStorage<QuestionnaireDocument>>(
                    _ => _.GetById(Moq.It.IsAny<string>()) == questionnaireDocument),
                userManager: userManagerMock.Object);
        }

        [Test]
        public async Task should_return_all_records_when_no_search()
        {
            var result = await factory.LoadAsync(questionnaireId, 1, 20, user);
            result.ChangeHistory.Count.Should().Be(3);
        }

        [Test]
        public async Task should_filter_by_target_title()
        {
            var result = await factory.LoadAsync(questionnaireId, 1, 20, user, "myvar");
            result.ChangeHistory.Count.Should().Be(2);
        }

        [Test]
        public async Task should_filter_by_reference_title()
        {
            var result = await factory.LoadAsync(questionnaireId, 1, 20, user, "section1");
            result.ChangeHistory.Count.Should().Be(1);
        }

        [Test]
        public async Task should_return_empty_when_no_match()
        {
            var result = await factory.LoadAsync(questionnaireId, 1, 20, user, "nomatch_xyz");
            result.ChangeHistory.Count.Should().Be(0);
        }

        [Test]
        public async Task should_search_case_insensitively()
        {
            var result = await factory.LoadAsync(questionnaireId, 1, 20, user, "MYVAR");
            result.ChangeHistory.Count.Should().Be(2);
        }

        [Test]
        public async Task should_preserve_search_on_result()
        {
            var result = await factory.LoadAsync(questionnaireId, 1, 20, user, "myvar");
            result.Search.Should().Be("myvar");
        }

        private QuestionnaireChangeHistoryFactory factory;
        private DesignerDbContext db;
        private readonly Guid questionnaireId = Guid.Parse("22222222222222222222222222222222");
        private readonly Guid questionId = Guid.Parse("33333333333333333333333333333333");

        private readonly ClaimsPrincipal user = new ClaimsPrincipal(new List<ClaimsIdentity>
        {
            new ClaimsIdentity(Mock.Of<IIdentity>(), new[]
            {
                new Claim(ClaimTypes.Role, SimpleRoleEnum.User.ToString()),
                new Claim(ClaimTypes.NameIdentifier, Id.gB.ToString())
            })
        });
    }
}
