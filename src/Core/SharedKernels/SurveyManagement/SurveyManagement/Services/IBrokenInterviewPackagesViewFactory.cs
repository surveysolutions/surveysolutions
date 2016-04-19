using WB.Core.SharedKernels.SurveyManagement.Views.BrokenInterviewPackages;

namespace WB.Core.SharedKernels.SurveyManagement.Services
{
    public interface IBrokenInterviewPackagesViewFactory
    {
        BrokenInterviewPackagesView GetFilteredItems(BrokenInterviewPackageFilter filter);
        BrokenInterviewPackageExceptionTypesView GetExceptionTypes(int pageSize, string searchBy);
    }
}
