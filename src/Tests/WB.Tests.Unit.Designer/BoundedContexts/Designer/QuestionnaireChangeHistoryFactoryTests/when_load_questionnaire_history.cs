using System;
using System.Threading.Tasks;
using FluentAssertions;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.BoundedContexts.Designer;
using WB.Core.BoundedContexts.Designer.MembershipProvider.Roles;
using System.Collections.Generic;
using WB.Tests.Abc;
using System.Security.Claims;
using System.Security.Principal;
using NUnit.Framework;
using System.Linq;
using WB.Core.BoundedContexts.Designer.DataAccess;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireChangeHistoryFactoryTests
{
    internal class when_load_questionnaire_history : QuestionnaireChangeHistoryFactoryTestContext
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

            questionnaireChangeRecordStorage = Create.InMemoryDbContext();

            questionnaireChangeRecordStorage.Add(Create.QuestionnaireChangeRecord(
                    questionnaireId: questionnaireId.FormatGuid(),
                    targetId: questionId,
                    targetType: QuestionnaireItemType.Question,
                    action: QuestionnaireActionType.Clone,
                    reference: new[] { Create.QuestionnaireChangeReference() }));

            questionnaireChangeRecordStorage.Add(Create.QuestionnaireChangeRecord(
                    questionnaireId: questionnaireId.FormatGuid(),
                    targetType: QuestionnaireItemType.Question,
                    action: QuestionnaireActionType.Update,
                    targetId: questionId));

            questionnaireChangeRecordStorage.Add(Create.QuestionnaireChangeRecord(
                  questionnaireId: questionnaireId.FormatGuid(),
                  targetType: QuestionnaireItemType.Questionnaire,
                  action: QuestionnaireActionType.ImportToHq,
                  userId: adminUser.GetId(),
                  targetId: questionId));

            questionnaireChangeRecordStorage.Add(Create.QuestionnaireChangeRecord(
                  questionnaireId: questionnaireId.FormatGuid(),
                  targetType: QuestionnaireItemType.Questionnaire,
                  action: QuestionnaireActionType.ImportToHq,
                  userId: currentUser.GetId(),
                  targetId: questionId));

            questionnaireChangeRecordStorage.SaveChanges();

            var userManagerMock = new Mock<IUserManager>();

            userManagerMock                
                .Setup(m => m.GetUsersInRoleAsync(SimpleRoleEnum.Administrator))
                .Returns(Task.FromResult((IList<DesignerIdentityUser>)new List<DesignerIdentityUser>
                {
                    new DesignerIdentityUser() { Id = adminUser.GetId() }
                }));

            questionnaireChangeHistoryFactory =
                CreateQuestionnaireChangeHistoryFactory(
                    questionnaireChangeRecordStorage,
                    Mock.Of<IPlainKeyValueStorage<QuestionnaireDocument>>(
                            _ => _.GetById(Moq.It.IsAny<string>()) == questionnaireDocument),
                    userManager: userManagerMock.Object);
        }

        private async Task BecauseOf(bool isAdmin = true)
            => result = await questionnaireChangeHistoryFactory.LoadAsync(questionnaireId, 1, 20, isAdmin ? adminUser : currentUser);

        [Test]
        public async Task should_return_4_hostorical_records_for_admin()
        {
            await BecauseOf(true);
            result.ChangeHistory.Count.Should().Be(4);
        }

        [Test]
        public async Task should_return_3_historical_records_for_user()
        {
            await BecauseOf(false);
            result.ChangeHistory.Count.Should().Be(3);
        }

        [Test]
        public async Task should_last_historical_record_be_clone()
        {
            await BecauseOf();
            result.ChangeHistory.Last().ActionType.Should().Be(QuestionnaireActionType.Clone);
        }

        [Test]
        public async Task should_last_historical_has_parent_id()
        {
            await BecauseOf();
            result.ChangeHistory.Last().TargetParentId.Should().NotBeNull();
        }

        [Test]
        public async Task should_last_historical_has_one_reference()
        {
            await BecauseOf();
            result.ChangeHistory.Last().HistoricalRecordReferences.Count.Should().Be(1);
        }

        [Test]
        public async Task should_second_historical_record_be_clone()
        {
            await BecauseOf();
            result.ChangeHistory[2].ActionType.Should().Be(QuestionnaireActionType.Update);
        }

        private QuestionnaireChangeHistoryFactory questionnaireChangeHistoryFactory;
        private DesignerDbContext questionnaireChangeRecordStorage;
        private Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private QuestionnaireChangeHistory result;

        private ClaimsPrincipal currentUser = new ClaimsPrincipal(new List<ClaimsIdentity>
        {
            new ClaimsIdentity(Mock.Of<IIdentity>(), new [] {
                new Claim(ClaimTypes.Role, SimpleRoleEnum.User.ToString()),
                new Claim(ClaimTypes.NameIdentifier, Id.gB.ToString()) }
            )
        });
        private ClaimsPrincipal adminUser = new ClaimsPrincipal(new List<ClaimsIdentity>
        {
            new ClaimsIdentity(Mock.Of<IIdentity>(), new [] {
                new Claim(ClaimTypes.Role, SimpleRoleEnum.Administrator.ToString()),
                new Claim(ClaimTypes.NameIdentifier, Id.gA.ToString()) }
            )
        });
    }
}
