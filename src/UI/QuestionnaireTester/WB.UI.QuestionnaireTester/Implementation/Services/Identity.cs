using WB.UI.QuestionnaireTester.Services;

namespace WB.UI.QuestionnaireTester.Implementation.Services
{
    public class Identity : IIdentity
    {
        public bool IsAuthenticated { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
    }
}