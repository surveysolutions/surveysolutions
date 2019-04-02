using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.Infrastructure.Implementation;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Invitations
{
    [TestOf(typeof(TokenGenerator))]
    public class TokenGeneratorTests
    {
        [Test]
        public void When_generating_unique_one_letter_token()
        {
            var invitationStorage = new InMemoryPlainStorageAccessor<Invitation>();
            for (int i = 0; i < TokenGenerator.Encode_32_Chars.Length-1; i++)
            {
                var invitation = Create.Entity.Invitation(i + 1, Create.Entity.Assignment(), TokenGenerator.Encode_32_Chars.Substring(i, 1));
                invitationStorage.Store(invitation, invitation.Id);
            }

            var generator = Create.Service.TokenGenerator(1, invitationStorage: invitationStorage);

            var token = generator.GenerateUnique();

            Assert.That(token, Is.EqualTo("Z"));
        }
    }
}
