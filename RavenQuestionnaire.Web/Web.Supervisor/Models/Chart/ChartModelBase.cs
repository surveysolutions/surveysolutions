namespace Web.Supervisor.Models.Chart
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Linq;

    using Main.Core.Entities.SubEntities;

    /// <summary>
    /// Base chart model
    /// </summary>
    public class ChartModelBase
    {
        #region Fields

        /// <summary>
        /// The status colors
        /// </summary>
        public readonly Dictionary<string, string> StatusColors = new Dictionary<string, string>
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
        private static readonly string[] colors = new[]
            {
               "#049cdb", "#46a546", "#c3325f", "#f89406", "#673301", "#ffc40d", "#400180", "#018080", "#9d261d" 
            };

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ChartModelBase"/> class.
        /// </summary>
        public ChartModelBase()
        {
            this.CategoryData = new List<object>();
            this.SubCategoryData = new List<object>();
            this.BarData = new List<object>();
            this.ScatterData = new List<object>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets barData.
        /// </summary>
        public List<object> BarData { get; set; }

        /// <summary>
        /// Gets or sets CategoryData.
        /// </summary>
        public List<object> CategoryData { get; set; }

        /// <summary>
        /// Gets or sets ScatterData.
        /// </summary>
        public List<object> ScatterData { get; set; }

        /// <summary>
        /// Gets or sets SubCategoryData.
        /// </summary>
        public List<object> SubCategoryData { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Creates color with corrected brightness.
        /// </summary>
        /// <param name="color">
        /// Color to correct.
        /// </param>
        /// <param name="correctionFactor">
        /// The brightness correction factor. Must be between -1 and 1. 
        /// Negative values produce darker colors.
        /// </param>
        /// <returns>
        /// Corrected <see cref="Color"/> structure.
        /// </returns>
        public static Color ChangeColorBrightness(Color color, float correctionFactor)
        {
            float red = color.R;
            float green = color.G;
            float blue = color.B;

            if (correctionFactor < 0)
            {
                correctionFactor = 1 + correctionFactor;
                red *= correctionFactor;
                green *= correctionFactor;
                blue *= correctionFactor;
            }
            else
            {
                red = (255 - red) * correctionFactor + red;
                green = (255 - green) * correctionFactor + green;
                blue = (255 - blue) * correctionFactor + blue;
            }

            return Color.FromArgb(color.A, (int)red, (int)green, (int)blue);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Calculates CategoryData and SubCategoryData by array of PieData
        /// </summary>
        /// <param name="piePreData">
        /// The pie pre data.
        /// </param>
        protected void CalcPieData(IEnumerable<PieData> piePreData)
        {
            foreach (PieData data in piePreData)
            {
                this.CategoryData.Add(new { category = data.name, data.name, data.title, data.y, data.color });

                for (int j = 0; j < data.data.Length; j++)
                {
                    var brightness = (float)(0.4 - ((float)j / data.data.Length / 2.5f));
                    Color c = ColorTranslator.FromHtml(data.color);
                    Color newColor = ChangeColorBrightness(c, brightness);
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
        /// Gets color for chart
        /// </summary>
        /// <param name="index">
        /// The index.
        /// </param>
        /// <returns>
        /// Hex color as string
        /// </returns>
        protected static string GetColor(int index)
        {
            return colors[index % colors.Count()];
        }

        #endregion

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
            #region Fields

            /// <summary>
            /// Data categories
            /// </summary>
            public string[] categories;

            /// <summary>
            /// The color of data set
            /// </summary>
            public string color;

            /// <summary>
            /// Array of data values
            /// </summary>
            public int[] data;

            /// <summary>
            /// The name of data set
            /// </summary>
            public string name;

            /// <summary>
            /// The title of data set
            /// </summary>
            public string title;

            /// <summary>
            /// Y -data
            /// </summary>
            public int y;

            #endregion
        }

        // ReSharper restore InconsistentNaming
    }
}