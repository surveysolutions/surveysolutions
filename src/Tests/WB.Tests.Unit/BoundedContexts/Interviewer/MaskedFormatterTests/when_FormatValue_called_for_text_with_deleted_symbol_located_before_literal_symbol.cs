﻿using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.MaskFormatter;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.MaskedFormatterTests
{
    [Subject(typeof(MaskedFormatter))]
    internal class when_FormatValue_called_for_text_with_deleted_symbol_located_before_literal_symbol
    {
        private Establish context = () =>
        {
            maskedFormatter = new MaskedFormatter(mask);
        };

        private Because of = () =>
            result = maskedFormatter.FormatValue(value, ref cursorPosition);

        private It should_result_be_equal_to_formatted_value = () =>
            result.ShouldEqual("w1-234-____");

        private It should_cursor_be_equal_to_6 = () =>
            cursorPosition.ShouldEqual(6);

        private static MaskedFormatter maskedFormatter;
        private static string result;
        private static string mask = "~*-###-~###";
        private static string value = "w1-234____";
        private static int cursorPosition = 6;
    }
}
