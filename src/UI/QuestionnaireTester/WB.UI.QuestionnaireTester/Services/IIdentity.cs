namespace WB.UI.QuestionnaireTester.Services
{
    public interface IIdentity
    {
        bool IsAuthenticated { get; }
        string Name { get; }
        string Password { get; }
    }
}