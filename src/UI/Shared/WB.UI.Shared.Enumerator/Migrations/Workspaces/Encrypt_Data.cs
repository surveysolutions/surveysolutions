using WB.Core.SharedKernels.Enumerator.Services;
using WB.UI.Shared.Enumerator.Services;

namespace WB.UI.Shared.Enumerator.Migrations.Workspaces
{
    [Migration(201904051420)]
    public class Encrypt_Data : IMigration
    {
        private readonly IApplicationCypher applicationCypher;

        public Encrypt_Data(IApplicationCypher applicationCypher)
        {
            this.applicationCypher = applicationCypher;
        }

        public void Up()
        {
            applicationCypher.EncryptAppData();
        }
    }
}
