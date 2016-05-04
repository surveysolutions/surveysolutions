using WB.Core.SharedKernels.Enumerator.Services.MaskText;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Services.MaskTextTests
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