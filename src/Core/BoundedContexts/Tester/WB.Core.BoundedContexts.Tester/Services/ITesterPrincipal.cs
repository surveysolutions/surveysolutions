using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.BoundedContexts.Tester.Services;

public interface ITesterPrincipal : IPrincipal
{
    void UseFakeIdentity();
    void RemoveFakeIdentity();

    bool IsFakeIdentity { get; }
}

 