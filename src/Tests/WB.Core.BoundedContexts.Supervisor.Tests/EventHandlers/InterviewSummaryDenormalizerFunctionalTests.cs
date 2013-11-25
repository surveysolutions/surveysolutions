using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Microsoft.Practices.ServiceLocation;
using Moq;
using NUnit.Framework;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Supervisor.EventHandler;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Supervisor.Tests.EventHandlers
{
    [TestFixture]
    public class InterviewSummaryDenormalizerFunctionalTests
    {
        [Test]
        public void GeneralTest()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);

            var target = new InterviewSummaryDenormalizerFunctional((id) => null, (item) => { }, (item) => { },
                new Mock<IVersionedReadSideRepositoryWriter<QuestionnaireBrowseItem>>().Object,
                new Mock<IReadSideRepositoryWriter<UserDocument>>().Object);

            var bus = new InProcessEventBus();
            target.RegisterHandlersInOldFashionNcqrsBus(bus);
        }
    }
}
