namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.HealthCheck.Checks
{
    public interface IAtomicHealthCheck<T> where T:class 
    {
        T Check();
    }
}