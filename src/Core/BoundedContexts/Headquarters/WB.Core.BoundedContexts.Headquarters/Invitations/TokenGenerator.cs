using System.Linq;
using System.Threading;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    public sealed class TokenGenerator
    {
        private const string Encode_32_Chars = "SXVNWRCK53MTFQP7A6HUBZ4GEID921L8";

        private static readonly ThreadLocal<char[]> _charBufferThreadLocal =
            new ThreadLocal<char[]>(() => new char[13]);

        private TokenGenerator()
        {
        }

        /// <summary>
        /// Returns a single instance of the <see cref="TokenGenerator"/>.
        /// </summary>
        public static TokenGenerator Instance { get; } = new TokenGenerator();

        /// <summary>
        /// Returns an ID. e.g: <c>0HLHI1F5INOFA</c>
        /// </summary>
        public string Generate(long id) => GenerateImpl(id);

        private static string GenerateImpl(long id)
        {
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
    }
}
