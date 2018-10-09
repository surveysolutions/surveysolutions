using System;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.Infrastructure.CommandBus;


namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.AccountRepositoryTests
{
    [TestOf(typeof(DesignerAccountRepository))]
    internal class AccountRepositoryTests : AccountRepositoryTestsContext
    {
        [Test]
        public void when_account_repository_creating_new_user()
        {
            // arrange
            var validatedUserId = Guid.Parse("11111111111111111111111111111111");
            var fullName = "John Doe";
            var commandService = new Mock<ICommandService>();
            var accountRepository = CreateAccountRepository(commandService: commandService.Object);
            // act
            var validatedAccount = accountRepository.Create(validatedUserId, null, string.Empty, string.Empty, fullName);
            // assert
            Assert.That(validatedAccount.ProviderUserKey, Is.EqualTo(validatedUserId));
            Assert.That(validatedAccount.FullName, Is.EqualTo(fullName));
        }
    }
}
