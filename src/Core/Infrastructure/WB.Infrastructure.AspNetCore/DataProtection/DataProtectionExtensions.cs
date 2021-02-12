using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.XmlEncryption;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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

        public static void EnablePostgresXmlRepositoryLogging(this IHost app)
        {
            PostgresXmlRepository.Logger = app.Services.GetRequiredService<ILogger<PostgresXmlRepository>>();
        }
    }
}
