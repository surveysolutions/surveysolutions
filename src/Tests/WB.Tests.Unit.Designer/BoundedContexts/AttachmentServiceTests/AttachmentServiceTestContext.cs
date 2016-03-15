using System;
using System.Security.Cryptography;

namespace WB.Tests.Unit.Designer.BoundedContexts.AttachmentServiceTests
{
    internal class AttachmentServiceTestContext
    {
        public static string GetHash(byte[] binaryContent)
        {
            byte[] hash;
            using (SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider())
            {
                hash = sha1.ComputeHash(binaryContent);
            }
            return BitConverter.ToString(hash).Replace("-", string.Empty);
        }
    }
}