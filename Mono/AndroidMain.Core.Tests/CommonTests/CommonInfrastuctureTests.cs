using System;
using System.Linq;
using Android.Content;
using AndroidNcqrs.Eventing.Storage.SQLite;
using Main.Core;
using Main.Core.Documents;
using Main.Core.Services;
using NUnit.Framework;
using Ncqrs;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using Ninject;
using RavenQuestionnaire.Core.Tests.Utils;
using Tests;

namespace AndroidMain.Core.Tests.CommonTests
{
	[TestFixture]
	public class CommonInfrastuctureTests
	{
		private IKernel _kernel;

		[SetUp]
		public void SetUp()
		{
			TestsContext.Context.DeleteDatabase(DataBaseHelper.DATABASE_NAME);

			var registry = new TestsRegistry();

			_kernel = new StandardKernel();

			var appContext = TestsContext.CurrentContext;

			var store = new SQLiteEventStore(TestsContext.CurrentContext);
			_kernel.Bind<IEventStore>().ToConstant(store).InSingletonScope();

			_kernel.Bind<IFileStorageService>().To<FakeFileStorage>();

			_kernel.Load(new FakeCore());

			NCQRSInit.Init(_kernel);
		}

		//[Test, Ignore]
		//public void should_desirialize_event()
		//{
		//    var storedEvent = StoredEvent.GetCreateTemplateEvent();

		//    Assert.NotNull(storedEvent);

		//    Assert.That(storedEvent.EventIdentifier, 
		//        Is.EqualTo(Guid.Parse("6de866ac-2a8d-4dc9-8a85-bf425b318caa")));
		//}

		[Test]
		public void store_template_event_in_db()
		{
			var storedEvent = StoredEvent.GetCreateTemplateEvent();

			var store = NcqrsEnvironment.Get<IEventStore>();

			Assert.NotNull(store);

			var eventStream = GetStream(storedEvent);

			store.Store(eventStream);

			var storedEvents = store.ReadFrom(storedEvent.EventSourceId, long.MinValue, long.MinValue);
			
			Assert.That(storedEvents.Count(), Is.EqualTo(1));
		}

		private UncommittedEventStream GetStream(StoredEvent storedEvent)
		{
			var @event = new UncommittedEvent(storedEvent.EventIdentifier,
			                                  storedEvent.EventSourceId, storedEvent.EventSequence, 1,
			                                  storedEvent.EventTimeStamp, storedEvent.Data, new Version(1, 0));

			var result = new UncommittedEventStream(storedEvent.EventSourceId);
			result.Append(@event);

			return result;
		}
	}

	public class TestsRegistry : CoreRegistry
	{

	}

	public class FakeFileStorage : IFileStorageService
	{
		public void DeleteFile(string filename)
		{
			
		}

		public FileDescription RetrieveFile(string filename)
		{
			return new FileDescription();
		}

		public FileDescription RetrieveThumb(string filename)
		{
			return new FileDescription();
		}

		public void StoreFile(FileDescription file)
		{
			
		}
	}
}