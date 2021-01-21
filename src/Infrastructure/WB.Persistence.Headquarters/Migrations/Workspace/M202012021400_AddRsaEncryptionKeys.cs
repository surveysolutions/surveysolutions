using System.Security.Cryptography;
using FluentMigrator;
using Newtonsoft.Json;
using WB.Core.SharedKernels.DataCollection.Implementation;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Migration(2020_12_02_14_00)]
    public class M202012021400_AddRsaEncryptionKeys : Migration
    {
        public override void Up()
        {
            using var rsa = new RSACryptoServiceProvider(2048);
            var serializeObject = JsonConvert.SerializeObject(new
            {
                PublicKey = RSACryptoServiceProviderExtensions.ToXmlString(rsa, false),
                PrivateKey = RSACryptoServiceProviderExtensions.ToXmlString(rsa, true)
            });

            Execute.Sql($@"INSERT INTO appsettings (id, value) VALUES ('Encryption.RsaKeys', '{serializeObject}')
                ON CONFLICT (id) DO NOTHING;");
        }

        public override void Down()
        {
            Delete.FromTable(@"appsettings").Row(new { id = @"Encryption.RsaKeys" });
        }
    }
}
