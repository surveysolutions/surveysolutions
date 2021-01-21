using StatData.Readers;
using StatData.Writers;

namespace StatData.Converters
{
    /// <summary>
    /// Converter of tab-delimited data to another format using a specified writer
    /// </summary>
    class TabToAnyConverter:DatasetConverter
    {
        /// <summary>
        /// Instantiates a new instance of TabToAnyConverter
        /// </summary>
        /// <param name="writer">Required writer-parameter</param>
        public TabToAnyConverter(IDatasetWriter writer):base(new TabReader(), writer)
        {
        }
    }
}
