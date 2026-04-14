using System;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Tests.Abc;
using WB.UI.Designer.Services;

namespace WB.Tests.Unit.Designer.Services
{
    public class WebTesterServiceTests
    {
        [Test]
        public void should_delegate_token_creation_to_jwt_service()
        {
            var jwtServiceMock = new Mock<IJwtTokenService>();
            jwtServiceMock
                .Setup(x => x.GenerateWebTesterToken(It.IsAny<DesignerIdentityUser>(), It.IsAny<Guid>()))
                .Returns("jwt.token.value");

            var subj = new WebTesterService(jwtServiceMock.Object);

            var token = subj.CreateTestQuestionnaire(Id.g1);

            Assert.That(token, Is.EqualTo("jwt.token.value"));
            jwtServiceMock.Verify(x => x.GenerateWebTesterToken(null, Id.g1), Times.Once);
        }

        [Test]
        public void should_pass_user_to_jwt_service_when_provided()
        {
            var jwtServiceMock = new Mock<IJwtTokenService>();
            jwtServiceMock
                .Setup(x => x.GenerateWebTesterToken(It.IsAny<DesignerIdentityUser>(), It.IsAny<Guid>()))
                .Returns("jwt.token.value");

            var user = new DesignerIdentityUser();
            var subj = new WebTesterService(jwtServiceMock.Object);

            subj.CreateTestQuestionnaire(Id.g1, user);

            jwtServiceMock.Verify(x => x.GenerateWebTesterToken(user, Id.g1), Times.Once);
        }
    }
}
