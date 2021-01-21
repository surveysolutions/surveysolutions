using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.XmlEncryption;

namespace WB.Infrastructure.AspNetCore.DataProtection
{
    public static class DataProtectionExtensions
    {
        public static void PersistWithPostgres(this IDataProtectionBuilder dataProtectionBuilder,
            string connectionString, string schema = "dataprotection")
        {
            dataProtectionBuilder.AddKeyManagementOptions(k =>
            {
                k.XmlRepository = new PostgresXmlRepository(connectionString, schema);
                k.XmlEncryptor = new NullXmlEncryptor();
            });
        }
    }
}
