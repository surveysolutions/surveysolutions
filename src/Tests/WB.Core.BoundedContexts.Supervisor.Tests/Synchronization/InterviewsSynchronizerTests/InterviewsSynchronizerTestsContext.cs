using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Moq.Protected;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation;

namespace WB.Core.BoundedContexts.Supervisor.Tests.Synchronization.InterviewsSynchronizerTests
{
    [Subject(typeof (InterviewsSynchronizer))]
    internal class InterviewsSynchronizerTestsContext {}
}