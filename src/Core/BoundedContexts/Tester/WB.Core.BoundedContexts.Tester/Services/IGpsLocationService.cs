using System.Threading;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Plugins.Location;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Services
{
    public interface IGpsLocationService
    {
        Task<MvxGeoLocation> GetLocation(CancellationToken cancellationToken);
    }
}