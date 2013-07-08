namespace Core.CAPI.Views.ExporStatistics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class ExporStatisticsInputModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExporStatisticsInputModel"/> class.
        /// </summary>
        /// <param name="keys">
        /// CQ keys
        /// </param>
        public ExporStatisticsInputModel(IEnumerable<Guid> keys)
        {
            this.Keys = new List<Guid>(keys);
        }

        /// <summary>
        /// Gets or sets Keys.
        /// </summary>
        public List<Guid> Keys { get; set; }
    }
}
