using Main.Core;
using Main.Core.EventHandlers;
using Main.Core.Events.User;
using NUnit.Framework;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ninject;

namespace RavenQuestionnaire.Core.Tests.Utils
{
	class FakeCore : CoreRegistry
	{
		
	}

	[TestFixture]
	public class NinjectMappingsSmokeTest
	{
		[Test]
		public void ninject_should_resolve_interface_registered_by_convensions()
		{
			var mainCoreModule = new FakeCore();

			var kernel = new StandardKernel(mainCoreModule);

			var handler = kernel.Get<IEventHandler<NewUserCreated>>();

			Assert.True(handler is UserDenormalizer);
		}
	}
}