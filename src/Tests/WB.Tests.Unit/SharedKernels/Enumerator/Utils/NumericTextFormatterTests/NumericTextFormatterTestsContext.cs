using WB.Core.SharedKernels.Enumerator.Utils;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Utils.NumericTextFormatterTests
{
    public class NumericTextFormatterTestsContext
    {
        public static NumericTextFormatter CreateNumericTextFormatter(NumericTextFormatterSettings settings = null)
        {
            return new NumericTextFormatter(settings: settings ?? new NumericTextFormatterSettings());
        }

        protected class FilterFormattedParam
        {
            public string SourceText { get; set; }
            public string AddedText { get; set; }
            public int InsertToIndex { get; set; }
        }
    }
}