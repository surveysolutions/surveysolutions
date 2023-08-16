#nullable enable
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Repositories;

public interface ICompanyLogoStorage
{
    public CompanyLogo? GetCompanyLogoByWorkspace(string keyName, string workspace);
}
