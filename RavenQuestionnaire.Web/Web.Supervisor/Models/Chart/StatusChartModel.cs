// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StatusChartModel.cs" company="">
//   
// </copyright>
// <summary>
//   Chart model for status page
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Web.Supervisor.Models.Chart
{
    using System.Collections.Generic;
    using System.Linq;

    using Core.Supervisor.Views.Status;

    using Main.Core.Entities;

    /// <summary>
    /// Chart model for status page
    /// </summary>
    public class StatusChartModel : ChartModelBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusChartModel"/> class.
        /// </summary>
        /// <param name="view">
        /// The view.
        /// </param>
        public StatusChartModel(StatusView view)
        {
            this.BarData.Add(
                new
                    {
                        name = "Total", 
                        data = view.Items.Select(t => t.Items.Sum(kvp => kvp.Value)).ToArray(), 
                        stack = "total"
                    });
            foreach (TemplateLight header in view.Headers)
            {
                this.BarData.Add(
                    new
                        {
                            name = header.Title.Acronim(), 
                            title = header.Title, 
                            data = view.Items.Select(t => t.Items[header]).ToArray(), 
                            stack = "parts"
                        });
            }

            IEnumerable<PieData> piePreData = this.GetPiePreData(view);

            this.CalcPieData(piePreData);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get pie pre data array 
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <returns>
        /// Array of PieData
        /// </returns>
        private IEnumerable<PieData> GetPiePreData(StatusView model)
        {
            PieData[] piePreData =
                model.Items.Select(
                    (t, index) =>
                    new PieData
                        {
                            y = t.Items.Values.Sum(), 
                            color = GetColor(index), 
                            name = t.User.Name, 
                            title = t.User.Name, 
                            categories = t.Items.Keys.Select(k => k.Title.Acronim()).ToArray(), 
                            data = t.Items.Values.ToArray()
                        }).ToArray();
            return piePreData;
        }

        #endregion
    }
}