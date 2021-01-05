using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Implementation;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    public sealed class TokenGenerator : RandomStringGenerator, ITokenGenerator
    {
        private readonly IPlainStorageAccessor<Invitation> invitationStorage;
        private readonly IPlainStorageAccessor<ServerSettings> tenantSettings;

        internal int tokenLength = 8;

        public TokenGenerator(
            IPlainStorageAccessor<Invitation> invitationStorage,
            IPlainStorageAccessor<ServerSettings> tenantSettings)
        {
            this.invitationStorage = invitationStorage;
            this.tenantSettings = tenantSettings;
        }

        public string GenerateUnique()
        {
            var tokens = Enumerable.Range(1, 10).Select(_ => GetRandomString(tokenLength, Encode_32_Chars)).ToArray();
            List<string> usedTokens = this.invitationStorage.Query(_ => _.Where(x => tokens.Contains(x.Token)).Select(x => x.Token).ToList());

            var availableTokens = tokens.Except(usedTokens).ToList();

            if (availableTokens.Any())
            {
                return availableTokens.First();
            }

            return GenerateUnique();
        }

        public string Generate(QuestionnaireIdentity identity)
        {
            ServerSettings exportKey = this.tenantSettings.GetById(ServerSettings.PublicTenantIdKey);
            long hashCode;

            // cannot use identity.GetHashCode() as it return int value biased to the right side of long bit space
            // generated token will look quite same
            // to fix this and 
            unchecked
            {
                // large prime number to affect whole long bit space on version change
                // https://primes.utm.edu/curios/page.php/255455445544554553.html
                const long prime = 255455445544554553;

                hashCode = identity.Version.GetHashCode() * prime;
                hashCode = hashCode ^ (identity.QuestionnaireId.GetHashCode() * prime);

                // as a note - TenantPublicId is a string - so it will return different hash code across application reboot
                // this one line will ensure that different tenants do not receive same token
                hashCode = hashCode ^ (exportKey.Value.GetHashCode() * prime);
            }

            return GenerateImpl(hashCode);
        }

        private static string GenerateImpl(long id)
        {
            const int length = 13;
            var buffer = ArrayPool<char>.Shared.Rent(length);

            try
            {
                buffer[0] = Encode_32_Chars[(int)(id >> 60) & 31];
                buffer[1] = Encode_32_Chars[(int)(id >> 55) & 31];
                buffer[2] = Encode_32_Chars[(int)(id >> 50) & 31];
                buffer[3] = Encode_32_Chars[(int)(id >> 45) & 31];
                buffer[4] = Encode_32_Chars[(int)(id >> 40) & 31];
                buffer[5] = Encode_32_Chars[(int)(id >> 35) & 31];
                buffer[6] = Encode_32_Chars[(int)(id >> 30) & 31];
                buffer[7] = Encode_32_Chars[(int)(id >> 25) & 31];
                buffer[8] = Encode_32_Chars[(int)(id >> 20) & 31];
                buffer[9] = Encode_32_Chars[(int)(id >> 15) & 31];
                buffer[10] = Encode_32_Chars[(int)(id >> 10) & 31];
                buffer[11] = Encode_32_Chars[(int)(id >> 5) & 31];
                buffer[12] = Encode_32_Chars[(int)id & 31];

                return new string(buffer, 0, length).Substring(4);
            }
            finally
            {
                ArrayPool<char>.Shared.Return(buffer);
            }
        }
    }

    public interface ITokenGenerator
    {
        string GenerateUnique();
        string Generate(QuestionnaireIdentity identity);
    }
}
