using Ionic.Zip;
using Moq;
using System;
using Ncqrs;
using Ncqrs.Eventing.Storage;
using Newtonsoft.Json;
using Ninject;
using System.IO;
using System.Web;
using Questionnaire.Core.Web.Export;
using Raven.Client;
using NUnit.Framework;
using RavenQuestionnaire.Core;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Events;
using RavenQuestionnaire.Web.Utils;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Views.Event;
using RavenQuestionnaire.Core.Views.ClientSettings;
using RavenQuestionnaire.Core.ClientSettingsProvider;


namespace RavenQuestionnaire.Web.Tests.Utils
{
    [TestFixture]
    public class ExportImportEventTest
    {
        public Mock<IViewRepository> viewRepositoryMock { get; set; }

        public Mock<IMemoryCommandInvoker>  invoker { get; set; }

        public Mock<IClientSettingsProvider> clientProvider { get; set; }

        [SetUp]
        public void CreateObjects()
        {
            viewRepositoryMock = new Mock<IViewRepository>();
            clientProvider = new Mock<IClientSettingsProvider>();
            invoker = new Mock<IMemoryCommandInvoker>();
        }

        [Test]
        public void When_FileImport()
        {
            var synchronizer = new Mock<IEventSync>();
            var postedfile = new Mock<HttpPostedFileBase>();
            var outputStream = new MemoryStream();
            var data = new ZipFileData
            {
                ClientGuid = Guid.NewGuid()
            };
            using (var zip = new ZipFile())
            {
                var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
                zip.AddEntry(string.Format("backup-{0}.txt", DateTime.Now.ToString().Replace(" ", "_")),
                                            JsonConvert.SerializeObject(data, Formatting.Indented, settings));
                zip.Save(outputStream);
            }
            outputStream.Seek(0, SeekOrigin.Begin);
            postedfile.Setup(f => f.ContentLength).Returns(8192).Verifiable();
            postedfile.Setup(f => f.ContentType).Returns("application/zip").Verifiable();
            postedfile.Setup(f => f.FileName).Returns("event.zip").Verifiable();
            postedfile.Setup(f => f.InputStream).Returns(outputStream);

            var events = new ExportImportEvent(clientProvider.Object, synchronizer.Object);
            events.Import(postedfile.Object);

            synchronizer.Verify(x => x.WriteEvents(It.IsAny<IEnumerable<AggregateRootEventStream>>()), Times.Once());

        }

        [Test]
        public void When_EventsExport()
        {
         //   Mock<ICommandHandler<ICommand>> mockHandler = new Mock<ICommandHandler<ICommand>>();
    //        Mock<IDocumentSession> documentSessionMock = new Mock<IDocumentSession>();
          //  Mock<IEventStore> eventStoreMock=new Mock<IEventStore>();
            var synchronizer = new Mock<IEventSync>();
          //  NcqrsEnvironment.SetDefault<IEventStore>(eventStoreMock.Object);
         /*   var kernel = new StandardKernel();
            kernel.Bind<ICommandHandler<ICommand>>().ToConstant(mockHandler.Object);*/
            clientProvider.Setup(x => x.ClientSettings).Returns(new ClientSettingsView(new ClientSettingsDocument() { PublicKey = Guid.NewGuid() }));
          //  kernel.Bind<IDocumentSession>().ToConstant(documentSessionMock.Object);
            var output = new EventBrowseView(0, 20, 0, new List<EventBrowseItem>());
            viewRepositoryMock.Setup(x => x.Load<EventBrowseInputModel, EventBrowseView>(It.Is<EventBrowseInputModel>(input => input.PublickKey==null))).Returns(output);
            var events = new ExportImportEvent(clientProvider.Object, synchronizer.Object);
            var result = events.Export();
            synchronizer.Verify(x => x.ReadCompleteQuestionare(), Times.Once());
         //   eventStoreMock.Verify(x => x.ReadByAggregateRoot(), Times.Once());
            //   Assert.AreEqual(result.GetType(), typeof(byte[]));
        }
    }
}
