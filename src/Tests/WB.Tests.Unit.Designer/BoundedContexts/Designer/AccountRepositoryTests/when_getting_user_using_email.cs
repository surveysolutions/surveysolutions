using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Views.Account;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.AccountRepositoryTests
{
    internal class when_getting_user_using_email : AccountRepositoryTestsContext
    {
        [Test]
        public void should_get_using_existing_email()
        {
            var accountViewFactory = new Mock<IAccountViewFactory>();
            var accountView = Mock.Of<IAccountView>();
            var userEmail = "a@test.com";

            accountViewFactory
                .Setup(x => x.Load(It.Is<AccountViewInputModel>(input => input.AccountEmail == userEmail)))
                .Returns(accountView);

            var repository = CreateAccountRepository(accountViewFactory: accountViewFactory.Object);

            var account = repository.GetByNameOrEmail(userEmail);
            Assert.That(account, Is.SameAs(accountView));
        }
    }
}