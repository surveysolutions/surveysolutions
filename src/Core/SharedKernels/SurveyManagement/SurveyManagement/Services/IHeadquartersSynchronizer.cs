namespace WB.Core.SharedKernels.SurveyManagement.Services
{
    public interface IHeadquartersSynchronizer
    {
        void Pull(string login, string password);
    }
}