using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Raven.Client;
using Raven.Client.Embedded;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Views.User;

namespace RavenQuestionnaire.Core.Tests.Views.User
{
    [TestFixture]
    public class UserViewFactoryTest
    {
        [Test]
        public void LoadByExistingUserId_UserViewIsReturned()
        {
            //var docMock = new Mock<IDenormalizerStorage<UserDocument>>();

            //UserViewInputModel input = new UserViewInputModel("user_id");
            //UserDocument expected = new UserDocument() { Id = "userdocuments/user_id", Email = "email@test.com", Password = "1234", UserName = "test" };

            //docMock.Setup(x => x.Load<UserDocument>("userdocuments/user_id")).Returns(expected);
            //UserViewFactory factory = new UserViewFactory(docMock);

            //UserView result = factory.Load(input);

            //docMock.Verify(x => x.Load<UserDocument>("userdocuments/user_id"));
            //Assert.True(result.UserId == "user_id" && result.Email == "email@test.com" && result.Password == "1234" &&
            //            result.UserName == "test");
        }
        [Test]
        public void LoadByExistingUserIdButUserMarketAsDeleted_NullIsReturned()
        {
            //var docMock = new Mock<IDenormalizerStorage<UserDocument>>();
            //UserViewInputModel input = new UserViewInputModel("user_id");
            //UserDocument expected = new UserDocument()
            //                            {
            //                                Id = "userdocuments/user_id",
            //                                Email = "email@test.com",
            //                                Password = "1234",
            //                                UserName = "test",
            //                                IsDeleted = true
            //                            };
            //docMock.Setup(x => x.Load<UserDocument>("userdocuments/user_id")).Returns(expected);
            //UserViewFactory factory = new UserViewFactory(documentSesionMock.Object);

            //UserView result = factory.Load(input);

            //docMock.Verify(x => x.Load<UserDocument>("userdocuments/user_id"));
            //Assert.True(result == null);
        }
        [Test]
        public void LoadByNotExistingUserId_NullIsReturned()
        {
            //Mock<IDocumentSession> documentSesionMock = new Mock<IDocumentSession>();
            //UserViewInputModel input = new UserViewInputModel("user_id");
            //UserViewFactory factory = new UserViewFactory(documentSesionMock.Object);

            //UserView result = factory.Load(input);

            //docMock.Verify(x => x.Load<UserDocument>("userdocuments/user_id"));
            //Assert.True(result == null);
        }


        [Test]
        public void LoadByExistingUserName_UserViewIsReturned()
        {
        //    UserViewInputModel input = new UserViewInputModel("user_name", null);
        //    UserDocument expected = new UserDocument()
        //                                {
        //                                    Id = "userdocuments/user_id",
        //                                    Email = "email@test.com",
        //                                    Password = "1234",
        //                                    UserName = "user_name"
        //                                };
            
        //    IEnumerable<UserDocument> expectedCollection = new[] {expected};
        //  /*  var ravenQueryableMock = new Mock<IRavenQueryable<UserDocument>>();
        //    ravenQueryableMock.Setup(x => x.GetEnumerator()).Returns(() => expectedCollection.GetEnumerator());
        //    ravenQueryableMock.Setup(x => x.Customize(It.IsAny<Action<Object>>()).GetEnumerator()).Returns(
        //        () => expectedCollection.GetEnumerator());

            
            
        //    documentSesionMock.Setup(x => x.Query<UserDocument>()).Returns(ravenQueryableMock.Object);*/
        //    IDocumentStore store = new EmbeddableDocumentStore() {RunInMemory = true};
        //    store.Initialize();
        //    IDocumentSession session = store.OpenSession();
        //    session.Store(expected);
        //    session.SaveChanges();
        //    UserViewFactory factory = new UserViewFactory(Mock<IDocumentSession> documentSesionMock = new Mock<IDocumentSession>(););

        //    UserView result = factory.Load(input);

        ////    documentSesionMock.Verify(x => x.Query<UserDocument>());
        //    Assert.True(result.UserId == "user_id" && result.Email == "email@test.com" && result.Password == "1234" &&
        //                result.UserName == "user_name");
        }

        /*    [Test]
        public void LoadByExistingUserNameButUserMarketAsDEleted_NullIsReturned()
        {
            Mock<IDocumentSession> documentSesionMock = new Mock<IDocumentSession>();
            UserViewInputModel input = new UserViewInputModel("user_id");
            UserDocument expected = new UserDocument()
            {
                Id = "userdocuments/user_id",
                Email = "email@test.com",
                Password = "1234",
                UserName = "test",
                IsDeleted = true
            };
            documentSesionMock.Setup(x => x.Load<UserDocument>("userdocuments/user_id")).Returns(expected);
            UserViewFactory factory = new UserViewFactory(documentSesionMock.Object);

            UserView result = factory.Load(input);

            documentSesionMock.Verify(x => x.Load<UserDocument>("userdocuments/user_id"));
            Assert.True(result == null);
        }
        [Test]
        public void LoadByNotExistingUserName_NullIsReturned()
        {
            Mock<IDocumentSession> documentSesionMock = new Mock<IDocumentSession>();
            UserViewInputModel input = new UserViewInputModel("user_id");
            UserViewFactory factory = new UserViewFactory(documentSesionMock.Object);

            UserView result = factory.Load(input);

            documentSesionMock.Verify(x => x.Load<UserDocument>("userdocuments/user_id"));
            Assert.True(result == null);
        }*/
    }
}
