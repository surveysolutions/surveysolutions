using StatData.Readers;
using StatData.Writers;

namespace StatData.Converters
{
    /// <summary>
    /// Converter of tab-delimited data file to a Stata dataset (Stata 12 format)
    /// </summary>
    public class TabToStata12Converter :DatasetConverter
    {
        /// <summary>
        /// Instantiates a new instance of TabToStataConverter
        /// </summary>
        public TabToStata12Converter():base(new TabReader(), new Stata12Writer())
        {
        }
    }
}
