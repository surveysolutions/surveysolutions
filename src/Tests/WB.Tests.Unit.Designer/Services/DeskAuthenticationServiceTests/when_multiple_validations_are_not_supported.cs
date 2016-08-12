using System;
using Machine.Specifications;
using WB.UI.Designer.Implementation.Services;

namespace WB.Tests.Unit.Designer.Services.DeskAuthenticationServiceTests
{
    [Ignore("KP-7529")]
    [Subject(typeof(DeskAuthenticationService))]
    internal class when_getting_return_url
    {
        Establish context = () =>
        {
            service = Create.DeskAuthenticationService(multipassKey: "multipass", returnUrlFormat: "{0}{1}{2}", siteKey: "solutions");
        };

        Because of = () =>
            returnUrl = service.GetReturnUrl(userId, "vasya", "vasya@super.com", new DateTime(2016, 8, 12, 10, 0, 0));

        It should_return_url_as_specified = () =>
            returnUrl.ShouldEqual("solutionsrXRZolc%2BS272B5etIWaY7qTVtWq63eB7TL06ZPTssmAKOhkiLG1kFqfdd1m0X1D9fg0P4CgUuF5WtYbtKVrtgHPZClFTwOZQb1VvdRnkjFnuX%2Bm26LlFRaaw45%2FnLkztRt%2F5otqMKMiawYm3jDdbK9%2FhIH62jQVeR4UgCehmERXCctYlv1FjA2Ekif6hR2FjC2d3QvzgYiRuWZmRftv2OQ%3D%3Dh2NCWaFGzbzhufWpmrelhOv8HgI%3D");

        static DeskAuthenticationService service;
        private static string returnUrl;
        static Guid userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
    }
}