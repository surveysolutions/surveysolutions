namespace WB.Core.SharedKernels.SurveyManagement.Services
{
    public interface IAndroidPackageReader
    {
        AndroidPackageInfo Read(string pathToApkFile);
    }
}