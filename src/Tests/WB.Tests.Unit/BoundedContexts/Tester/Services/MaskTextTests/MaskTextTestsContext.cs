using WB.Core.BoundedContexts.Tester.Services.MaskText;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.Services.MaskTextTests
{
    internal class MaskTextTestsContext
    {
        protected static MaskedText CreateMaskedText(string mask = null)
        {
            return new MaskedText()
            {
                Mask = mask
            };
        }
    }
}