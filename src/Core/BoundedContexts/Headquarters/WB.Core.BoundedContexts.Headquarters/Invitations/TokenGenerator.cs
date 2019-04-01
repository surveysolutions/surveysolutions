using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    public sealed class TokenGenerator : ITokenGenerator
    {
        private readonly IPlainStorageAccessor<Invitation> invitationStorage;
        protected internal const string Encode_32_Chars = "23456789ABCDEFGHJKLMNPQRSTUVWXYZ";
        protected internal int tokenLength = 8;

        public TokenGenerator(IPlainStorageAccessor<Invitation> invitationStorage)
        {
            this.invitationStorage = invitationStorage;
        }

        public string GenerateUnique()
        {
            var tokens = Enumerable.Range(1, 10).Select(_ => GetRandomAlphanumericString(tokenLength)).ToArray();
            List<string> usedTokens = this.invitationStorage.Query(_ => _.Where(x => tokens.Contains(x.Token)).Select(x => x.Token).ToList());

            var availableTokens = tokens.Except(usedTokens).ToList();

            if (availableTokens.Any())
            {
                return availableTokens.First();
            }

            return GenerateUnique();
        }

        /// <summary>
        /// Returns an ID. e.g: <c>0HLHI1F5INOFA</c>
        /// </summary>
        public string Generate(long id) => GenerateImpl(id);

        private static string GenerateImpl(long id)
        {
            var _charBufferThreadLocal = new ThreadLocal<char[]>(() => new char[13]);

            var buffer = _charBufferThreadLocal.Value;

            buffer[0] = Encode_32_Chars[(int) (id >> 60) & 31];
            buffer[1] = Encode_32_Chars[(int) (id >> 55) & 31];
            buffer[2] = Encode_32_Chars[(int) (id >> 50) & 31];
            buffer[3] = Encode_32_Chars[(int) (id >> 45) & 31];
            buffer[4] = Encode_32_Chars[(int) (id >> 40) & 31];
            buffer[5] = Encode_32_Chars[(int) (id >> 35) & 31];
            buffer[6] = Encode_32_Chars[(int) (id >> 30) & 31];
            buffer[7] = Encode_32_Chars[(int) (id >> 25) & 31];
            buffer[8] = Encode_32_Chars[(int) (id >> 20) & 31];
            buffer[9] = Encode_32_Chars[(int) (id >> 15) & 31];
            buffer[10] = Encode_32_Chars[(int) (id >> 10) & 31];
            buffer[11] = Encode_32_Chars[(int) (id >> 5) & 31];
            buffer[12] = Encode_32_Chars[(int) id & 31];

            return new string(buffer, 0, buffer.Length).Substring(4);
        }

        public static string GetRandomAlphanumericString(int length)
        {
            return GetRandomString(length, Encode_32_Chars);
        }

        public static string GetRandomString(int length, IEnumerable<char> characterSet)
        {
            if (length < 0)
                throw new ArgumentException("length must not be negative", "length");
            if (length > int.MaxValue / 8) 
                throw new ArgumentException("length is too big", "length");
            if (characterSet == null)
                throw new ArgumentNullException("characterSet");
            var characterArray = characterSet.Distinct().ToArray();
            if (characterArray.Length == 0)
                throw new ArgumentException("characterSet must not be empty", "characterSet");

            var bytes = new byte[length * 8];
            new RNGCryptoServiceProvider().GetBytes(bytes);
            var result = new char[length];
            for (int i = 0; i < length; i++)
            {
                ulong value = BitConverter.ToUInt64(bytes, i * 8);
                result[i] = characterArray[value % (uint)characterArray.Length];
            }
            return new string(result);
        }
    }

    public interface ITokenGenerator
    {
        string GenerateUnique();
        string Generate(long id);
    }
}
