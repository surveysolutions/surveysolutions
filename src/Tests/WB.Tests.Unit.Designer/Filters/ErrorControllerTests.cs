using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using WB.UI.Designer.Controllers;
using WB.UI.Shared.Web.Attributes;

namespace WB.Tests.Unit.Designer.Filters;

[TestFixture]
[TestOf(typeof(ErrorController))]
public class ErrorControllerTests
{
    [Test]
    public void should_mark_error_controller_with_no_transaction()
    {
        typeof(ErrorController)
            .GetCustomAttributes(typeof(NoTransactionAttribute), inherit: true)
            .OfType<NoTransactionAttribute>()
            .Should().ContainSingle();
    }
}
