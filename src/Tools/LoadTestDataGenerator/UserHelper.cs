using Main.Core.Documents;
using Main.Core.Entities.SubEntities;

namespace LoadTestDataGenerator
{
    public static class UserHelper
    {
        public static UserLight ToUserLight(this UserDocument document)
        {
            return new UserLight(document.PublicKey, document.UserName);
        }
    }
}