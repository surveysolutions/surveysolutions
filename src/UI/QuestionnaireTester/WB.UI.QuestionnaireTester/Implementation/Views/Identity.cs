using WB.Core.GenericSubdomains.Utils.Services;

namespace WB.UI.QuestionnaireTester.Implementation.Views
{
    public class Identity : IIdentity
    {
        public bool IsAuthenticated { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
    }
}