#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.SharedKernels.Questionnaire.Categories;
using WB.UI.Designer.Controllers.Api.Headquarters;

namespace WB.Tests.Unit.Designer.Api.Headquarters
{
    [TestFixture]
    [TestOf(typeof(HQLookupController))]
    public class HQLookupControllerTests
    {
        [Test]
        public void when_lookup_file_is_missing_should_return_not_found()
        {
            var service = new Mock<ILookupTableService>();
            service.Setup(x => x.GetLookupTableContentFile(It.IsAny<QuestionnaireRevision>(), It.IsAny<Guid>()))
                .Returns((LookupTableContentFile?)null);
            var controller = new HQLookupController(service.Object);

            var result = controller.Get(new QuestionnaireRevision(Guid.NewGuid()), Guid.NewGuid().ToString());

            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public void when_lookup_file_exists_should_return_json_without_type_names()
        {
            var tableId = Guid.NewGuid();
            var revision = new QuestionnaireRevision(Guid.NewGuid());
            var service = new Mock<ILookupTableService>();
            service.Setup(x => x.GetLookupTableContentFile(revision, tableId))
                .Returns(new LookupTableContentFile("file.tab", new byte[] { 1, 2 }));
            var controller = new HQLookupController(service.Object);

            var result = controller.Get(revision, tableId.ToString());

            var content = result.Should().BeOfType<ContentResult>().Subject;
            content.ContentType.Should().StartWith("application/json");
            content.Content.Should().Contain("file.tab").And.NotContain("$type");
        }

        [Test]
        public void when_table_id_is_not_a_guid_should_throw()
        {
            var controller = new HQLookupController(Mock.Of<ILookupTableService>());

            Assert.Throws<FormatException>(() => controller.Get(new QuestionnaireRevision(Guid.NewGuid()), "not-a-guid"));
        }

        [Test]
        public void should_require_authorization()
        {
            typeof(HQLookupController).GetCustomAttributes(typeof(AuthorizeAttribute), true).Should().NotBeEmpty();
        }
    }

    [TestFixture]
    [TestOf(typeof(HQReusableCategoriesController))]
    public class HQReusableCategoriesControllerTests
    {
        [Test]
        public void when_categories_are_missing_should_return_not_found()
        {
            var service = new Mock<IReusableCategoriesService>();
            service.Setup(x => x.GetCategoriesById(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns((IQueryable<CategoriesItem>)null!);
            var controller = new HQReusableCategoriesController(service.Object);

            controller.Get(Guid.NewGuid(), Guid.NewGuid()).Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public void when_categories_exist_should_return_json_content()
        {
            var id = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var items = new List<CategoriesItem>
            {
                new CategoriesItem { Id = 1, Text = "One" },
                new CategoriesItem { Id = 2, Text = "Two", ParentId = 1 }
            };
            var service = new Mock<IReusableCategoriesService>();
            service.Setup(x => x.GetCategoriesById(id, categoryId)).Returns(items.AsQueryable());
            var controller = new HQReusableCategoriesController(service.Object);

            var result = controller.Get(id, categoryId);

            var content = result.Should().BeOfType<ContentResult>().Subject;
            content.ContentType.Should().StartWith("application/json");
            content.Content.Should().Contain("One").And.Contain("Two").And.NotContain("$type");
        }

        [Test]
        public void should_require_authorization()
        {
            typeof(HQReusableCategoriesController).GetCustomAttributes(typeof(AuthorizeAttribute), true).Should().NotBeEmpty();
        }
    }
}


