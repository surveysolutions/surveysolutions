using System;
using NUnit.Framework;

namespace WB.Tests.Unit.Designer.Applications.ImportControllerTests
{
    internal class when_getting_Questionaire_and_request_parameter_is_null : ImportControllerTestContext
    {
        [Test]
        public void should_throw_ArgumentNullException()
        {
            var importController = CreateImportController();
            Assert.Throws<ArgumentNullException>(() =>
                importController.Questionnaire(null));
        }
    }
}
