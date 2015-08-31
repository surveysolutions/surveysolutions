﻿using Machine.Specifications;
using WB.Core.SharedKernels.Enumerator.Services.MaskText;
using It = Machine.Specifications.It;


namespace WB.Tests.Unit.SharedKernels.Enumerator.Services.MaskTextTests
{
    internal class when_filtering_text_dont_consided_to_mask_with_digits_and_letters : MaskTextTestsContext
    {
        Establish context = () =>
        {
            maskedText = CreateMaskedText("*-##-~~~");
        };

        Because of = () =>
            filterResult = maskedText.FilterOnlyMaskedChars("---2-Z3.", 0);

        It should_be_accept_only_AnyChars = () =>
            filterResult.ShouldEqual("-_2Z__");

        static string filterResult;
        static MaskedText maskedText;
    }
}