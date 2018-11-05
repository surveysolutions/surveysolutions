using System.Security.Cryptography;
using FluentMigrator;
using Newtonsoft.Json;

namespace WB.UI.Headquarters.Migrations.PlainStore
{
    [Migration(201810101543)]
    public class M201810101543_AddRsaEncryptionKeys : Migration
    {
        public override void Up()
        {
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                var serializeObject = JsonConvert.SerializeObject(new
                {
                    PublicKey = rsa.ToXmlString(false),
                    PrivateKey = rsa.ToXmlString(true)
                });

                Insert.IntoTable(@"appsettings").Row(new { id = @"Encryption.RsaKeys", value = serializeObject });
            }
            
        }

        public override void Down()
        {
            Delete.FromTable(@"appsettings").Row(new {id = @"Encryption.RsaKeys" });
        }
    }
}
