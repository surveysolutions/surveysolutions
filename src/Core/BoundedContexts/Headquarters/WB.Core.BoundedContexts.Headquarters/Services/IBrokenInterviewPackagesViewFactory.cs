using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.BrokenInterviewPackages;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public interface IBrokenInterviewPackagesViewFactory
    {
        BrokenInterviewPackagesView GetFilteredItems(BrokenInterviewPackageFilter filter);
        BrokenInterviewPackageExceptionTypesView GetExceptionTypes(int pageSize, string searchBy);
        BrokenInterviewPackage GetPackage(int id);
    }
}
