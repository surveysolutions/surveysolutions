namespace Web.Supervisor.Models
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;

    using Main.Core.Entities.SubEntities;

    /// <summary>
    /// Base chart model
    /// </summary>
    public class ChartModelBase
    {
        public ChartModelBase()
        {
            this.CategoryData = new List<object>();
            this.SubCategoryData = new List<object>();
            this.BarData = new List<object>();
            this.ScatterData = new List<object>();
        }

        /// <summary>
        /// The status colors
        /// </summary>
        public Dictionary<string, string> statusColors = new Dictionary<string, string>()
            {
                { SurveyStatus.Unassign.Name, "#c3325f" },
                { SurveyStatus.Initial.Name, "#049cdb" },
                { SurveyStatus.Redo.Name, "#f89406" },
                { SurveyStatus.Error.Name, "#c83025" },
                { SurveyStatus.Complete.Name, "#018080" },
                { SurveyStatus.Approve.Name, "#46a546" },
                { "Total", "#333333" },
            };

        /// <summary>
        /// The colors
        /// </summary>
        private readonly string[] colors = new[]
            { "#049cdb", "#46a546", "#c3325f", "#f89406", "#673301", "#ffc40d", "#400180", "#018080", "#9d261d" };

        /// <summary>
        /// Gets or sets barData.
        /// </summary>
        public List<object> BarData { get; set; }

        /// <summary>
        /// Gets or sets ScatterData.
        /// </summary>
        public List<object> ScatterData { get; set; }

        /// <summary>
        /// Gets or sets SubCategoryData.
        /// </summary>
        public List<object> SubCategoryData { get; set; }

        /// <summary>
        /// Gets or sets CategoryData.
        /// </summary>
        public List<object> CategoryData { get; set; }

        /// <summary>
        /// Gets color for chart
        /// </summary>
        /// <param name="index">
        /// The index.
        /// </param>
        /// <returns>
        /// Hex color as string
        /// </returns>
        protected string GetColor(int index)
        {
            return this.colors[index % this.colors.Count()];
        }

        /// <summary>
        /// Calculates CategoryData and SubCategoryData by array of PieData
        /// </summary>
        /// <param name="piePreData">
        /// The pie pre data.
        /// </param>
        protected void CalcPieData(IEnumerable<PieData> piePreData)
        {
            foreach (var data in piePreData)
            {
                this.CategoryData.Add(
                    new
                        {
                            category = data.name, 
                            name = data.name, 
                            title = data.title, 
                            y = data.y, 
                            color = data.color
                        });

                for (var j = 0; j < data.data.Length; j++)
                {
                    var brightness = (float)(0.4 - ((float)j / data.data.Length / 2.5f));
                    Color c = ColorTranslator.FromHtml(data.color);
                    var newColor = ControlPaint.Light(c, brightness);
                    this.SubCategoryData.Add(
                        new
                            {
                                category = data.name,
                                name = data.categories[j],
                                y = data.data[j],
                                color = ColorTranslator.ToHtml(newColor)
                            });
                }
            }
        }

        /// <summary>
        /// Pie Data
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:AccessibleFieldsMustBeginWithUpperCaseLetter",
            Justification = "Reviewed. Suppression is OK here.")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate",
               Justification = "Reviewed. Suppression is OK here.")]
        // ReSharper disable InconsistentNaming
        protected class PieData
        {
            /// <summary>
            /// Y -data
            /// </summary>
            public int y;

            /// <summary>
            /// The color of data set
            /// </summary>
            public string color;

            /// <summary>
            /// The name of data set
            /// </summary>
            public string name;

            /// <summary>
            /// The title of data set
            /// </summary>
            public string title;

            /// <summary>
            /// Data categories
            /// </summary>
            public string[] categories;

            /// <summary>
            /// Array of data values
            /// </summary>
            public int[] data;
        }
        // ReSharper restore InconsistentNaming
    }
}