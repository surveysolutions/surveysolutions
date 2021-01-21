using StatData.Readers;
using StatData.Writers;

namespace StatData.Converters
{
    /// <summary>
    /// Converter of tab-delimited data file to an SPSS system file
    /// </summary>
    public class TabToSpssConverter : DatasetConverter
    {
        /// <summary>
        /// Instantiates a new instance of TabToSpssConverter
        /// </summary>        
        public TabToSpssConverter():base(new TabReader(), new SpssWriter())
        {
        }
    }
}
