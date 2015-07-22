using System.Threading;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Plugins.Location;

namespace WB.Core.BoundedContexts.Tester.Services
{
    public interface IGpsLocationService
    {
        Task<MvxGeoLocation> GetLocation(CancellationToken cancellationToken);
    }
}