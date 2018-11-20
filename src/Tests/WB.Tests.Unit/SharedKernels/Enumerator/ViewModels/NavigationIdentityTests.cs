using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels
{
    [TestOf(typeof(NavigationIdentity))]
    public class NavigationIdentityTests
    {
        [Test]
        public void when_creating_for_pdf_should_use_static_text_as_anchored_element()
        {
            var targetStaticText = Create.Identity(Id.gA);

            // Act
            var navigationIdentity = NavigationIdentity.CreateForPdfView(targetStaticText);

            // Assert
            Assert.That(navigationIdentity.AnchoredElementIdentity, Is.EqualTo(targetStaticText));
        }
    }
}
