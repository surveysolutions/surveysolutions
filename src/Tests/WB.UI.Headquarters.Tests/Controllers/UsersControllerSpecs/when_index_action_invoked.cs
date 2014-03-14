using System.Linq;
using System.Web.Mvc;
using Machine.Specifications;
using Raven.Client;
using Raven.Client.Embedded;
using WB.Core.BoundedContexts.Headquarters.Authentication;
using WB.Core.BoundedContexts.Headquarters.Authentication.Models;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Models;
using It = Machine.Specifications.It;

namespace WB.UI.Headquarters.Tests.Controllers.UsersControllerSpecs
{
    [Subject(typeof(UsersController))]
    public class when_index_action_invoked
    {
        static UsersController controller;
        static ActionResult actionResult;
        static EmbeddableDocumentStore store;

        Establish context = () =>
        {
            store = new EmbeddableDocumentStore() { RunInMemory = true };
            store.Initialize();

            IDocumentSession documentSession = store.OpenSession();

            documentSession.Store(new ApplicationUser("11"){ IsAdministrator = true });
            documentSession.SaveChanges();
            controller = Create.UsersController(storageProvider: store);
        };

        Because of = () => actionResult = controller.Index();

        It should_fill_view_model = () => {
            var model = actionResult.GetModel<UsersListModel>();
            model.Users.Count.ShouldEqual(1);
            model.Users.First().Id.ShouldEqual("11");
            model.Users.First().Role.ShouldEqual(ApplicationRoles.Administrator);
        };

        Cleanup after = () => store.Dispose();
    }
}