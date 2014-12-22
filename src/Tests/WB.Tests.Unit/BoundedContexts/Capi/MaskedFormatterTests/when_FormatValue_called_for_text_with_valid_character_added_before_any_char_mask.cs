﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.MaskFormatter;

namespace WB.Tests.Unit.BoundedContexts.Capi.MaskedFormatterTests
{
    [Subject(typeof(MaskedFormatter))]
    internal class when_FormatValue_called_for_text_with_valid_character_added_before_any_char_mask
    {
        Establish context = () =>
        {
            maskedFormatter = new MaskedFormatter(mask);
        };

        Because of = () =>
            result = maskedFormatter.FormatValue(value, ref cursorPosition);

        It should_result_be_equal_to_formatted_value = () =>
            result.ShouldEqual("b_-___-____");

        It should_cursor_be_equal_to_1 = () =>
            cursorPosition.ShouldEqual(1);

        private static MaskedFormatter maskedFormatter;
        private static string result;
        private static string mask = "~*-###-~###";
        private static string value = "b__-___-____";
        private static int cursorPosition = 1;
    }
}
