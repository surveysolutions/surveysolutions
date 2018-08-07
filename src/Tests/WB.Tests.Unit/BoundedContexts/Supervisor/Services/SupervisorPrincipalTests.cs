using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Supervisor.Services.Implementation;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Supervisor.Services
{
    [TestOf(typeof(SupervisorPrincipal))]
    public class SupervisorPrincipalTests
    {
        [Test]
        public void should_not_allow_signin_with_wrong_password()
        {
            var login = "supervisor";
            var password = "password";
            var hashedPassword = "hash";
            var userId = "sv";

            var identityStorage = new InMemoryPlainStorage<SupervisorIdentity>();
            var supervisorIdentity = Create.Other.SupervisorIdentity(id: userId, userName: login, passwordHash: hashedPassword);
            identityStorage.Store(supervisorIdentity);

            var passwordHash = Mock.Of<IPasswordHasher>(x => x.VerifyPassword(hashedPassword, password) == PasswordVerificationResult.Success &&
                                                             x.Hash(password) == hashedPassword &&
                                                             x.Hash("wrong") == "wrongHash");

            var principal = CreatePrincipal(identityStorage, passwordHash);

            // Act
            bool signIn = principal.SignIn(login, "wrong", false);

            // Assert
            Assert.That(signIn, Is.False);
            Assert.That(principal.CurrentUserIdentity, Is.Null);
            Assert.That(principal.IsAuthenticated, Is.False);

            var identity = identityStorage.GetById(userId);
            Assert.That(identity.PasswordHash, Is.Not.EqualTo("wrongHash"));
        }

        private SupervisorPrincipal CreatePrincipal(IPlainStorage<SupervisorIdentity> usersStorage = null,
            IPasswordHasher passwordHasher = null)
        {
           return new SupervisorPrincipal(usersStorage ?? new InMemoryPlainStorage<SupervisorIdentity>(),
                passwordHasher ?? Mock.Of<IPasswordHasher>());
        }
    }
}
