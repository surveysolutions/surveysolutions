using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using System.Linq.Expressions;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services
{
    public class When_Interviewer_SignIn
    {
        [SetUp]
        public void Setup()
        {
            this.interviewStorageMock = new Mock<IPlainStorage<InterviewerIdentity>>();
            this.passwordHasher = new Mock<IPasswordHasher>();
            this.passwordHasher.Setup(ph => ph.Hash(password)).Returns(hashedPassword);
            this.passwordHasher.Setup(ph => ph.VerifyPassword(hashedPassword, password)).Returns(PasswordVerificationResult.Success);
            this.principal = Create.Service.InterviewerPrincipal(this.interviewStorageMock.Object, passwordHasher.Object);
        }

        private void SetupInterviewerIdentity(string name, string pass, string passwordHash)
        {
            this.interviewerIdentity = new InterviewerIdentity
            {
                Id = Id.g1.FormatGuid(),
                UserId = Id.g1,
                Name = name,
                PasswordHash = passwordHash
            };

            interviewStorageMock
                .Setup(storage => storage.Where(It.IsAny<Expression<Func<InterviewerIdentity, bool>>>()))
                .Returns(new List<InterviewerIdentity>
                {
                    interviewerIdentity
                });
        }

        private Mock<IPlainStorage<InterviewerIdentity>> interviewStorageMock;
        private Mock<IPasswordHasher> passwordHasher;
        private InterviewerPrincipal principal;
        private const string hashedPassword = "hashedPassword";
        private const string password = "password";

        private InterviewerIdentity interviewerIdentity;

        [Test]
        public void Should_be_able_to_SignIn_using_PasswordHash()
        {
            SetupInterviewerIdentity("Adams", null, hashedPassword);

            Assert.True(this.principal.SignIn("Adams", password, true));

            this.interviewStorageMock.Verify(v => v.Store(It.IsAny<InterviewerIdentity>()), 
                Times.Never, "Should not store user with updated password");

            this.passwordHasher.Verify(ph => ph.VerifyPassword(hashedPassword, password), Times.Once);
        }

        [Test]
        public void Should_be_able_to_SignIn_using_OldPasswordHash_and_update_hash()
        {
            SetupInterviewerIdentity("Adams", null, hashedPassword);

            this.interviewStorageMock.Setup(i => i.GetById(Id.g1.FormatGuid())).Returns(interviewerIdentity);

            this.passwordHasher.Setup(ph => ph.VerifyPassword(hashedPassword, password))
                .Returns(PasswordVerificationResult.SuccessRehashNeeded);

            this.passwordHasher.Setup(ph => ph.Hash(password)).Returns("NEWHASH");

            Assert.True(this.principal.SignIn("Adams", password, true));

            this.interviewStorageMock.Verify(v => v.Store(
                    It.Is<InterviewerIdentity>(u => u.PasswordHash == "NEWHASH")),
                 Times.Once, "Should store user with updated hash");
            
            this.passwordHasher.Verify(ph => ph.VerifyPassword(hashedPassword, password), Times.Once);
            this.passwordHasher.Verify(ph => ph.Hash(password), Times.Once);
        }

        [Test]
        public void Should_not_be_able_to_SignIn_using_wrong_username()
        {
            interviewStorageMock
              .Setup(storage => storage.Where(It.IsAny<Expression<Func<InterviewerIdentity, bool>>>()))
              .Returns(new List<InterviewerIdentity>());

            Assert.False(this.principal.SignIn("Adamsy", password, true));
        }

        [Test]
        public void Should_not_be_able_to_SignIn_using_wrong_passwordHash()
        {
            SetupInterviewerIdentity("Adamsy", null, hashedPassword);

            Assert.False(this.principal.SignIn("Adamsy", "wrong password", true));

            this.interviewStorageMock.Verify(v => v.Store(It.IsAny<InterviewerIdentity>()),
                Times.Never, "Should not store any user updates");

            this.passwordHasher.Verify(ph => ph.VerifyPassword(hashedPassword, "wrong password"), Times.Once);
        }

        [Test]
        public void Should_not_be_able_to_SignIn_using_wrong_password()
        {
            SetupInterviewerIdentity("Adamsy", password, null);

            Assert.False(this.principal.SignIn("Adamsy", "wrong password", true));

            this.interviewStorageMock.Verify(v => v.Store(It.IsAny<InterviewerIdentity>()),

            Times.Never, "Should not store any user updates");
        }
    }
}
