#nullable enable
using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Classifications;
using WB.UI.Designer.Controllers.Api.Designer;

namespace WB.Tests.Unit.Designer.Applications
{
    [TestFixture]
    [TestOf(typeof(ClassificationsExceptionFilterAttribute))]
    public class ClassificationsExceptionFilterAttributeTests
    {
        private static ExceptionContext Context(Exception exception) =>
            new ExceptionContext(
                new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()),
                new List<IFilterMetadata>())
            { Exception = exception };

        [Test]
        public void when_undefined_classification_error_should_return_bad_request()
        {
            var context = Context(new ClassificationException("boom"));

            new ClassificationsExceptionFilterAttribute().OnException(context);

            context.Result.Should().BeOfType<BadRequestResult>();
        }

        [Test]
        public void when_no_access_classification_error_should_return_forbid()
        {
            var context = Context(new ClassificationException(ClassificationExceptionType.NoAccess, "no"));

            new ClassificationsExceptionFilterAttribute().OnException(context);

            context.Result.Should().BeOfType<ForbidResult>();
        }

        [Test]
        public void when_other_exception_should_not_set_result()
        {
            var context = Context(new InvalidOperationException());

            new ClassificationsExceptionFilterAttribute().OnException(context);

            context.Result.Should().BeNull();
        }
    }
}

