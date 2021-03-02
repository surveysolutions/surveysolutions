using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate.AspNetCore.Serialization;
using HotChocolate.Execution;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql
{
    public class CompositeSerializer : DefaultHttpResultSerializer
    {
        public override async ValueTask SerializeAsync(
            IExecutionResult result,
            Stream stream,
            CancellationToken cancellationToken)
        {
            await base.SerializeAsync(result, stream, cancellationToken);
        }
    }
}
