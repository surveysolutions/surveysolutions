using StatData.Readers;
using StatData.Writers;

namespace StatData.Converters
{
    /// <summary>
    /// Converter of tab-delimited data file to a Stata dataset
    /// </summary>
    public class TabToStataConverter :DatasetConverter
    {
        /// <summary>
        /// Instantiates a new instance of TabToStataConverter
        /// </summary>
        public TabToStataConverter():base(new TabReader(), new StataWriter())
        {
        }
    }
}
