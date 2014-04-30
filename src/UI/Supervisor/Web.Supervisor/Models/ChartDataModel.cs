namespace Web.Supervisor.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// Class for display visual chart
    /// </summary>
    public class ChartDataModel
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ChartDataModel"/> class.
        /// </summary>
        public ChartDataModel()
        {
            this.Data = new Dictionary<string, int>();
            this.DataItems = new List<ChartDataItem>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChartDataModel"/> class.
        /// </summary>
        /// <param name="title">
        /// The title.
        /// </param>
        public ChartDataModel(string title)
            : this()
        {
            this.Title = title;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets Id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets Title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets Data.
        /// </summary>
        public Dictionary<string, int> Data { get; set; }

        /// <summary>
        /// Gets or sets DataItems.
        /// </summary>
        public List<ChartDataItem> DataItems { get; set; }

        #endregion
    }

    public class ChartDataItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChartDataItem"/> class.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="total">
        /// The total.
        /// </param>
        /// <param name="approved">
        /// The approved.
        /// </param>
        public ChartDataItem(string name, int total, int approved)
        {
            this.Name = name;
            this.Total = total;
            this.Approved = approved;
        }

        /// <summary>
        /// Gets or sets Name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets Total.
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// Gets or sets Approved.
        /// </summary>
        public int Approved { get; set; }
    }
}