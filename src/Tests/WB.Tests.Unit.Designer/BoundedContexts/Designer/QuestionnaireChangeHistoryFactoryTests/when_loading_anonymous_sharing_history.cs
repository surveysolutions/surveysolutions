using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using FluentAssertions;
using Main.Core.Documents;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.MembershipProvider.Roles;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireChangeHistoryFactoryTests
{
    [TestFixture]
    [TestOf(typeof(QuestionnaireChangeHistoryFactory))]
    internal class when_loading_anonymous_sharing_history : QuestionnaireChangeHistoryFactoryTestContext
    {
        [Test]
        public async Task should_allow_reverting_to_anonymous_sharing_records_without_questionnaire_snapshot()
        {
            var questionnaireId = Guid.NewGuid();
            var questionnaireDocument = Create.QuestionnaireDocument(id: questionnaireId);
            var dbContext = Create.InMemoryDbContext();
            dbContext.QuestionnaireChangeRecords.Add(Create.QuestionnaireChangeRecord(
                questionnaireId: questionnaireId.FormatGuid(),
                action: QuestionnaireActionType.AnonymousSharingEnabled,
                targetId: questionnaireId,
                targetType: QuestionnaireItemType.Questionnaire,
                targetTitle: questionnaireDocument.Title));
            dbContext.SaveChanges();

            var questionnaireStorage = new Mock<IPlainKeyValueStorage<QuestionnaireDocument>>();
            questionnaireStorage.Setup(x => x.GetById(It.IsAny<string>())).Returns(questionnaireDocument);

            var factory = CreateQuestionnaireChangeHistoryFactory(
                dbContext,
                questionnaireStorage.Object);

            var result = await factory.LoadAsync(questionnaireId, 1, 20, CreateUser());

            result!.ChangeHistory.Should().ContainSingle();
            result.ChangeHistory[0].HasRevertTo.Should().BeTrue();
        }

        private static IPrincipal CreateUser()
            => new ClaimsPrincipal(new List<ClaimsIdentity>
            {
                new(new Mock<IIdentity>().Object, new[]
                {
                    new Claim(ClaimTypes.Role, SimpleRoleEnum.Administrator.ToString()),
                    new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
                })
            });
    }
}
